using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Dtos.Material;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Dtos;


namespace EcoFashionBackEnd.Services
{
    /// <summary>
    /// Service for enriching Material entities with calculated data
    /// Centralizes sustainability and transport calculations
    /// </summary>
    public class MaterialEnrichmentService
    {
        private readonly SustainabilityService _sustainabilityService;
        private readonly AppDbContext _dbContext;

        public MaterialEnrichmentService(
            SustainabilityService sustainabilityService,
            AppDbContext dbContext)
        {
            _sustainabilityService = sustainabilityService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Enrich single material with sustainability data
        /// </summary>
        public async Task<MaterialDetailDto> EnrichMaterialAsync(Material material)
        {
            var sustainabilityReport = await _sustainabilityService.CalculateMaterialSustainabilityScore(material.MaterialId);
            var benchmarkEntities = await GetMaterialBenchmarksAsync(material.TypeId);
            var benchmarks = benchmarkEntities
                .Select(b => CalculateBenchmarkComparison(b, material))
                .Where(b => b != null)
                .ToList();

            return MapToDetailDto(material, sustainabilityReport, benchmarks);
        }

        /// <summary>
        /// Enrich multiple materials with sustainability data (batch processing)
        /// </summary>
        public async Task<List<MaterialDetailDto>> EnrichMaterialsAsync(List<Material> materials)
        {
            var materialIds = materials.Select(m => m.MaterialId).ToList();
            var sustainabilityReports = await _sustainabilityService.CalculateMaterialsSustainabilityScores(materialIds);
            
            // Get all benchmarks for all material types
            var typeIds = materials.Select(m => m.TypeId).Distinct().ToList();
            var allBenchmarks = await GetBenchmarksForTypesAsync(typeIds);

            var result = new List<MaterialDetailDto>();
            foreach (var material in materials)
            {
                sustainabilityReports.TryGetValue(material.MaterialId, out var sustainabilityReport);
                var materialBenchmarks = allBenchmarks
                    .Where(b => b.TypeId == material.TypeId)
                    .Select(b => CalculateBenchmarkComparison(b, material))
                    .Where(b => b != null)
                    .ToList();
                
                result.Add(MapToDetailDto(material, sustainabilityReport, materialBenchmarks));
            }

            return result;
        }
        
        /// <summary>
        /// Enrich and sort materials by sustainability score (descending)
        /// </summary>
        public async Task<List<MaterialDetailDto>> EnrichAndSortMaterialsBySustainabilityAsync(List<Material> materials)
        {
            var enrichedMaterials = await EnrichMaterialsAsync(materials);
            
            // Sort by sustainability score descending, then by name for consistency
            return enrichedMaterials
                .OrderByDescending(m => m.SustainabilityScore)
                .ThenBy(m => m.Name)
                .ToList();
        }

        /// <summary>
        /// Apply transport calculations to material creation request
        /// </summary>
        public void EnrichMaterialCreationRequest(MaterialCreationFormRequest request)
        {
            // Auto-calculate transport info if not provided
            TransportCalculationService.CalculateTransportInfo(request);
        }

        /// <summary>
        /// Get transport evaluation for a material
        /// </summary>
        public object GetTransportEvaluation(Material material)
        {
            return TransportCalculationService.GetTransportEvaluation(
                material.TransportDistance ?? 0, 
                material.TransportMethod ?? "Unknown"
            );
        }

        /// <summary>
        /// Get production evaluation for a material
        /// </summary>
        public object GetProductionEvaluation(Material material)
        {
            return TransportCalculationService.GetProductionEvaluation(material.ProductionCountry);
        }

        /// <summary>
        /// Map Material entity to DetailDto with all enrichments
        /// </summary>
        private MaterialDetailDto MapToDetailDto(
            Material material, 
            MaterialSustainabilityReport? sustainabilityReport,
            List<MaterialTypeBenchmarkModel> benchmarks)
        {
            return new MaterialDetailDto
            {
                MaterialId = material.MaterialId,
                Name = material.Name ?? string.Empty,
                Description = material.Description ?? string.Empty,
                RecycledPercentage = material.RecycledPercentage,
                QuantityAvailable = material.QuantityAvailable,
                PricePerUnit = material.PricePerUnit,
                DocumentationUrl = material.DocumentationUrl ?? string.Empty,
                
                // Sustainability data
                CarbonFootprint = material.CarbonFootprint,
                CarbonFootprintUnit = material.CarbonFootprintUnit,
                WaterUsage = material.WaterUsage,
                WaterUsageUnit = material.WaterUsageUnit,
                WasteDiverted = material.WasteDiverted,
                WasteDivertedUnit = material.WasteDivertedUnit,

                // Production data
                ProductionCountry = material.ProductionCountry,
                ProductionRegion = material.ProductionRegion,
                ManufacturingProcess = material.ManufacturingProcess,

                // Certification data
                CertificationDetails = material.CertificationDetails,
                CertificationExpiryDate = material.CertificationExpiryDate,

                // Transport data
                TransportDistance = material.TransportDistance,
                TransportMethod = material.TransportMethod,

                // Status
                ApprovalStatus = material.ApprovalStatus,
                AdminNote = material.AdminNote,
                IsAvailable = material.IsAvailable,
                LastUpdated = material.LastUpdated,
                CreatedAt = material.CreatedAt,

                // Supplier data
                SupplierId = material.SupplierId,
                SupplierName = material.Supplier?.SupplierName ?? string.Empty,
                
                // Supplier object
                Supplier = material.Supplier != null ? new SupplierPublicModel
                {
                    SupplierId = material.Supplier.SupplierId,
                    SupplierName = material.Supplier.SupplierName,
                    AvatarUrl = material.Supplier.AvatarUrl,
                    Bio = material.Supplier.Bio,
                    SpecializationUrl = material.Supplier.SpecializationUrl,
                    PortfolioUrl = material.Supplier.PortfolioUrl,
                    PortfolioFiles = material.Supplier.PortfolioFiles,
                    BannerUrl = material.Supplier.BannerUrl,
                    Email = material.Supplier.Email,
                    PhoneNumber = material.Supplier.PhoneNumber,
                    Address = material.Supplier.Address,
                    Rating = material.Supplier.Rating,
                    ReviewCount = material.Supplier.ReviewCount,
                    Certificates = material.Supplier.Certificates,
                    CreatedAt = material.Supplier.CreatedAt
                } : null,
                
                // Images URLs
                ImageUrls = material.MaterialImages?.Select(mi => mi.Image?.ImageUrl).Where(url => !string.IsNullOrEmpty(url)).Select(url => url!).ToList() ?? new List<string>(),

                // Type name
                MaterialTypeName = material.MaterialType?.TypeName ?? string.Empty,

                // Sustainability scores (calculated from report)
                SustainabilityScore = sustainabilityReport?.OverallSustainabilityScore,
                SustainabilityLevel = sustainabilityReport?.SustainabilityLevel,
                SustainabilityColor = sustainabilityReport?.LevelColor,

                // Sustainability criteria
                SustainabilityCriteria = material.MaterialSustainabilityMetrics?.Select(ms => new MaterialSustainabilityCriterionDto
                {
                    CriterionId = ms.CriterionId,
                    Name = ms.SustainabilityCriterion?.Name,
                    Description = ms.SustainabilityCriterion?.Description,
                    Unit = ms.SustainabilityCriterion?.Unit,
                    Value = ms.Value
                }).ToList() ?? new List<MaterialSustainabilityCriterionDto>(),
                
                // Benchmarks (convert from entities to models)
                Benchmarks = benchmarks.Where(b => b != null).ToList()
            };
        }

        /// <summary>
        /// Get benchmarks for a specific material type
        /// </summary>
        private async Task<List<MaterialTypeBenchmark>> GetMaterialBenchmarksAsync(int typeId)
        {
            return await _dbContext.MaterialTypesBenchmarks
                .Where(b => b.TypeId == typeId)
                .Include(b => b.SustainabilityCriteria)
                .ToListAsync();
        }

        /// <summary>
        /// Get benchmarks for multiple material types
        /// </summary>
        private async Task<List<MaterialTypeBenchmark>> GetBenchmarksForTypesAsync(List<int> typeIds)
        {
            return await _dbContext.MaterialTypesBenchmarks
                .Where(b => typeIds.Contains(b.TypeId))
                .Include(b => b.MaterialType)
                .Include(b => b.SustainabilityCriteria)
                .ToListAsync();
        }

        /// <summary>
        /// Map Material entity to MaterialCreationResponse with enriched data
        /// </summary>
        public async Task<MaterialCreationResponse> MapToCreationResponseAsync(Material material)
        {
            var sustainabilityReport = await _sustainabilityService.CalculateMaterialSustainabilityScore(material.MaterialId);
            var materialTypeName = await GetMaterialTypeNameAsync(material.TypeId);

            return new MaterialCreationResponse
            {
                MaterialId = material.MaterialId,
                Name = material.Name ?? string.Empty,
                Description = material.Description ?? string.Empty,
                MaterialTypeName = materialTypeName,
                RecycledPercentage = material.RecycledPercentage,
                QuantityAvailable = material.QuantityAvailable,
                PricePerUnit = material.PricePerUnit,
                DocumentationUrl = material.DocumentationUrl,
                CreatedAt = material.CreatedAt,
                LastUpdated = material.LastUpdated,
                CarbonFootprint = material.CarbonFootprint ?? 0,
                WaterUsage = material.WaterUsage ?? 0,
                WasteDiverted = material.WasteDiverted ?? 0,
                ProductionCountry = material.ProductionCountry,
                ManufacturingProcess = material.ManufacturingProcess,
                CertificationDetails = material.CertificationDetails,
                SustainabilityScore = (int)(sustainabilityReport?.OverallSustainabilityScore ?? 0),
                SustainabilityLevel = sustainabilityReport?.SustainabilityLevel ?? "Unknown",
                SustainabilityColor = sustainabilityReport?.LevelColor ?? "gray",
                MarketPosition = GetMarketPosition((int?)(sustainabilityReport?.OverallSustainabilityScore ?? 0)),
                CompetitiveAdvantage = GetCompetitiveAdvantage(material),
                CriterionScores = sustainabilityReport?.CriterionDetails?.Select(cd => new MaterialCreationResponse.CriterionScoreDetail
                {
                    CriterionName = cd.CriterionName,
                    ActualValue = cd.ActualValue,
                    BenchmarkValue = cd.BenchmarkValue,
                    Unit = cd.Unit,
                    Score = cd.Score,
                    Status = cd.Status
                }).ToList() ?? new List<MaterialCreationResponse.CriterionScoreDetail>(),
                Summary = new MaterialCreationResponse.SustainabilitySummary
                {
                    TotalCriteria = sustainabilityReport?.Summary.TotalCriteria ?? 0,
                    ExcellentCriteria = sustainabilityReport?.Summary.ExcellentCriteria ?? 0,
                    GoodCriteria = sustainabilityReport?.Summary.GoodCriteria ?? 0,
                    AverageCriteria = sustainabilityReport?.Summary.AverageCriteria ?? 0,
                    NeedsImprovementCriteria = sustainabilityReport?.Summary.NeedsImprovementCriteria ?? 0,
                    Recommendation = sustainabilityReport?.Summary.Recommendation ?? string.Empty
                }
            };
        }

        /// <summary>
        /// Map Material entity to MaterialDetailResponse with enriched data
        /// </summary>
        public async Task<MaterialDetailResponse> MapToDetailResponseAsync(Material material)
        {
            var sustainabilityReport = await _sustainabilityService.CalculateMaterialSustainabilityScore(material.MaterialId);

            // Get all benchmarks for this material's type
            var allBenchmarks = await _dbContext.MaterialTypesBenchmarks
                .Include(b => b.MaterialType)
                .Include(b => b.SustainabilityCriteria)
                .ToListAsync();

            var materialBenchmarks = new List<MaterialTypeBenchmarkModel>();
            
            try
            {
                materialBenchmarks = allBenchmarks
                    .Where(b => b.TypeId == material.TypeId)
                    .Select(b => CalculateBenchmarkComparison(b, material))
                    .Where(b => b != null) // Filter out null results (like Transport)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating benchmark comparison: {ex.Message}");
                // Fallback to basic benchmarks without comparison
                materialBenchmarks = allBenchmarks
                    .Where(b => b.TypeId == material.TypeId)
                    .Select(b => new MaterialTypeBenchmarkModel
                    {
                        BenchmarkId = b.BenchmarkId,
                        TypeId = b.TypeId,
                        CriteriaId = b.CriteriaId,
                        Value = (float)b.Value,
                        MaterialType = b.MaterialType,
                        SustainabilityCriteria = b.SustainabilityCriteria
                    })
                    .ToList();
            }

            return new MaterialDetailResponse
            {
                MaterialId = material.MaterialId,
                Name = material.Name ?? string.Empty,
                Description = material.Description ?? string.Empty,
                MaterialTypeName = material.MaterialType?.TypeName ?? string.Empty,
                RecycledPercentage = material.RecycledPercentage,
                QuantityAvailable = material.QuantityAvailable,
                PricePerUnit = material.PricePerUnit,
                DocumentationUrl = material.DocumentationUrl,
                CreatedAt = material.CreatedAt,
                CarbonFootprint = material.CarbonFootprint,
                CarbonFootprintUnit = material.CarbonFootprintUnit,
                WaterUsage = material.WaterUsage,
                WaterUsageUnit = material.WaterUsageUnit,
                WasteDiverted = material.WasteDiverted,
                WasteDivertedUnit = material.WasteDivertedUnit,
                ProductionCountry = material.ProductionCountry,
                ProductionRegion = material.ProductionRegion,
                ManufacturingProcess = material.ManufacturingProcess,
                CertificationDetails = material.CertificationDetails,
                CertificationExpiryDate = material.CertificationExpiryDate,
                TransportDistance = material.TransportDistance,
                TransportMethod = material.TransportMethod,
                ApprovalStatus = material.ApprovalStatus,
                AdminNote = material.AdminNote,
                IsAvailable = material.IsAvailable,
                LastUpdated = material.LastUpdated,
                SustainabilityScore = sustainabilityReport?.OverallSustainabilityScore,
                SustainabilityLevel = sustainabilityReport?.SustainabilityLevel,
                SustainabilityColor = sustainabilityReport?.LevelColor,
                Supplier = material.Supplier == null ? null : new SupplierPublicModel
                {
                    SupplierId = material.Supplier.SupplierId,
                    SupplierName = material.Supplier.SupplierName,
                    AvatarUrl = material.Supplier.AvatarUrl,
                    Bio = material.Supplier.Bio,
                    SpecializationUrl = material.Supplier.SpecializationUrl,
                    PortfolioUrl = material.Supplier.PortfolioUrl,
                    PortfolioFiles = material.Supplier.PortfolioFiles,
                    BannerUrl = material.Supplier.BannerUrl,
                    Email = material.Supplier.Email,
                    PhoneNumber = material.Supplier.PhoneNumber,
                    Address = material.Supplier.Address,
                    Rating = material.Supplier.Rating,
                    ReviewCount = material.Supplier.ReviewCount,
                    Certificates = material.Supplier.Certificates,
                    CreatedAt = material.Supplier.CreatedAt
                },
                ImageUrls = material.MaterialImages?.Select(img => img.Image?.ImageUrl).Where(url => !string.IsNullOrEmpty(url)).Select(url => url!).ToList() ?? new List<string>(),
                SustainabilityCriteria = material.MaterialSustainabilityMetrics?.Select(ms => new MaterialSustainabilityCriterionDto
                {
                    CriterionId = ms.CriterionId,
                    Name = ms.SustainabilityCriterion?.Name,
                    Description = ms.SustainabilityCriterion?.Description,
                    Unit = ms.SustainabilityCriterion?.Unit,
                    Value = ms.Value
                }).ToList() ?? new List<MaterialSustainabilityCriterionDto>(),
                Benchmarks = materialBenchmarks,
                CriterionDetails = sustainabilityReport?.CriterionDetails ?? new List<CriterionCalculationDetail>()
            };
        }

        // Helper methods từ MaterialService
        private async Task<string> GetMaterialTypeNameAsync(int typeId)
        {
            var materialType = await _dbContext.MaterialTypes.FindAsync(typeId);
            return materialType?.TypeName ?? "Unknown";
        }

        private string GetMarketPosition(int? sustainabilityScore)
        {
            if (!sustainabilityScore.HasValue) return "Unknown";
            return sustainabilityScore.Value switch
            {
                >= 80 => "Premium - Phù hợp cho thị trường cao cấp",
                >= 60 => "Standard - Phù hợp cho thị trường phổ thông",
                >= 40 => "Budget - Phù hợp cho thị trường giá rẻ",
                _ => "Limited - Chỉ phù hợp cho thị trường đặc biệt"
            };
        }

        private string GetCompetitiveAdvantage(Material material)
        {
            var advantages = new List<string>();

            // Check for sustainable transport (short distance and eco-friendly method)
            if (material.TransportDistance.HasValue && material.TransportDistance < 1000 && 
                !string.IsNullOrEmpty(material.TransportMethod) && 
                material.TransportMethod.ToLower() != "air")
                advantages.Add("Vận chuyển bền vững");
            if (material.CarbonFootprint < 3)
                advantages.Add("Lượng carbon thấp");
            if (material.WaterUsage < 100)
                advantages.Add("Tiết kiệm nước");
            if (!string.IsNullOrEmpty(material.CertificationDetails))
                advantages.Add("Có chứng nhận bền vững");

            return string.Join(", ", advantages);
        }

        // Helper method để tính toán so sánh benchmark (từ MaterialService)
        private MaterialTypeBenchmarkModel CalculateBenchmarkComparison(
            MaterialTypeBenchmark benchmark, 
            Material material)
        {
            var result = new MaterialTypeBenchmarkModel
            {
                BenchmarkId = benchmark.BenchmarkId,
                TypeId = benchmark.TypeId,
                CriteriaId = benchmark.CriteriaId,
                Value = (float)benchmark.Value,
                MaterialType = benchmark.MaterialType,
                SustainabilityCriteria = benchmark.SustainabilityCriteria
            };

            // Lấy giá trị thực tế từ material
            float? actualValue = benchmark.CriteriaId switch
            {
                1 => material.CarbonFootprint.HasValue ? (float)material.CarbonFootprint : null, // Carbon Footprint
                2 => material.WaterUsage.HasValue ? (float)material.WaterUsage : null, // Water Usage
                3 => material.WasteDiverted.HasValue ? (float)material.WasteDiverted : null, // Waste Diverted
                4 => !string.IsNullOrEmpty(material.CertificationDetails) && 
                     HasRecognizedSustainabilityCertification(material.CertificationDetails) ? 1f : 0f, // Sustainability Certification
                5 => null, // Transport - calculated dynamically in SustainabilityService
                _ => null
            };

            // Skip Transport criterion as it's calculated dynamically
            if (benchmark.CriteriaId == 5)
            {
                return null; // Return null to skip Transport in benchmark comparison
            }

            result.ActualValue = actualValue;

            if (actualValue.HasValue)
            {
                // Tính phần trăm cải thiện
                float benchmarkValue = (float)benchmark.Value;
                float improvement = 0f;

                // Logic tính toán tùy theo loại tiêu chí
                switch (benchmark.CriteriaId)
                {
                    case 1: // Carbon Footprint - thấp hơn = tốt hơn
                        if (benchmarkValue > 0)
                            improvement = ((benchmarkValue - actualValue.Value) / benchmarkValue) * 100;
                        break;
                    case 2: // Water Usage - thấp hơn = tốt hơn
                        if (benchmarkValue > 0)
                            improvement = ((benchmarkValue - actualValue.Value) / benchmarkValue) * 100;
                        break;
                    case 3: // Waste Diverted - cao hơn = tốt hơn
                        if (benchmarkValue > 0)
                            improvement = ((actualValue.Value - benchmarkValue) / benchmarkValue) * 100;
                        break;
                    case 4: // Organic Certification - boolean logic
                        if (actualValue.Value >= 1f && benchmarkValue >= 1f)
                        {
                            improvement = 0f; // Cả hai đều có certification
                            result.ImprovementStatus = "Đạt chuẩn";
                            result.ImprovementColor = "success";
                        }
                        else if (actualValue.Value >= 1f && benchmarkValue < 1f)
                        {
                            improvement = 100f; // Có certification khi benchmark không yêu cầu = bonus
                            result.ImprovementStatus = "Vượt chuẩn (Bonus)";
                            result.ImprovementColor = "success";
                        }
                        else if (actualValue.Value < 1f && benchmarkValue >= 1f)
                        {
                            improvement = -100f; // Không có certification khi benchmark yêu cầu
                            result.ImprovementStatus = "Chưa đạt";
                            result.ImprovementColor = "error";
                        }
                        else
                        {
                            improvement = 0f; // Cả hai đều không có certification = đạt chuẩn
                            result.ImprovementStatus = "Không yêu cầu";
                            result.ImprovementColor = "success";
                        }
                        result.ImprovementPercentage = improvement;
                        return result; // Return sớm cho Organic Certification
                    case 5: // Transport - calculated dynamically in SustainabilityService
                        // Transport is handled separately, skip this case
                        break;
                }

                result.ImprovementPercentage = improvement;

                // Xác định trạng thái và màu sắc (chỉ cho các tiêu chí khác Organic Certification)
                if (benchmark.CriteriaId != 4) // Không áp dụng cho Organic Certification
                {
                    if (improvement > 0)
                    {
                        result.ImprovementStatus = "Tốt hơn";
                        result.ImprovementColor = "success";
                    }
                    else if (improvement < 0)
                    {
                        result.ImprovementStatus = "Kém hơn";
                        result.ImprovementColor = "error";
                    }
                    else
                    {
                        result.ImprovementStatus = "Bằng";
                        result.ImprovementColor = "warning";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Kiểm tra xem material có chứng chỉ bền vững được công nhận không (10 chứng chỉ chính + 6 mới)
        /// </summary>
        private static bool HasRecognizedSustainabilityCertification(string certificationDetails)
        {
            if (string.IsNullOrEmpty(certificationDetails))
                return false;

            var details = certificationDetails.ToUpperInvariant();
            
            // Tier 1: Comprehensive sustainability standards
            if (details.Contains("GOTS") || 
                details.Contains("CRADLE TO CRADLE") || 
                details.Contains("USDA ORGANIC") || 
                details.Contains("BLUESIGN"))
                return true;

            // Tier 2: High-value specialized standards
            if (details.Contains("OCS") || 
                details.Contains("EU ECOLABEL") || 
                details.Contains("FAIRTRADE") || 
                details.Contains("BCI") || 
                details.Contains("BETTER COTTON") ||
                details.Contains("OEKO-TEX") || 
                details.Contains("RWS") || 
                details.Contains("ECO PASSPORT"))
                return true;

            // Tier 3: Material-specific recycling standards
            if (details.Contains("GRS") || 
                details.Contains("RCS") || 
                details.Contains("RECYCLED CLAIM"))
                return true;

            return false;
        }
    }
}
