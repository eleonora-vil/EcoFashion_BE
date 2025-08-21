using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Dtos.Material;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;


namespace EcoFashionBackEnd.Services
{
    public class MaterialService
    {
        private readonly AppDbContext _dbContext;
        private readonly MaterialQueryService _materialQueryService;
        private readonly MaterialEnrichmentService _materialEnrichmentService;
        private readonly MaterialBusinessService _materialBusinessService;
        private readonly CloudService _cloudService;

        public MaterialService(
            AppDbContext dbContext,
            MaterialQueryService materialQueryService,
            MaterialEnrichmentService materialEnrichmentService,
            MaterialBusinessService materialBusinessService,
            CloudService cloudService)
        {
            _dbContext = dbContext;
            _materialQueryService = materialQueryService;
            _materialEnrichmentService = materialEnrichmentService;
            _materialBusinessService = materialBusinessService;
            _cloudService = cloudService;
        }
        
        // Get material detail by id
        public async Task<ApiResult<MaterialDetailResponse>> GetMaterialDetailByIdAsync(int materialId)
        {
            try
            {
                var material = await _materialQueryService.GetMaterialByIdAsync(materialId);
                if (material == null)
                    return ApiResult<MaterialDetailResponse>.Fail("Material not found");

                var response = await _materialEnrichmentService.MapToDetailResponseAsync(material);
                return ApiResult<MaterialDetailResponse>.Succeed(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMaterialDetailByIdAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ApiResult<MaterialDetailResponse>.Fail(ex.Message);
            }
        }

        // REMOVED: GetMaterialsByTypeAsync() - Replaced by GetAllMaterialsWithFiltersAsync(typeId: typeId)
        // REMOVED: GetAllMaterialsAsync() - Replaced by GetAllMaterialsWithFiltersAsync(publicOnly: true)
        
        /// <summary>
        /// Get all materials with comprehensive filtering and sorting by sustainability score
        /// </summary>
        public async Task<ApiResult<List<MaterialDetailDto>>> GetAllMaterialsWithFiltersAsync(
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
            bool sortBySustainability = true,
            bool publicOnly = true)
        {
            try
            {
                var materials = await _materialQueryService.GetMaterialsWithComprehensiveFiltersAsync(
                    isAvailable: publicOnly ? true : null,
                    approvalStatus: publicOnly ? "Approved" : null,
                    typeId: typeId,
                    supplierId: supplierId,
                    supplierName: supplierName,
                    materialName: materialName,
                    productionCountry: productionCountry,
                    minPrice: minPrice,
                    maxPrice: maxPrice,
                    minQuantity: minQuantity,
                    hasCertification: hasCertification,
                    transportMethod: transportMethod,
                    publicOnly: publicOnly
                );

                List<MaterialDetailDto> materialDtos;
                
                if (sortBySustainability)
                {
                    // Sort by sustainability score (descending)
                    materialDtos = await _materialEnrichmentService.EnrichAndSortMaterialsBySustainabilityAsync(materials);
                }
                else
                {
                    // Default enrichment without sorting
                    materialDtos = await _materialEnrichmentService.EnrichMaterialsAsync(materials);
                }

                return ApiResult<List<MaterialDetailDto>>.Succeed(materialDtos);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialDetailDto>>.Fail(ex.Message);
            }
        }

        

        // Admin: get ALL materials regardless of approval/availability
        public async Task<ApiResult<List<MaterialDetailDto>>> GetAllMaterialsAdminAsync()
        {
            try
            {
                var materials = await _materialQueryService.GetAllMaterialsAdminAsync();
                var materialDtos = await _materialEnrichmentService.EnrichMaterialsAsync(materials);
                return ApiResult<List<MaterialDetailDto>>.Succeed(materialDtos);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialDetailDto>>.Fail(ex.Message);
            }
        }

        public async Task<ApiResult<MaterialCreationResponse>> CreateMaterialFromFormAsync(MaterialCreationFormRequest request)
        {
            try
            {
                // Validate business rules
                var (isValid, errorMessage) = await _materialBusinessService.ValidateMaterialCreationAsync(request);
                if (!isValid)
                {
                    return ApiResult<MaterialCreationResponse>.Fail(errorMessage);
                }

                // Create material using business service
                var material = await _materialBusinessService.CreateMaterialAsync(request);

                // Map to response using enrichment service
                var response = await _materialEnrichmentService.MapToCreationResponseAsync(material);

                return ApiResult<MaterialCreationResponse>.Succeed(response);
            }
            catch (Exception ex)
            {
                return ApiResult<MaterialCreationResponse>.Fail(ex.Message);
            }
        }

        public async Task<ApiResult<List<MaterialImageDto>>> UploadMaterialImagesAsync(int materialId, List<IFormFile> imageFiles)
        {
            try
            {
                var material = await _dbContext.Materials
                    .Include(m => m.MaterialImages)
                    .ThenInclude(mi => mi.Image)
                    .FirstOrDefaultAsync(m => m.MaterialId == materialId);

                if (material == null)
                {
                    return ApiResult<List<MaterialImageDto>>.Fail("Material not found");
                }

                if (imageFiles == null || imageFiles.Count == 0)
                {
                    return ApiResult<List<MaterialImageDto>>.Fail("No image files provided");
                }

                var uploadResults = await _cloudService.UploadImagesAsync(imageFiles);

                var savedImages = new List<MaterialImageDto>();
                foreach (var uploadResult in uploadResults)
                {
                    var url = uploadResult?.SecureUrl?.ToString();
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        continue;
                    }

                    var materialImage = new MaterialImage
                    {
                        MaterialId = material.MaterialId,
                        Image = new Image
                        {
                            ImageUrl = url
                        }
                    };

                    _dbContext.MaterialImages.Add(materialImage);
                    await _dbContext.SaveChangesAsync();

                    savedImages.Add(new MaterialImageDto
                    {
                        ImageId = materialImage.ImageId,
                        ImageUrl = url
                    });
                }

                if (savedImages.Count == 0)
                {
                    return ApiResult<List<MaterialImageDto>>.Fail("Image upload failed");
                }

                return ApiResult<List<MaterialImageDto>>.Succeed(savedImages);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialImageDto>>.Fail(ex.Message);
            }
        }

        public async Task<ApiResult<List<MaterialTypeModel>>> GetAllMaterialTypesAsync()
        {
            try
            {
                var materialTypes = await _dbContext.MaterialTypes
                    .Where(mt => mt.IsActive)
                    .OrderBy(mt => mt.DisplayOrder)
                    .Select(mt => new MaterialTypeModel
                    {
                        TypeId = mt.TypeId,
                        TypeName = mt.TypeName ?? string.Empty,
                        ImageUrl = mt.ImageUrl, // Added missing ImageUrl field
                        Description = mt.Description,
                        Category = mt.Category,
                        IsOrganic = mt.IsOrganic,
                        IsRecycled = mt.IsRecycled,
                        SustainabilityNotes = mt.SustainabilityNotes,
                        DisplayOrder = mt.DisplayOrder,
                        IsActive = mt.IsActive
                    })
                    .ToListAsync();

                return ApiResult<List<MaterialTypeModel>>.Succeed(materialTypes);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialTypeModel>>.Fail(ex.Message);
            }
        }

        public async Task<ApiResult<bool>> DeleteMaterialAsync(int materialId)
        {
            try
            {
                var success = await _materialBusinessService.DeleteMaterialAsync(materialId);
                if (!success)
                    return ApiResult<bool>.Fail("Material not found");

                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        public async Task<ApiResult<bool>> SetMaterialApprovalStatusAsync(int materialId, bool approve, string? adminNote = null)
        {
            try
            {
                var success = await _materialBusinessService.SetMaterialApprovalAsync(materialId, approve, adminNote);
                if (!success)
                    return ApiResult<bool>.Fail("Material not found");

                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        // Get all material types

        // Get supplier materials
        public async Task<ApiResult<List<MaterialDetailDto>>> GetSupplierMaterialsAsync(string supplierId, string? approvalStatus)
        {
            try
            {
                if (!Guid.TryParse(supplierId, out var supplierGuid))
                {
                    return ApiResult<List<MaterialDetailDto>>.Fail("Invalid supplier ID format");
                }

                // Use the query service with filters
                var materials = await _materialQueryService.GetMaterialsFilteredAsync(
                    supplierId: supplierGuid, 
                    approvalStatus: string.IsNullOrEmpty(approvalStatus) || approvalStatus.ToLower() == "all" ? null : approvalStatus);

                var materialDtos = await _materialEnrichmentService.EnrichMaterialsAsync(materials);
                return ApiResult<List<MaterialDetailDto>>.Succeed(materialDtos);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialDetailDto>>.Fail($"Error getting supplier materials: {ex.Message}");
            }
        }




        // // Unified filtered method (admin/supplier/public)
        // public async Task<ApiResult<List<MaterialDetailDto>>> GetMaterialsFilteredAsync(string? approvalStatus, bool? isAvailable, string? supplierId, bool includeAll)
        // {
        //     try
        //     {
        //         var query = _dbContext.Materials
        //             .Include(m => m.MaterialType)
        //             .Include(m => m.Supplier)
        //             .Include(m => m.MaterialImages).ThenInclude(mi => mi.Image)
        //             .Include(m => m.MaterialSustainabilityMetrics)
        //             .ThenInclude(ms => ms.SustainabilityCriterion)
        //             .AsQueryable();

        //         if (!includeAll)
        //         {
        //             if (string.IsNullOrWhiteSpace(approvalStatus) && !isAvailable.HasValue && string.IsNullOrWhiteSpace(supplierId))
        //             {
        //                 query = query.Where(m => m.IsAvailable && m.ApprovalStatus == "Approved");
        //             }
        //             else
        //             {
        //                 if (!string.IsNullOrWhiteSpace(approvalStatus)) query = query.Where(m => m.ApprovalStatus == approvalStatus);
        //                 if (isAvailable.HasValue) query = query.Where(m => m.IsAvailable == isAvailable.Value);
        //                 if (!string.IsNullOrWhiteSpace(supplierId)) query = query.Where(m => m.SupplierId.ToString() == supplierId);
        //             }
        //         }

        //         var materials = await query.ToListAsync();

        //         var materialIds = materials.Select(m => m.MaterialId).ToList();
        //         var sustainabilityReports = await _sustainabilityService.CalculateMaterialsSustainabilityScores(materialIds);
        //         var allBenchmarks = await _dbContext.MaterialTypesBenchmarks
        //             .Include(b => b.MaterialType)
        //             .Include(b => b.SustainabilityCriteria)
        //             .ToListAsync();

        //         var materialDtos = new List<MaterialDetailDto>();
        //         foreach (var material in materials)
        //         {
        //             sustainabilityReports.TryGetValue(material.MaterialId, out var sustainabilityReport);
        //             var materialBenchmarks = allBenchmarks.Where(b => b.TypeId == material.TypeId).Select(b => new MaterialTypeBenchmarkModel
        //             {
        //                 BenchmarkId = b.BenchmarkId,
        //                 TypeId = b.TypeId,
        //                 CriteriaId = b.CriteriaId,
        //                 Value = (float)b.Value,
        //                 MaterialType = b.MaterialType,
        //                 SustainabilityCriteria = b.SustainabilityCriteria
        //             }).ToList();

        //             var dto = new MaterialDetailDto
        //             {
        //                 MaterialId = material.MaterialId,
        //                 Name = material.Name ?? string.Empty,
        //                 Description = material.Description ?? string.Empty,
        //                 MaterialTypeName = material.MaterialType?.TypeName ?? string.Empty,
        //                 RecycledPercentage = material.RecycledPercentage,
        //                 QuantityAvailable = material.QuantityAvailable,
        //                 PricePerUnit = material.PricePerUnit,
        //                 CreatedAt = material.CreatedAt,
        //                 CarbonFootprint = material.CarbonFootprint,
        //                 CarbonFootprintUnit = material.CarbonFootprintUnit,
        //                 WaterUsage = material.WaterUsage,
        //                 WaterUsageUnit = material.WaterUsageUnit,
        //                 WasteDiverted = material.WasteDiverted,
        //                 WasteDivertedUnit = material.WasteDivertedUnit,
        //                 ProductionCountry = material.ProductionCountry,
        //                 ProductionRegion = material.ProductionRegion,
        //                 ManufacturingProcess = material.ManufacturingProcess,
        //                 CertificationDetails = material.CertificationDetails,
        //                 CertificationExpiryDate = material.CertificationExpiryDate,
        //                 TransportDistance = material.TransportDistance,
        //                 TransportMethod = material.TransportMethod,
        //                 ApprovalStatus = material.ApprovalStatus,
        //                 AdminNote = material.AdminNote,
        //                 IsAvailable = material.IsAvailable,
        //                 LastUpdated = material.LastUpdated,
        //                 ImageUrls = material.MaterialImages?.Select(mi => mi.Image?.ImageUrl ?? string.Empty).Where(u => !string.IsNullOrWhiteSpace(u)).ToList() ?? new List<string>(),
        //                 SustainabilityScore = sustainabilityReport?.OverallSustainabilityScore,
        //                 SustainabilityLevel = sustainabilityReport?.SustainabilityLevel,
        //                 SustainabilityColor = sustainabilityReport?.LevelColor,
        //                 Supplier = material.Supplier == null ? null : new SupplierPublicModel
        //                 {
        //                     SupplierId = material.Supplier.SupplierId.ToString(),
        //                     SupplierName = material.Supplier.SupplierName,
        //                     AvatarUrl = material.Supplier.AvatarUrl,
        //                     Bio = material.Supplier.Bio,
        //                     SpecializationUrl = material.Supplier.SpecializationUrl,
        //                     PortfolioUrl = material.Supplier.PortfolioUrl,
        //                     PortfolioFiles = material.Supplier.PortfolioFiles,
        //                     BannerUrl = material.Supplier.BannerUrl,
        //                     Email = material.Supplier.Email,
        //                     PhoneNumber = material.Supplier.PhoneNumber,
        //                     Address = material.Supplier.Address,
        //                     Rating = material.Supplier.Rating,
        //                     ReviewCount = material.Supplier.ReviewCount,
        //                     Certificates = material.Supplier.Certificates,
        //                     CreatedAt = material.Supplier.CreatedAt,
        //                     UserFullName = material.Supplier.User?.FullName
        //                 },
        //                 SustainabilityCriteria = material.MaterialSustainabilityMetrics?.Select(ms => new MaterialSustainabilityCriterionDto
        //                 {
        //                     CriterionId = ms.CriterionId,
        //                     Name = ms.SustainabilityCriterion?.Name,
        //                     Description = ms.SustainabilityCriterion?.Description,
        //                     Unit = ms.SustainabilityCriterion?.Unit,
        //                     Value = ms.Value
        //                 }).ToList() ?? new List<MaterialSustainabilityCriterionDto>(),
        //                 Benchmarks = materialBenchmarks
        //             };

        //             materialDtos.Add(dto);
        //         }

        //         return ApiResult<List<MaterialDetailDto>>.Succeed(materialDtos);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ApiResult<List<MaterialDetailDto>>.Fail(ex.Message);
        //     }
        // }
        
    }
}
