using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using EcoFashionBackEnd.Helpers;

namespace EcoFashionBackEnd.Services
{
    public class CustomerService
    {
        private readonly IRepository<User, int> _customerRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public CustomerService(
            IRepository<User, int> customerRepository,
            IMapper mapper,
            AppDbContext dbContext)
        {
            _customerRepository = customerRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserModel>> GetAllCustomers()
        {
            var result = _customerRepository.GetAll().ToList();
            return _mapper.Map<List<UserModel>>(result);
        }

        public async Task<int> CreateCustomer(CreateCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) &&
                string.IsNullOrWhiteSpace(request.Phone) &&
                string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("At least one of Username, Phone, or Email must be provided.");
            }
            var customer = new User
            {
                Email = request.Email,
                Username = request.Username,
                Phone = request.Phone,
               // Status = UserStatus.Active,
                PasswordHash = SecurityUtil.Hash(request.Password),
                RoleId = 4,
            };

            await _customerRepository.AddAsync(customer);
            var result = await _dbContext.SaveChangesAsync();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to create new customer.");
            }

            return customer.UserId;
        }
        public async Task<UserModel?> GetCustomerById(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return _mapper.Map<UserModel>(customer);
        }
        public async Task<bool> UpdateCustomer(int id, UpdateCustomerRequest request)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null)
                return false;

            existingCustomer.Email = request.Email ?? existingCustomer.Email;
            existingCustomer.Phone = request.Phone ?? existingCustomer.Phone;
            existingCustomer.Username = request.Username ?? existingCustomer.Username;
            existingCustomer.FullName = request.FullName ?? existingCustomer.FullName;
            if (!string.IsNullOrEmpty(request.Password))
                existingCustomer.PasswordHash = SecurityUtil.Hash(request.Password);

            _customerRepository.Update(existingCustomer);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeactiveCustomer(int id)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null) return false;
            //existingCustomer.Status = UserStatus.Inactive;
            existingCustomer.LastUpdatedAt = DateTime.UtcNow;
            _customerRepository.Update(existingCustomer);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
