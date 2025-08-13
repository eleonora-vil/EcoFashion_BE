using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Exceptions;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcoFashionBackEnd.Services
{
    public class SupplierService
    {
        private readonly IRepository<Supplier, Guid> _supplierRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public SupplierService(
            IRepository<Supplier, Guid> supplierRepository,
            IMapper mapper,
            AppDbContext dbContext)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        // Landing Pages Methods - Public APIs

        /// <summary>
        /// Get public suppliers for listing page (with pagination)
        /// </summary>
        public async Task<List<SupplierSummaryModel>> GetPublicSuppliers(int page = 1, int pageSize = 12)
        {
            var query = _supplierRepository.GetAll().Include(s => s.User);

            var suppliers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return suppliers.Select(s => new SupplierSummaryModel
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                AvatarUrl = s.AvatarUrl,
                Bio = s.Bio,
                BannerUrl = s.BannerUrl,
                Rating = s.Rating,
                ReviewCount = s.ReviewCount,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get supplier public profile for landing page
        /// </summary>
        public async Task<SupplierPublicModel?> GetSupplierPublicProfile(Guid id)
        {
            var supplier = await _supplierRepository.GetAll().Include(s => s.User).FirstOrDefaultAsync(s => s.SupplierId == id && s.Status == "Active");

            if (supplier == null) return null;

            return new SupplierPublicModel
            {
                SupplierId = supplier.SupplierId,
                SupplierName = supplier.SupplierName,
                AvatarUrl = supplier.AvatarUrl,
                Bio = supplier.Bio,
                SpecializationUrl = supplier.SpecializationUrl,

                PortfolioFiles = supplier.PortfolioFiles,
                BannerUrl = supplier.BannerUrl,
                Email = supplier.Email, // Có thể ẩn tùy business logic
                PhoneNumber = supplier.PhoneNumber, // Có thể ẩn tùy business logic
                Address = supplier.Address,
                Rating = supplier.Rating,
                ReviewCount = supplier.ReviewCount,
                Certificates = supplier.Certificates,
                CreatedAt = supplier.CreatedAt,
                UserFullName = supplier.User?.FullName
            };
        }

        /// <summary>
        /// Search public suppliers by keyword
        /// </summary>
        public async Task<List<SupplierSummaryModel>> SearchPublicSuppliers(string? keyword, int page = 1, int pageSize = 12)
        {
            IQueryable<Supplier> query = _supplierRepository.GetAll().Include(s => s.User);

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(s =>
                    (s.SupplierName != null && s.SupplierName.ToLower().Contains(lowerKeyword)) ||
                    (s.Bio != null && s.Bio.ToLower().Contains(lowerKeyword)) ||
                    (s.User != null && s.User.FullName != null && s.User.FullName.ToLower().Contains(lowerKeyword))
                );
            }

            var suppliers = await query
                .OrderByDescending(s => s.Rating ?? 0)
                .ThenByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return suppliers.Select(s => new SupplierSummaryModel
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                AvatarUrl = s.AvatarUrl,
                Bio = s.Bio,
                BannerUrl = s.BannerUrl,
                Rating = s.Rating,
                ReviewCount = s.ReviewCount,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get featured suppliers for homepage (top rated/newest)
        /// </summary>
        public async Task<List<SupplierSummaryModel>> GetFeaturedSuppliers(int count = 6)
        {
            var suppliers = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.Rating ?? 0)
                .ThenByDescending(s => s.ReviewCount ?? 0)
                .ThenByDescending(s => s.CreatedAt)
                .Take(count)
                .ToListAsync();

            return suppliers.Select(s => new SupplierSummaryModel
            {
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                AvatarUrl = s.AvatarUrl,
                Bio = s.Bio,
                BannerUrl = s.BannerUrl,
                Rating = s.Rating,
                ReviewCount = s.ReviewCount,
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Get supplier full profile (for authenticated user - owner/admin)
        /// </summary>
        public async Task<SupplierModel?> GetSupplierFullProfile(Guid id)
        {
            var supplier = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SupplierId == id);
            return supplier != null ? _mapper.Map<SupplierModel>(supplier) : null;
        }

        /// <summary>
        /// Get supplier ID by user ID (for user profile lookup)
        /// </summary>
        public async Task<Guid?> GetSupplierIdByUserId(int userId)
        {
            var supplier = await _supplierRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == userId);
            return supplier?.SupplierId;
        }

        /// <summary>
        /// Get supplier full profile by user ID
        /// </summary>
        public async Task<SupplierModel?> GetSupplierByUserId(int userId)
        {
            var supplier = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
            return supplier != null ? _mapper.Map<SupplierModel>(supplier) : null;
        }

        // Existing Admin/Management Methods

        public async Task<IEnumerable<SupplierModel>> GetAllSuppliers()
        {
            var result = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .ToListAsync();
            return _mapper.Map<List<SupplierModel>>(result);
        }

        public async Task<Guid> CreateSupplier(CreateSupplierRequest request)
        {
            var supplier = new Supplier
            {
                SupplierId = Guid.NewGuid(),
                UserId = request.UserId,
                SupplierName = request.SupplierName,
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
                IdentificationPictureFront = request.IdentificationPictureFront,
                IdentificationPictureBack = request.IdentificationPictureBack,
                Certificates = request.Certificates,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            await _supplierRepository.AddAsync(supplier);
            var result = await _dbContext.SaveChangesAsync();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to create new supplier.");
            }

            return supplier.SupplierId;
        }

        public async Task<SupplierModel?> GetSupplierById(Guid id)
        {
            var supplier = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SupplierId == id);
            return _mapper.Map<SupplierModel>(supplier);
        }

        public async Task<bool> UpdateSupplier(Guid id, UpdateSupplierRequest request)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(id);
            if (existingSupplier == null)
            {
                return false;
            }

            existingSupplier.SupplierName = request.SupplierName ?? existingSupplier.SupplierName;
            existingSupplier.AvatarUrl = request.AvatarUrl ?? existingSupplier.AvatarUrl;
            existingSupplier.PortfolioUrl = request.PortfolioUrl ?? existingSupplier.PortfolioUrl;
            existingSupplier.PortfolioFiles = request.PortfolioFiles ?? existingSupplier.PortfolioFiles;
            existingSupplier.BannerUrl = request.BannerUrl ?? existingSupplier.BannerUrl;
            existingSupplier.SpecializationUrl = request.SpecializationUrl ?? existingSupplier.SpecializationUrl;
            existingSupplier.Email = request.Email ?? existingSupplier.Email;
            existingSupplier.PhoneNumber = request.PhoneNumber ?? existingSupplier.PhoneNumber;
            existingSupplier.Address = request.Address ?? existingSupplier.Address;
            existingSupplier.TaxNumber = request.TaxNumber ?? existingSupplier.TaxNumber;
            existingSupplier.IdentificationNumber = request.IdentificationNumber ?? existingSupplier.IdentificationNumber;
            existingSupplier.IdentificationPictureFront = request.IdentificationPictureFront ?? existingSupplier.IdentificationPictureFront;
            existingSupplier.IdentificationPictureBack = request.IdentificationPictureBack ?? existingSupplier.IdentificationPictureBack;
            existingSupplier.Certificates = request.Certificates ?? existingSupplier.Certificates;
            existingSupplier.UpdatedAt = DateTime.UtcNow;

            _supplierRepository.Update(existingSupplier);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSupplier(Guid id)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(id);
            if (existingSupplier == null)
            {
                return false;
            }

            existingSupplier.Status = "Inactive";
            existingSupplier.UpdatedAt = DateTime.UtcNow;
            _supplierRepository.Update(existingSupplier);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SupplierModel>> FilterSuppliers(string? supplierName, string? email, string? phoneNumber, string? status)
        {
            IQueryable<Supplier> query = _supplierRepository.GetAll().Include(s => s.User);

            if (!string.IsNullOrEmpty(supplierName))
            {
                query = query.Where(s => s.SupplierName == supplierName);
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(s => s.Email == email);
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(s => s.PhoneNumber == phoneNumber);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            var result = await query.ToListAsync();
            return _mapper.Map<List<SupplierModel>>(result);
        }

        public async Task<IEnumerable<SupplierModel>> SearchSuppliers(string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return await GetAllSuppliers();
            }

            var lowerKeyword = keyword.ToLower();

            var result = await _supplierRepository.GetAll()
                .Include(s => s.User)
                .Where(s =>
                    (s.SupplierName != null && s.SupplierName.ToLower().Contains(lowerKeyword)) ||
                    (s.Email != null && s.Email.ToLower().Contains(lowerKeyword)) ||
                    (s.PhoneNumber != null && s.PhoneNumber.ToLower().Contains(lowerKeyword))
                )
                .ToListAsync();
            return _mapper.Map<List<SupplierModel>>(result);
        }
    }
}
