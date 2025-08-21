using AutoMapper;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.DesignerMaterialInventory;
using EcoFashionBackEnd.Dtos.Material;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class DesignerMaterialInventoryService
    {
        IRepository<DesignerMaterialInventory, int> _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly SustainabilityService _sustainabilityService;

        public DesignerMaterialInventoryService(IRepository<DesignerMaterialInventory, int> inventoryRepository,
            IMapper mapper,
            AppDbContext dbContext, SustainabilityService sustainabilityService)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _dbContext = dbContext;
            _sustainabilityService = sustainabilityService;
        }

        public async Task<IEnumerable<DesignerMaterialInventoryModel>> GetAllDesignerMaterialInventoriesAsync()
        {
            var inventories = await _dbContext.DesignerMaterialInventories
                .Include(dmi => dmi.Material)
                .ToListAsync();
            return _mapper.Map<List<DesignerMaterialInventoryModel>>(inventories);
        }

        public async Task<DesignerMaterialInventoryModel> GetDesignerMaterialInventoryByIdAsync(int id)
        {
            var inventory = await _dbContext.DesignerMaterialInventories
                .Include(dmi => dmi.Material)
                .FirstOrDefaultAsync(dmi => dmi.InventoryId == id);
            return _mapper.Map<DesignerMaterialInventoryModel>(inventory);
        }
        public async Task<DesignerMaterialInventoryModel> CreateDesignerMaterialInventoryAsync(int userId, CreateDesignerMaterialInventoryRequest request)
        {
            var designer = await _dbContext.Designers
                            .FirstOrDefaultAsync(d => d.UserId == userId);
            if (designer == null)
                throw new ArgumentException("Người dùng không phải là nhà thiết kế");
            var inventory = _mapper.Map<DesignerMaterialInventory>(request);
            inventory.Status = inventory.Quantity != 0
                ? "in_stock"
                : "out_of_stock";
            inventory.LastBuyDate = DateTime.UtcNow;
            var addedInventory = await _inventoryRepository.AddAsync(inventory);
            await _dbContext.SaveChangesAsync();
            // Reload full object with Material and Designer
            var fullInventory = await _dbContext.DesignerMaterialInventories
                .Include(dmi => dmi.Material)
                .FirstOrDefaultAsync(dmi => dmi.InventoryId == addedInventory.InventoryId);
            return _mapper.Map<DesignerMaterialInventoryModel>(fullInventory);
        }
        public async Task<DesignerMaterialInventoryModel?> UpdateDesignerMaterialInventoryAsync(int userId, int inventoryId, UpdateDesignerMaterialInventoryRequest request)
        {
            var designer = await _dbContext.Designers
                            .FirstOrDefaultAsync(d => d.UserId == userId);
            if (designer == null)
                throw new ArgumentException("Người dùng không phải là nhà thiết kế");
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null)
                return null;
           
            _mapper.Map(request, inventory);
            inventory.Status = inventory.Quantity != 0
                ? "in_stock"
                : "out_of_stock";
            _inventoryRepository.Update(inventory);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<DesignerMaterialInventoryModel>(inventory);
        }
        public async Task<bool> DeleteDesignerMaterialInventoryAsync(int userId, int inventoryId)
        {
            var designer = await _dbContext.Designers
                            .FirstOrDefaultAsync(d => d.UserId == userId);
            if (designer == null)
                throw new ArgumentException("Người dùng không phải là nhà thiết kế");
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
            if (inventory == null)
                return false;
            
            _inventoryRepository.Remove(inventoryId);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ApiResult<List<DesignerMaterialInventoryDto>>> GetDesignerMaterialInventoryByDesignerIdAsync(Guid designerId)
        {
            try
            {
                var inventories = await _dbContext.DesignerMaterialInventories
                    .Include(dmi => dmi.Material).ThenInclude(m => m.Supplier)
                    .ToListAsync();
                var inventoriesDtos = new List<DesignerMaterialInventoryDto>();
                var materialIds = inventories.Select(m => m.MaterialId).ToList();
                var sustainabilityReports = await _sustainabilityService.CalculateMaterialsSustainabilityScores(materialIds);
                foreach (var inventorie in inventories)
                {
                    sustainabilityReports.TryGetValue(inventorie.MaterialId, out var sustainabilityReport);
                    var dto = new DesignerMaterialInventoryDto
                    {
                        InventoryId = inventorie.InventoryId,
                        MaterialId = inventorie.MaterialId,
                        Quantity = (int?)inventorie.Quantity,
                        LastBuyDate = inventorie.LastBuyDate,
                        Material = new DesginerStoredMaterialsDto
                        {
                            MaterialId = inventorie.Material.MaterialId,
                            Name = inventorie.Material.Name ?? string.Empty,
                            Description = inventorie.Material.Description ?? string.Empty,
                            MaterialTypeName = inventorie.Material.MaterialType.TypeName ?? string.Empty,
                            RecycledPercentage = inventorie.Material.RecycledPercentage,
                            QuantityAvailable = inventorie.Material.QuantityAvailable,
                            PricePerUnit = inventorie.Material.PricePerUnit,
                            CreatedAt = inventorie.Material.CreatedAt,
                            CarbonFootprint = inventorie.Material.CarbonFootprint,
                            CarbonFootprintUnit = inventorie.Material.CarbonFootprintUnit,
                            WaterUsage = inventorie.Material.WaterUsage,
                            WaterUsageUnit = inventorie.Material.WaterUsageUnit,
                            WasteDiverted = inventorie.Material.WasteDiverted,
                            WasteDivertedUnit = inventorie.Material.WasteDivertedUnit,
                            ProductionCountry = inventorie.Material.ProductionCountry,
                            ProductionRegion = inventorie.Material.ProductionRegion,
                            ManufacturingProcess = inventorie.Material.ManufacturingProcess,
                            CertificationDetails = inventorie.Material.CertificationDetails,
                            CertificationExpiryDate = inventorie.Material.CertificationExpiryDate,
                            TransportDistance = inventorie.Material.TransportDistance,
                            TransportMethod = inventorie.Material.TransportMethod,
                            SupplierName = inventorie.Material.Supplier?.SupplierName ?? string.Empty,
                            SupplierId = inventorie.Material.SupplierId,
                            ImageUrls = inventorie.Material.MaterialImages.Select(img => img.Image.ImageUrl).Where(url => !string.IsNullOrEmpty(url)).Select(url => url!).ToList() ?? new List<string>(),
                            // Sustainability information
                            SustainabilityScore = sustainabilityReport?.OverallSustainabilityScore,
                        },
                    };
                    inventoriesDtos.Add(dto);
                }

                return ApiResult<List<DesignerMaterialInventoryDto>>.Succeed(inventoriesDtos);
            }
            catch (Exception ex)
            {
                return ApiResult<List<DesignerMaterialInventoryDto>>.Fail(ex.Message);
            }
            
        }
    }
}
