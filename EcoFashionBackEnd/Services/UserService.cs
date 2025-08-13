using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Auth;
using EcoFashionBackEnd.Dtos.User;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Exceptions;
using EcoFashionBackEnd.Helpers;
using EcoFashionBackEnd.Repositories;
using Microsoft.IdentityModel.Tokens;
using EcoFashionBackEnd.Settings;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EcoFashionBackEnd.Services
{
    public class UserService
    {
        private readonly IRepository<User, int> _userRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(
            IRepository<User, int> userRepository,
            IMapper mapper,
            IRepository<UserRole, int> userRoleRepository,
            AppDbContext dbContext,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _dbContext = dbContext;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
        {
            // Tìm user theo email
            var user = await _dbContext.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new UserNotFoundException("Tài khoản không tồn tại.");
            }

            // Kiểm tra mật khẩu (so sánh hash)
            if (user.PasswordHash != SecurityUtil.Hash(request.PasswordHash))
            {
                throw new BadRequestException("Mật khẩu không chính xác.");
            }

            // Kiểm tra trạng thái tài khoản
            //if (user.Status != UserStatus.Active)
            //{
            //    throw new BadRequestException("Tài khoản không hoạt động.");
            //}

            var expiresAt = DateTime.Now.AddDays(7);
            var jwtKey = _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var token = user.GenerateJsonWebToken(jwtKey, DateTime.Now);

            var response = new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    UserId = user.UserId,
                    FullName = user.FullName ?? "",
                    Email = user.Email ?? "",
                    Phone = user.Phone,
                    Username = user.Username,
                    Role = user.UserRole?.RoleName ?? "",
                    RoleId = user.RoleId,
                    Status = user.Status.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };

            return response;
        }
        public async Task<UserModel?> GetUserById(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<UserModel>(user);
        }

        // Dang ky tai khoan
        // Đăng ký người dùng mới
        public async Task<SignupResponse> SignupAsync(SignupRequest req)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user is not null)
            {
                throw new BadRequestException("Email đã được sử dụng.");
            }

            var otp = new Random().Next(100000, 999999);
            var newUser = new User
            {
                Email = req.Email,
                PasswordHash = SecurityUtil.Hash(req.Password),
                FullName = req.fullname,
                Username = req.username,
                Phone = req.Phone,
                OTPCode = otp.ToString(),
                OTPExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Status = Entities.UserStatus.Pending,
                RoleId = 4,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);
            var res = await _dbContext.SaveChangesAsync();

            if (res <= 0)
            {
                throw new InvalidOperationException("Không thể tạo tài khoản. Vui lòng thử lại.");
            }

            var mailData = new MailData()
            {
                EmailToId = newUser.Email ?? "",
                EmailToName = newUser.FullName ?? "",
                EmailBody = $"Your OTP code is: {otp}",
                EmailSubject = "Xác thực tài khoản EcoFashion"
            };

            var emailResult = await _emailService.SendEmailAsync(mailData);
            if (!emailResult)
            {
                throw new BadRequestException("Không thể gửi email xác thực. Vui lòng thử lại.");
            }

            return new SignupResponse
            {
                Success = true,
                Message = "Đăng ký thành công! Vui lòng kiểm tra email để xác thực tài khoản.",
                Email = newUser.Email ?? ""
            };
        }

        public async Task<bool> VerifyOTPAsync(VerifyOTPRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null)
            {
                throw new BadRequestException("Tài khoản không tồn tại.");
            }

            if (user.Status == Entities.UserStatus.Active)
            {
                throw new BadRequestException("Tài khoản đã được xác thực.");
            }

            if (string.IsNullOrEmpty(user.OTPCode) || user.OTPCode != request.OTPCode)
            {
                throw new BadRequestException("Mã OTP không chính xác.");
            }

            if (user.OTPExpiresAt == null || user.OTPExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("Mã OTP đã hết hạn.");
            }

            user.Status = Entities.UserStatus.Active;
            user.OTPCode = null;
            user.OTPExpiresAt = null;
            user.LastUpdatedAt = DateTime.UtcNow;

            _dbContext.Users.Update(user);
            var updateResult = await _dbContext.SaveChangesAsync();

            return updateResult > 0;
        }

        public async Task<bool> ResendOTPAsync(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                throw new BadRequestException("Tài khoản không tồn tại.");
            }

            if (user.Status == Entities.UserStatus.Active)
            {
                throw new BadRequestException("Tài khoản đã được xác thực.");
            }

            var otp = new Random().Next(100000, 999999);
            user.OTPCode = otp.ToString();
            user.OTPExpiresAt = DateTime.UtcNow.AddMinutes(10);
            user.LastUpdatedAt = DateTime.UtcNow;

            _dbContext.Users.Update(user);
            var updateResult = await _dbContext.SaveChangesAsync();

            if (updateResult <= 0)
            {
                throw new Exception("Không thể cập nhật mã OTP.");
            }

            var mailData = new MailData()
            {
                EmailToId = user.Email ?? "",
                EmailToName = user.FullName ?? "",
                EmailBody = $"Your new OTP code is: {otp}",
                EmailSubject = "Gửi lại mã xác thực EcoFashion"
            };

            var emailResult = await _emailService.SendEmailAsync(mailData);
            if (!emailResult)
            {
                throw new BadRequestException("Không thể gửi email xác thực.");
            }

            return true;
        }

        public async Task<UserInfo?> GetUserProfileAsync(int userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new UserNotFoundException("Không tìm thấy thông tin user.");
            }

            return new UserInfo
            {
                UserId = user.UserId,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone,
                Username = user.Username,
                Role = user.UserRole?.RoleName ?? "",
                RoleId = user.RoleId,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}
