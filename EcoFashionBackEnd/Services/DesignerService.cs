using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcoFashionBackEnd.Services
{
    public class DesignerService
    {
        private readonly IRepository<Designer, Guid> _designerRepository;
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public DesignerService(
            IRepository<Designer, Guid> designerRepository,
            IRepository<Supplier, Guid> supplierRepository,
            IMapper mapper,
            AppDbContext dbContext)
        {
            _designerRepository = designerRepository;
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }
      
        public async Task<List<DesignerSummaryModel>> GetPublicDesigners(int page = 1, int pageSize = 12)
        {
            var query = _designerRepository.GetAll().Include(d => d.User);

            var designers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return designers.Select(d => new DesignerSummaryModel
            {
                DesignerId = d.DesignerId,
                DesignerName = d.DesignerName,
                AvatarUrl = d.AvatarUrl,
                Bio = d.Bio,
                BannerUrl = d.BannerUrl,
                Rating = d.Rating,
                ReviewCount = d.ReviewCount,
                CreatedAt = d.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get designer public profile for landing page
        /// </summary>
        public async Task<DesignerPublicModel?> GetDesignerPublicProfile(Guid id)
        {
            var designer = await _designerRepository.GetAll().Include(d => d.User).FirstOrDefaultAsync(d => d.DesignerId == id && d.Status == "Active");

            if (designer == null) return null;

            return new DesignerPublicModel
            {
                DesignerId = designer.DesignerId,
                DesignerName = designer.DesignerName,
                AvatarUrl = designer.AvatarUrl,
                Bio = designer.Bio,
                SpecializationUrl = designer.SpecializationUrl,
                PortfolioFiles = designer.PortfolioFiles,
                BannerUrl = designer.BannerUrl,
                Email = designer.Email, // Có thể ẩn tùy business logic
                PhoneNumber = designer.PhoneNumber, // Có thể ẩn tùy business logic
                Address = designer.Address,
                Rating = designer.Rating,
                ReviewCount = designer.ReviewCount,
                Certificates = designer.Certificates,
                CreatedAt = designer.CreatedAt,
                UserFullName = designer.User?.FullName
            };
        }

        /// <summary>
        /// Search public designers by keyword
        /// </summary>
        public async Task<List<DesignerSummaryModel>> SearchPublicDesigners(string? keyword, int page = 1, int pageSize = 12)
        {
            IQueryable<Designer> query = _designerRepository.GetAll().Include(d => d.User);
            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(d =>
                    (d.DesignerName != null && d.DesignerName.ToLower().Contains(lowerKeyword)) ||
                    (d.Bio != null && d.Bio.ToLower().Contains(lowerKeyword)) ||
                    (d.User != null && d.User.FullName != null && d.User.FullName.ToLower().Contains(lowerKeyword))
                );
            }
            var designers = await query
                .OrderByDescending(d => d.Rating ?? 0)
                .ThenByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return designers.Select(d => new DesignerSummaryModel
            {
                DesignerId = d.DesignerId,
                DesignerName = d.DesignerName,
                AvatarUrl = d.AvatarUrl,
                Bio = d.Bio,
                BannerUrl = d.BannerUrl,
                Rating = d.Rating,
                ReviewCount = d.ReviewCount,
                CreatedAt = d.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get featured designers for homepage (top rated/newest)
        /// </summary>
        public async Task<List<DesignerSummaryModel>> GetFeaturedDesigners(int count = 6)
        {
            var designers = await _designerRepository.GetAll()
                .Include(d => d.User)
                .Where(d => d.Status == "Active")
                .OrderByDescending(d => d.Rating ?? 0)
                .ThenByDescending(d => d.ReviewCount ?? 0)
                .ThenByDescending(d => d.CreatedAt)
                .Take(count)
                .ToListAsync();
            return designers.Select(d => new DesignerSummaryModel
            {
                DesignerId = d.DesignerId,
                DesignerName = d.DesignerName,
                AvatarUrl = d.AvatarUrl,
                Bio = d.Bio,
                BannerUrl = d.BannerUrl,
                Rating = d.Rating,
                ReviewCount = d.ReviewCount,
                CreatedAt = d.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get designer full profile (for authenticated user - owner/admin)
        /// </summary>
        public async Task<DesignerModel?> GetDesignerFullProfile(Guid id)
        {
            var designer = await _designerRepository.GetAll()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DesignerId == id);
            return designer != null ? _mapper.Map<DesignerModel>(designer) : null;
        }

        // Existing Admin/Management Methods

        public async Task<IEnumerable<DesignerModel>> GetAllDesigners()
        {
            var result = await _designerRepository.GetAll()
                .Include(d => d.User)
                .ToListAsync();
            return _mapper.Map<List<DesignerModel>>(result);
        }

        public async Task<Guid> CreateDesigner(CreateDesignerRequest request)
        {
            var designer = new Designer
            {
                DesignerId = Guid.NewGuid(),
                UserId = request.UserId,
                DesignerName = request.DesignerName,
                AvatarUrl = request.AvatarUrl,
                PortfolioUrl = request.PortfolioUrl,
                PortfolioFiles = request.PortfolioFiles,
                BannerUrl = request.BannerUrl,
                SpecializationUrl = request.SpecializationUrl,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                TaxNumber = request.TaxNumber,
                IdentificationNumber = request.IdentificationNumber,
                //IdentificationPictureFront = request.IdentificationPictureFront,
                //IdentificationPictureBack = request.IdentificationPictureBack,
                Certificates = request.Certificates,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            await _designerRepository.AddAsync(designer);
            var result = await _dbContext.SaveChangesAsync();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to create new designer.");
            }

            return designer.DesignerId;
        }

        public async Task<DesignerModel?> GetDesignerById(Guid id)
        {
            var designer = await _designerRepository.GetAll()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DesignerId == id);
            return _mapper.Map<DesignerModel>(designer);
        }



        public async Task<bool> UpdateDesigner(Guid id, UpdateDesignerRequest request)
        {
            var existingDesigner = await _designerRepository.GetByIdAsync(id);
            if (existingDesigner == null)
            {
                return false;
            }

            existingDesigner.DesignerName = request.DesignerName ?? existingDesigner.DesignerName;
            existingDesigner.AvatarUrl = request.AvatarUrl ?? existingDesigner.AvatarUrl;
            existingDesigner.PortfolioUrl = request.PortfolioUrl ?? existingDesigner.PortfolioUrl;
            existingDesigner.PortfolioFiles = request.PortfolioFiles ?? existingDesigner.PortfolioFiles;
            existingDesigner.BannerUrl = request.BannerUrl ?? existingDesigner.BannerUrl;
            existingDesigner.SpecializationUrl = request.SpecializationUrl ?? existingDesigner.SpecializationUrl;
            existingDesigner.Email = request.Email ?? existingDesigner.Email;
            existingDesigner.PhoneNumber = request.PhoneNumber ?? existingDesigner.PhoneNumber;
            existingDesigner.Address = request.Address ?? existingDesigner.Address;
            existingDesigner.TaxNumber = request.TaxNumber ?? existingDesigner.TaxNumber;
            existingDesigner.IdentificationNumber = request.IdentificationNumber ?? existingDesigner.IdentificationNumber;
            //existingDesigner.IdentificationPictureFront = request.IdentificationPictureFront ?? existingDesigner.IdentificationPictureFront;
            //existingDesigner.IdentificationPictureBack = request.IdentificationPictureBack ?? existingDesigner.IdentificationPictureBack;
            existingDesigner.Certificates = request.Certificates ?? existingDesigner.Certificates;
            existingDesigner.UpdatedAt = DateTime.UtcNow;

            _designerRepository.Update(existingDesigner);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDesigner(Guid id)
        {
            var existingDesigner = await _designerRepository.GetByIdAsync(id);
            if (existingDesigner == null)
            {
                return false;
            }

            existingDesigner.Status = "Inactive";
            existingDesigner.UpdatedAt = DateTime.UtcNow;
            _designerRepository.Update(existingDesigner);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DesignerModel>> FilterDesigners(string? designerName, string? email, string? phoneNumber, string? status)
        {
            IQueryable<Designer> query = _designerRepository.GetAll().Include(d => d.User);
            if (!string.IsNullOrEmpty(designerName))
            {
                query = query.Where(d => d.DesignerName == designerName);
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(d => d.Email == email);
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(d => d.PhoneNumber == phoneNumber);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(d => d.Status == status);
            }
            var result = await query.ToListAsync();
            return _mapper.Map<List<DesignerModel>>(result);
        }

        public async Task<IEnumerable<DesignerModel>> SearchDesigners(string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return await GetAllDesigners(); 
            }
            var lowerKeyword = keyword.ToLower();
            var result = await _designerRepository.GetAll()
                .Include(d => d.User)
                .Where(d =>
                    (d.DesignerName != null && d.DesignerName.ToLower().Contains(lowerKeyword)) ||
                    (d.Email != null && d.Email.ToLower().Contains(lowerKeyword)) ||
                    (d.PhoneNumber != null && d.PhoneNumber.ToLower().Contains(lowerKeyword))
                )
                .ToListAsync();
            return _mapper.Map<List<DesignerModel>>(result);
        }

        // Supplier Connection Methods

        public async Task<FollowedSupplierResponse?> ConnectWithSupplier(Guid designerId, Guid supplierId)
        {
            var designerExists = await _designerRepository.GetByIdAsync(designerId);
            var supplierExists = await _supplierRepository.GetByIdAsync(supplierId);

            if (designerExists == null || supplierExists == null)
            {
                return null; 
            }

            var existingConnection = await _dbContext.SavedSuppliers
                .FirstOrDefaultAsync(s => s.DesignerId == designerId && s.SupplierId == supplierId);

            if (existingConnection != null)
            {
                return null; 
            }

            var savedSupplier = new SavedSupplier
            {
                DesignerId = designerId,
                SupplierId = supplierId
            };

            await _dbContext.SavedSuppliers.AddAsync(savedSupplier);
            var result = await _dbContext.SaveChangesAsync();

            if (result > 0)
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier != null)
                {
                    return new FollowedSupplierResponse
                    {
                        SupplierId = supplier.SupplierId,
                        SupplierName = supplier.SupplierName,
                        PortfolioUrl = supplier.PortfolioUrl
                    };
                }
            }

            return null;
        }

        public async Task<IEnumerable<SupplierModel>> GetFollowedSuppliers(Guid designerId)
        {
            var followedSuppliers = await _dbContext.SavedSuppliers
                .Where(ss => ss.DesignerId == designerId)
                .Include(ss => ss.Supplier)
                .ThenInclude(s => s.User)
                .Select(ss => ss.Supplier)
                .ToListAsync();

            return _mapper.Map<List<SupplierModel>>(followedSuppliers);
        }

        public async Task<Guid?> GetDesignerIdByUserId(int userId)
        {
            var designer = await _designerRepository.GetAll()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            return designer?.DesignerId;
        }

        /// <summary>
        /// Get designer full profile by user ID
        /// </summary>
        public async Task<DesignerModel?> GetDesignerByUserId(int userId)
        {
            var designer = await _designerRepository.GetAll()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);
            return designer != null ? _mapper.Map<DesignerModel>(designer) : null;
        }

       
        public async Task<bool> RemoveFollowedSupplier(Guid designerId, Guid supplierId)
        {
            var savedSupplier = await _dbContext.SavedSuppliers
                .FirstOrDefaultAsync(ss => ss.DesignerId == designerId && ss.SupplierId == supplierId);

            if (savedSupplier == null)
            {
                return false;
            }

            _dbContext.SavedSuppliers.Remove(savedSupplier);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }
    }
}
