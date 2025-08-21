using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    /// <summary>
    /// Centralized query service for Material-related database operations
    /// Eliminates duplicate Include patterns across services
    /// </summary>
    public class MaterialQueryService
    {
        private readonly AppDbContext _dbContext;

        public MaterialQueryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Standard Material query with all related entities
        /// Used by MaterialService, CartService, OrderDetailService
        /// </summary>
        public IQueryable<Material> GetMaterialsWithIncludes()
        {
            return _dbContext.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Supplier)
                .Include(m => m.MaterialImages).ThenInclude(mi => mi.Image)
                .Include(m => m.MaterialSustainabilityMetrics)
                    .ThenInclude(ms => ms.SustainabilityCriterion);
        }

        /// <summary>
        /// Lightweight Material query for basic operations
        /// </summary>
        public IQueryable<Material> GetMaterialsBasic()
        {
            return _dbContext.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Supplier);
        }

        /// <summary>
        /// Material query with images only (for display purposes)
        /// </summary>
        public IQueryable<Material> GetMaterialsWithImages()
        {
            return _dbContext.Materials
                .Include(m => m.MaterialType)
                .Include(m => m.Supplier)
                .Include(m => m.MaterialImages).ThenInclude(mi => mi.Image);
        }

        /// <summary>
        /// Get single material by ID with full includes
        /// </summary>
        public async Task<Material?> GetMaterialByIdAsync(int materialId)
        {
            return await GetMaterialsWithIncludes()
                .FirstOrDefaultAsync(m => m.MaterialId == materialId);
        }

        /// <summary>
        /// Get multiple materials by IDs with full includes
        /// </summary>
        public async Task<List<Material>> GetMaterialsByIdsAsync(List<int> materialIds)
        {
            return await GetMaterialsWithIncludes()
                .Where(m => materialIds.Contains(m.MaterialId))
                .ToListAsync();
        }

        /// <summary>
        /// Get approved and available materials for public display
        /// </summary>
        public async Task<List<Material>> GetPublicMaterialsAsync()
        {
            return await GetMaterialsWithIncludes()
                .Where(m => m.IsAvailable && m.ApprovalStatus == "Approved")
                .ToListAsync();
        }

        /// <summary>
        /// Get all materials for admin (regardless of approval status)
        /// </summary>
        public async Task<List<Material>> GetAllMaterialsAdminAsync()
        {
            return await GetMaterialsWithIncludes()
                .ToListAsync();
        }

        /// <summary>
        /// Get materials by supplier
        /// </summary>
        public async Task<List<Material>> GetMaterialsBySupplierAsync(Guid supplierId)
        {
            return await GetMaterialsWithIncludes()
                .Where(m => m.SupplierId == supplierId)
                .ToListAsync();
        }

        /// <summary>
        /// Get materials by type (with approval filter)
        /// </summary>
        public async Task<List<Material>> GetMaterialsByTypeAsync(int typeId, bool approvedOnly = true)
        {
            var query = GetMaterialsWithIncludes()
                .Where(m => m.TypeId == typeId);
                
            if (approvedOnly)
            {
                query = query.Where(m => m.IsAvailable && m.ApprovalStatus == "Approved");
            }
                
            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Get materials with custom filters
        /// </summary>
        public async Task<List<Material>> GetMaterialsFilteredAsync(
            bool? isAvailable = null, 
            string? approvalStatus = null, 
            Guid? supplierId = null, 
            int? typeId = null)
        {
            var query = GetMaterialsWithIncludes();
            
            if (isAvailable.HasValue)
                query = query.Where(m => m.IsAvailable == isAvailable.Value);
                
            if (!string.IsNullOrWhiteSpace(approvalStatus))
                query = query.Where(m => m.ApprovalStatus == approvalStatus);
                
            if (supplierId.HasValue)
                query = query.Where(m => m.SupplierId == supplierId.Value);
                
            if (typeId.HasValue)
                query = query.Where(m => m.TypeId == typeId.Value);
                
            return await query.ToListAsync();
        }
        
        /// <summary>
        /// Get materials with comprehensive filtering for public/customer use
        /// </summary>
        public async Task<List<Material>> GetMaterialsWithComprehensiveFiltersAsync(
            bool? isAvailable = null,
            string? approvalStatus = null,
            int? typeId = null,
            Guid? supplierId = null,
            string? supplierName = null,
            string? materialName = null,
            string? productionCountry = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minQuantity = null,
            bool? hasCertification = null,
            string? transportMethod = null,
            bool publicOnly = true)
        {
            var query = GetMaterialsWithIncludes();
            
            // Default filter for public access - only approved and available materials
            if (publicOnly)
            {
                query = query.Where(m => m.IsAvailable && m.ApprovalStatus == "Approved");
            }
            else
            {
                // Apply availability filter if specified
                if (isAvailable.HasValue)
                    query = query.Where(m => m.IsAvailable == isAvailable.Value);
                    
                // Apply approval status filter if specified
                if (!string.IsNullOrWhiteSpace(approvalStatus))
                    query = query.Where(m => m.ApprovalStatus == approvalStatus);
            }
            
            // Material type filter
            if (typeId.HasValue)
                query = query.Where(m => m.TypeId == typeId.Value);
                
            // Supplier filters
            if (supplierId.HasValue)
                query = query.Where(m => m.SupplierId == supplierId.Value);
                
            if (!string.IsNullOrWhiteSpace(supplierName))
                query = query.Where(m => m.Supplier != null && m.Supplier.SupplierName.Contains(supplierName));
                
            // Material name filter
            if (!string.IsNullOrWhiteSpace(materialName))
                query = query.Where(m => m.Name != null && m.Name.Contains(materialName));
                
            // Production country filter
            if (!string.IsNullOrWhiteSpace(productionCountry))
                query = query.Where(m => m.ProductionCountry == productionCountry);
                
            // Price range filters
            if (minPrice.HasValue)
                query = query.Where(m => m.PricePerUnit >= minPrice.Value);
                
            if (maxPrice.HasValue)
                query = query.Where(m => m.PricePerUnit <= maxPrice.Value);
                
            // Quantity filter
            if (minQuantity.HasValue)
                query = query.Where(m => m.QuantityAvailable >= minQuantity.Value);
                
            // Certification filter
            if (hasCertification.HasValue)
            {
                if (hasCertification.Value)
                    query = query.Where(m => !string.IsNullOrEmpty(m.CertificationDetails));
                else
                    query = query.Where(m => string.IsNullOrEmpty(m.CertificationDetails));
            }
            
            // Transport method filter
            if (!string.IsNullOrWhiteSpace(transportMethod))
                query = query.Where(m => m.TransportMethod == transportMethod);
                
            return await query.ToListAsync();
        }
    }
}
