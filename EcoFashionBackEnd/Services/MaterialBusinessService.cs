using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Common.Payloads.Requests;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    /// <summary>
    /// Service for handling complex Material business logic
    /// Centralizes inventory management, validation, and workflow operations
    /// </summary>
    public class MaterialBusinessService
    {
        private readonly AppDbContext _dbContext;
        private readonly NotificationService _notificationService;

        public MaterialBusinessService(
            AppDbContext dbContext,
            NotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Validate material creation request with business rules
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateMaterialCreationAsync(MaterialCreationFormRequest request)
        {
            // Guard: validate supplier and material type to avoid FK errors
            var supplierExists = await _dbContext.Suppliers.AnyAsync(s => s.SupplierId == request.SupplierId);
            if (!supplierExists)
            {
                return (false, "Supplier not found or not loaded. Vui lòng đăng nhập lại hoặc thử lại sau.");
            }

            var materialTypeExists = await _dbContext.MaterialTypes.AnyAsync(mt => mt.TypeId == request.TypeId);
            if (!materialTypeExists)
            {
                return (false, "Material type không hợp lệ.");
            }

            // Note:
            // // Kiểm tra trùng tên Material theo từng supplier (không phân biệt hoa thường, bỏ khoảng trắng đầu/cuối)
            // var normalizedName = request.Name?.Trim();
            // if (!string.IsNullOrWhiteSpace(normalizedName))
            // {
            //     var normalizedLowerName = normalizedName.ToLower();
            //     var isDuplicateName = await _dbContext.Materials
            //         .AnyAsync(m => m.SupplierId == request.SupplierId
            //                        && m.Name != null
            //                        && m.Name.Trim().ToLower() == normalizedLowerName);

            //     if (isDuplicateName)
            //     {
            //         return (false, "Bạn đã có Material cùng tên. Vui lòng chọn tên khác.");
            //     }
            // }

            return (true, string.Empty);
        }

        /// <summary>
        /// Create new material with business logic and sustainability metrics
        /// </summary>
        public async Task<Material> CreateMaterialAsync(MaterialCreationFormRequest request)
        {
            // Tự động tính toán thông tin vận chuyển nếu chưa có (handles both auto and override)
            TransportCalculationService.CalculateTransportInfo(request);

            var material = new Material
            {
                SupplierId = request.SupplierId,
                TypeId = request.TypeId,
                Name = request.Name,
                Description = request.Description,
                RecycledPercentage = request.RecycledPercentage,
                QuantityAvailable = request.QuantityAvailable,
                PricePerUnit = request.PricePerUnit,
                DocumentationUrl = request.DocumentationUrl,
                CarbonFootprint = request.CarbonFootprint,
                CarbonFootprintUnit = "kg CO2e/mét",
                WaterUsage = request.WaterUsage,
                WaterUsageUnit = "lít/mét",
                WasteDiverted = request.WasteDiverted,
                WasteDivertedUnit = "%",
                ProductionCountry = request.ProductionCountry,
                ProductionRegion = request.ProductionRegion,
                ManufacturingProcess = request.ManufacturingProcess,
                CertificationDetails = request.CertificationDetails,
                CertificationExpiryDate = request.CertificationExpiryDate,
                // Use calculated transport info from CalculateTransportInfo (supports override)
                TransportDistance = request.TransportDistance,
                TransportMethod = request.TransportMethod,
                // Newly created materials require admin approval
                ApprovalStatus = "Pending",
                IsAvailable = false,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            _dbContext.Materials.Add(material);
            await _dbContext.SaveChangesAsync();

            // Add sustainability metrics
            await AddSustainabilityMetricsAsync(material, request);

            // Send notification to admin about new material
            await _notificationService.CreateNewMaterialNotificationAsync(material.MaterialId);

            return material;
        }

        /// <summary>
        /// Add sustainability metrics for a material
        /// </summary>
        private async Task AddSustainabilityMetricsAsync(Material material, MaterialCreationFormRequest request)
        {
            var sustainabilityEntries = new List<MaterialSustainability>();

            // Add core sustainability metrics (Carbon, Water, Waste) - use fixed CriterionIds
            if (request.CarbonFootprint.HasValue)
            {
                sustainabilityEntries.Add(new MaterialSustainability
                {
                    MaterialId = material.MaterialId,
                    CriterionId = 1, // Carbon Footprint
                    Value = request.CarbonFootprint.Value
                });
            }
            if (request.WaterUsage.HasValue)
            {
                sustainabilityEntries.Add(new MaterialSustainability
                {
                    MaterialId = material.MaterialId,
                    CriterionId = 2, // Water Usage
                    Value = request.WaterUsage.Value
                });
            }
            if (request.WasteDiverted.HasValue)
            {
                sustainabilityEntries.Add(new MaterialSustainability
                {
                    MaterialId = material.MaterialId,
                    CriterionId = 3, // Waste Diverted
                    Value = request.WasteDiverted.Value
                });
            }

            // Add sustainability certifications from frontend selection
            // Frontend sends certificates as { criterionId: X, value: 100 } where value=100 means "has certificate"
            if (request.SustainabilityCriteria != null && request.SustainabilityCriteria.Any())
            {
                // Get valid criterion IDs that exist in database and are active
                var validCriterionIds = await _dbContext.SustainabilityCriterias
                    .Where(sc => sc.IsActive)
                    .Select(sc => sc.CriterionId)
                    .ToListAsync();

                foreach (var criterion in request.SustainabilityCriteria)
                {
                    // Skip if criterion doesn't exist or is already added by core metrics
                    if (!validCriterionIds.Contains(criterion.CriterionId) || 
                        sustainabilityEntries.Any(se => se.CriterionId == criterion.CriterionId))
                    {
                        continue;
                    }

                    sustainabilityEntries.Add(new MaterialSustainability
                    {
                        MaterialId = material.MaterialId,
                        CriterionId = criterion.CriterionId,
                        Value = criterion.Value
                    });
                }
            }

            // Add all entries in one go to avoid duplicate key conflicts
            if (sustainabilityEntries.Any())
            {
                _dbContext.MaterialSustainabilities.AddRange(sustainabilityEntries);
                await _dbContext.SaveChangesAsync();
            }

            // Transport (CriterionId = 5) is calculated dynamically in SustainabilityService
            // No need to store transport data in MaterialSustainability table
        }

        /// <summary>
        /// Handle material approval/rejection workflow
        /// </summary>
        public async Task<bool> SetMaterialApprovalAsync(int materialId, bool approve, string? adminNote = null)
        {
            var material = await _dbContext.Materials.FindAsync(materialId);
            if (material == null) return false;

            material.ApprovalStatus = approve ? "Approved" : "Rejected";
            material.IsAvailable = approve; // only available when approved
            if (!string.IsNullOrWhiteSpace(adminNote))
            {
                material.AdminNote = adminNote;
            }
            material.LastUpdated = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            
            if (approve) 
            {
                await EnsureInventoryForApprovedMaterialAsync(material);
            }

            // Send notification to supplier about approval status
            await _notificationService.CreateMaterialApprovalNotificationAsync(materialId, material.SupplierId, material.ApprovalStatus, adminNote);

            return true;
        }

        /// <summary>
        /// Tự động tạo kho mặc định cho supplier nếu chưa có
        /// </summary>
        private async Task EnsureInventoryForApprovedMaterialAsync(Material material)
        {
            // 1) Tạo kho mặc định cho supplier nếu chưa có
            var warehouse = await _dbContext.Warehouses
                .FirstOrDefaultAsync(w => w.SupplierId == material.SupplierId && w.IsDefault);

            if (warehouse == null)
            {
                warehouse = new Warehouse
                {
                    Name = "Kho nhà cung cấp",
                    WarehouseType = "Material",
                    SupplierId = material.SupplierId,
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Warehouses.Add(warehouse);
                await _dbContext.SaveChangesAsync();
            }

            // 2) Tạo dòng tồn cho material ở kho mặc định nếu chưa có
            var stock = await _dbContext.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialId == material.MaterialId && s.WarehouseId == warehouse.WarehouseId);

            if (stock == null)
            {
                var initialQuantity = Convert.ToDecimal(material.QuantityAvailable);
                
                stock = new MaterialStock
                {
                    MaterialId = material.MaterialId,
                    WarehouseId = warehouse.WarehouseId,
                    // CASE A (đồng bộ với QuantityAvailable đã nhập khi tạo):
                    QuantityOnHand = initialQuantity,
                    // CASE B : để 0 và supplier nhập kho thủ công
                    // QuantityOnHand = 0m,
                    MinThreshold = 0m,
                    LastUpdated = DateTime.UtcNow
                };
                _dbContext.MaterialStocks.Add(stock);
                await _dbContext.SaveChangesAsync();

                // Ghi log transaction cho lần nhập kho đầu tiên khi approve
                if (initialQuantity > 0)
                {
                    var transaction = new MaterialStockTransaction
                    {
                        MaterialId = material.MaterialId,
                        WarehouseId = warehouse.WarehouseId,
                        TransactionType = MaterialTransactionType.SupplierReceipt,
                        QuantityChange = initialQuantity,
                        BeforeQty = 0m,
                        AfterQty = initialQuantity,
                        Unit = "mét", // Default unit cho vải
                        ReferenceType = "MaterialApproval",
                        ReferenceId = material.MaterialId.ToString(),
                        Note = $"Nhập kho tự động khi approve material '{material.Name}' - Số lượng ban đầu từ supplier: {material.QuantityAvailable}",
                        CreatedByUserId = null, // System automatic
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.MaterialStockTransactions.Add(transaction);
                    await _dbContext.SaveChangesAsync();
                }
            }

            // 3) Đồng bộ tổng về Material để homepage hiển thị đúng
            var total = await _dbContext.MaterialStocks
                .Where(s => s.MaterialId == material.MaterialId)
                .SumAsync(s => s.QuantityOnHand);

            material.QuantityAvailable = (int)total;
            material.LastUpdated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete material with business validation
        /// </summary>
        public async Task<bool> DeleteMaterialAsync(int materialId)
        {
            var material = await _dbContext.Materials.FindAsync(materialId);
            if (material == null)
                return false;

            // TODO: Add business validation for deletion (e.g., check if material is in use in orders)

            _dbContext.Materials.Remove(material);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Calculate sustainability score for a material (delegation to SustainabilityService)
        /// </summary>
        private async Task<decimal?> GetMaterialSustainabilityScore(int materialId, SustainabilityService sustainabilityService)
        {
            try
            {
                var report = await sustainabilityService.CalculateMaterialSustainabilityScore(materialId);
                return report?.OverallSustainabilityScore;
            }
            catch
            {
                return null;
            }
        }
    }
}
