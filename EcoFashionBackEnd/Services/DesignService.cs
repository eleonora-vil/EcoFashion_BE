using AutoMapper;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.DesignShow;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class DesignService
    {
        private readonly IRepository<Design, int> _designRepository;
        private readonly IRepository<DesignFeature, int> _designsFeatureRepository;
        private readonly IRepository<DesignsVariant, int> _designsVarientRepository;
        private readonly IRepository<DesignsMaterial, int> _designMaterialRepository;
        private readonly IRepository<Image, int> _imageRepository;
        private readonly IRepository<DesignImage, int> _designImageRepository;
        private readonly IRepository<Warehouse, int> _warehouseRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly CloudService _cloudService;
        private readonly SustainabilityService _sustainabilityService;


        public DesignService(
            IRepository<Design, int> designRepository,
            IRepository<DesignFeature, int> designsFeatureRepository,
            IRepository<DesignsVariant, int> designsVarientRepository,
            IRepository<DesignsMaterial, int> designsMaterialRepository,
            IRepository<Image, int> imageRepository,
            IRepository<DesignImage, int> designImageRepository,
            IRepository<Warehouse, int> warehouseRepository,

            AppDbContext dbContext,
            IMapper mapper,
            CloudService cloudService)
        {
            _designRepository = designRepository;
            _designsFeatureRepository = designsFeatureRepository;
            _designsVarientRepository = designsVarientRepository;
            _designMaterialRepository = designsMaterialRepository;
            _imageRepository = imageRepository;
            _designImageRepository = designImageRepository;
            _warehouseRepository = warehouseRepository;
            _dbContext = dbContext;
            _mapper = mapper;
            _cloudService = cloudService;
        }

        public async Task<List<DesignWithProductInfoDto>> GetDesignsWithProductsAsync()
        {
            var designs = await _designRepository.GetAll()
                .Where(d => d.Products.Any())  // chỉ lấy design có sản phẩm
                .Include(d => d.DesignImages)
                    .ThenInclude(di => di.Image)
                .Include(d => d.DesignsMaterials)
                    .ThenInclude(dm => dm.Materials)
                .ToListAsync();

            var result = designs.Select(d => new DesignWithProductInfoDto
            {
                DesignId = d.DesignId,
                Name = d.Name,
                RecycledPercentage = d.RecycledPercentage,
                ItemTypeId = d.ItemTypeId ?? 0,
                SalePrice = d.SalePrice,
                DesignImageUrls = d.DesignImages.Select(di => di.Image.ImageUrl).ToList(),
                Materials = d.DesignsMaterials.Select(dm => new MaterialDto
                {
                    MaterialId = dm.MaterialId,
                    MaterialName = dm.Materials.Name,
                    MeterUsed = (decimal)dm.MeterUsed
                }).ToList(),
                ProductCount = d.Products.Count
            }).ToList();

            return result;
        }
        public async Task<List<DesignSummaryDto>> GetDesignsWithProductsByDesignerAsync(Guid designerId)
        {
            var designs = await _designRepository.GetAll()
                .Where(d => d.DesignerId == designerId && d.Products.Any())
                .Include(d => d.DesignImages)
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                .Select(d => new DesignSummaryDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeId = d.ItemTypeId,
                    SalePrice = d.SalePrice,
                    DesignImages = d.DesignImages.Select(di => di.Image.ImageUrl).ToList(),
                    Materials = d.DesignsMaterials.Select(dm => new MaterialDto
                    {
                        MaterialId = dm.MaterialId,
                        MaterialName = dm.Materials.Name,
                        MeterUsed = dm.MeterUsed
                    }).ToList()
                })
                .ToListAsync();

            return designs;
        }

        public async Task<DesignDetailDto> GetDesignDetailWithProductsAsync(int designId, Guid designerId)
        {
            var productWarehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);

            var design = await _designRepository.GetAll()
                .Include(d => d.Products).ThenInclude(p => p.Inventories)
                .Include(d => d.Products).ThenInclude(p => p.Size)
                .Include(d => d.DesignImages).ThenInclude(di => di.Image)
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                .Include(d => d.ItemTypes)
                .Include(d => d.DesignFeatures)  // feature is now on Design
                .FirstOrDefaultAsync(d => d.DesignId == designId && d.DesignerId == designerId);

            if (design == null) throw new Exception("Design không tồn tại hoặc không thuộc Designer này.");

            var designDetailDto = new DesignDetailDto
            {
                DesignId = design.DesignId,
                DesignerId = design.DesignerId,
                Name = design.Name,
                Description = design.Description,
                RecycledPercentage = design.RecycledPercentage,
                SalePrice = design.SalePrice,
                ItemTypeId = design.ItemTypes?.ItemTypeId ?? 0,
                ItemTypeName = design.ItemTypes.TypeName,
                CarbonFootprint = design.CarbonFootprint,
                WaterUsage = design.WaterUsage,
                WasteDiverted = design.WasteDiverted,
                Feature = design.DesignFeatures == null ? null : new DesignFeatureDto
                {
                    ReduceWaste = design.DesignFeatures.ReduceWaste,
                    LowImpactDyes = design.DesignFeatures.LowImpactDyes,
                    Durable = design.DesignFeatures.Durable,
                    EthicallyManufactured = design.DesignFeatures.EthicallyManufactured,
                },
                Products = design.Products.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    Price = p.Price,
                    ColorCode = p.ColorCode,
                    SizeId = p.SizeId,
                    QuantityAvailable = p.Inventories
                        .Where(pi => pi.WarehouseId == productWarehouseId)
                        .Select(pi => pi.QuantityAvailable)
                        .FirstOrDefault()
                }).ToList(),
                DesignImages = design.DesignImages.Select(di => di.Image.ImageUrl).ToList(),
                Materials = design.DesignsMaterials.Select(dm => dm.Materials.Name).ToList(),
            };

            return designDetailDto;
        }


        public async Task<int> GetDefaultProductWarehouseIdForDesigner(Guid designerId)
        {
            var warehouse = await _warehouseRepository.GetAll()
                .FirstOrDefaultAsync(w => w.DesignerId == designerId && w.WarehouseType == "Product");

            if (warehouse == null)
                throw new Exception("Không tìm thấy kho sản phẩm (Product) mặc định cho designer.");

            return warehouse.WarehouseId;
        }





        //public async Task<DesignDetailDto?> GetDesignDetailById(int id)
        //{
        //    var design = await _dbContext.Designs
        //       // .Include(d => d.DesignTypes)
        //       // .Include(d => d.DesignsFeature)
        //      //  .Include(d => d.DesignsVariants).ThenInclude(v => v.DesignsSize)
        //        .Include(d => d.DesignsMaterials)
        //            .ThenInclude(dm => dm.Materials)
        //                .ThenInclude(m => m.MaterialType)
        //        .Include(d => d.DesignsMaterials)
        //            .ThenInclude(dm => dm.Materials)
        //                .ThenInclude(m => m.MaterialSustainabilityMetrics)
        //                    .ThenInclude(ms => ms.SustainabilityCriterion)
        //        .Include(d => d.DesignerProfile)
        //        .FirstOrDefaultAsync(d => d.DesignId == id);

        //    if (design == null) return null;

        //    return new DesignDetailDto
        //    {
        //        DesignId = design.DesignId,
        //        Name = design.Name,
        //        Description = design.Description,
        //        RecycledPercentage = design.RecycledPercentage,
        //        SalePrice = design.SalePrice,
        //        UnitPrice = design.UnitPrice,
        //        ProductScore = design.ProductScore,
        //        CreatedAt = design.CreatedAt,

        //       // DesignTypeName = design.DesignTypes?.DesignName,

        //        Variants = design.DesignsVariants.Select(v => new VariantDto
        //        {
        //            SizeName = v.Size?.SizeName ?? "",
        //            Color = v.ColorCode

        //        }).ToList(),

        //        Materials = design.DesignsMaterials.Select(dm => new MaterialDto
        //        {
        //            MaterialId = dm.MaterialId,
        //            MeterUsed = (double)dm.MeterUsed,
        //            MaterialName = dm.Materials?.Name,
        //            MaterialDescription = dm.Materials?.Description,
        //            MaterialTypeName = dm.Materials?.MaterialType?.TypeName,
        //            CarbonFootprint = dm.Materials.CarbonFootprint,
        //            CarbonFootprintUnit = dm.Materials.CarbonFootprintUnit,
        //            WasteDiverted = dm.Materials.WasteDiverted,
        //            WasteDivertedUnit = dm.Materials.WasteDivertedUnit,
        //            WaterUsage = dm.Materials.WaterUsage,
        //            WaterUsageUnit = dm.Materials.WaterUsageUnit,
        //            CertificationDetails = dm.Materials.CertificationDetails,
        //        }).ToList(),



        //        Designer = new DesignerPublicDto
        //        {
        //            DesignerId = design.DesignerProfile.DesignerId,
        //            DesignerName = design.DesignerProfile.DesignerName,
        //            AvatarUrl = design.DesignerProfile.AvatarUrl,
        //            Bio = design.DesignerProfile.Bio,
        //            SpecializationUrl = design.DesignerProfile.SpecializationUrl,
        //            PortfolioUrl = design.DesignerProfile.PortfolioUrl,
        //            BannerUrl = design.DesignerProfile.BannerUrl,
        //            Rating = design.DesignerProfile.Rating,
        //            ReviewCount = design.DesignerProfile.ReviewCount,
        //            Certificates = design.DesignerProfile.Certificates
        //        }
        //    };
        //}


        //public async Task<int> CreateDesign(CreateDesignRequest request, Guid designerId, List<IFormFile> imageFiles)
        //{
        //    // Validate DesignTypeId
        //    //if (!request.DesignTypeId.HasValue ||
        //    //  !await _dbContext.DesignsTypes.AnyAsync(dt => dt.DesignTypeId == request.DesignTypeId.Value))
        //    //{
        //    //    throw new Exception("DesignTypeId không hợp lệ hoặc không tồn tại.");
        //    //}


        //    // Map sang model
        //    var designModel = _mapper.Map<DesignModel>(request);
        //    designModel.DesignerId = designerId;
        //    designModel.CreatedAt = DateTime.UtcNow;

        //    // Model → Entity
        //    var design = _mapper.Map<Design>(designModel);
        //    await _designRepository.AddAsync(design);
        //    await _designRepository.Commit(); // commit để có DesignId

        //    // Tạo DesignFeature
        //    var featureModel = _mapper.Map<DesignFeatureModel>(request.Feature);
        //    featureModel.DesignId = design.DesignId;

        //    //var feature = _mapper.Map<DesignFeature>(featureModel);
        //    //await _designsFeatureRepository.AddAsync(feature);


        //    // Tạo DesignMaterials
        //    var materialRequests = new List<DesignMaterialRequest>();
        //    if (!string.IsNullOrWhiteSpace(request.MaterialsJson))
        //    {
        //        try
        //        {
        //            materialRequests = JsonConvert.DeserializeObject<List<DesignMaterialRequest>>(request.MaterialsJson) ?? new();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Error parsing MaterialsJson: " + ex.Message);
        //            throw new Exception("MaterialsJson không hợp lệ.");
        //        }
        //    }

        //    Console.WriteLine("Số lượng materials: " + materialRequests.Count);

        //    if (materialRequests.Any())
        //    {
        //        var materialModels = _mapper.Map<List<DesignMaterialModel>>(materialRequests);

        //        // Gán DesignId vào model trước khi map sang entity
        //        foreach (var model in materialModels)
        //        {
        //            model.DesignId = design.DesignId;
        //        }

        //        var materialEntities = _mapper.Map<List<DesignsMaterial>>(materialModels);

        //        foreach (var material in materialEntities)
        //        {
        //            await _designMaterialRepository.AddAsync(material);
        //        }
        //    }


        //    // Upload ảnh
        //    if (imageFiles?.Any() == true)
        //    {
        //        var uploadResults = await _cloudService.UploadImagesAsync(imageFiles);
        //        foreach (var uploadResult in uploadResults)
        //        {
        //            if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
        //            {
        //                var designImage = new DesignImage
        //                {
        //                    DesignId = design.DesignId,
        //                    Image = new Image
        //                    {
        //                        ImageUrl = uploadResult.SecureUrl.ToString()
        //                    }
        //                };

        //                await _designImageRepository.AddAsync(designImage);
        //            }
        //            else
        //            {
        //                Console.WriteLine(" Upload failed or returned null SecureUrl.");
        //            }
        //        }
        //    }

        //    await _designRepository.Commit();
        //    return design.DesignId;

        //}

        //public async Task<List<DesignDetailDto>> GetAllDesignsByDesignerIdAsync(Guid designerId)
        //{
        //    var result = await _designRepository.GetAll().AsNoTracking()
        //        .Where(d => d.DesignerId == designerId)
        //        .Select(d => new DesignDetailDto
        //        {
        //            DesignId = d.DesignId,
        //            Name = d.Name,
        //            RecycledPercentage = d.RecycledPercentage,
        //            SalePrice = (decimal)d.SalePrice,
        //            ProductScore = d.ProductScore,
        //            CreatedAt = d.CreatedAt,
        //           // DesignTypeName = d.DesignTypes.DesignName,

        //            Materials = d.DesignsMaterials.Select(dm => new MaterialDto
        //            {
        //                PersentageUsed = (double)dm.MeterUsed,
        //                MaterialName = dm.Materials.Name,
        //            }).ToList(),



        //            Designer = new DesignerPublicDto
        //            {
        //                DesignerName = d.DesignerProfile.DesignerName,
        //            }
        //        })
        //        .ToListAsync();

        //    return result;
        //}

        //public async Task<IEnumerable<DesignDetailDto?>> GetAllDesignsByDesingerIdPagination(Guid designerId,int page = 1, int pageSize = 12)
        //{
        //    var designs = await _dbContext.Designs
        //        .AsNoTracking()
        //        .Where(d => d.DesignerId == designerId)
        //        .OrderByDescending(d => d.CreatedAt)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(design => new DesignDetailDto
        //        {
        //            DesignId = design.DesignId,
        //            Name = design.Name,
        //            RecycledPercentage = design.RecycledPercentage,
        //            SalePrice = (decimal)design.SalePrice,
        //            ProductScore = design.ProductScore,
        //            CreatedAt = design.CreatedAt,

        //          //  DesignTypeName = design.DesignTypes != null ? design.DesignTypes.DesignName : null,
        //            Materials = design.DesignsMaterials
        //                .Select(dm => new MaterialDto
        //                {
        //                    PersentageUsed = (double)dm.MeterUsed,
        //                    MaterialName = dm.Materials != null ? dm.Materials.Name : null
        //                })
        //                .ToList(),


        //            Designer = design.DesignerProfile != null
        //                ? new DesignerPublicDto
        //                {
        //                    DesignerName = design.DesignerProfile.DesignerName
        //                }
        //                : null
        //        })
        //        .ToListAsync();

        //    return designs;
        //}

        //public async Task<IEnumerable<DesignDetailDto?>> GetAllDesigns()
        //{
        //    var designs = await _dbContext.Designs
        //        //.Include(d => d.DesignTypes)
        //        .Include(d => d.DesignsMaterials)
        //        .Include(d => d.DesignerProfile)
        //        .ToListAsync();

        //    return designs.Select(design => new DesignDetailDto
        //    {
        //        DesignId = design.DesignId,
        //        Name = design.Name,
        //        RecycledPercentage = design.RecycledPercentage,
        //        SalePrice = (decimal)design.SalePrice,
        //        ProductScore = design.ProductScore,
        //        CreatedAt = design.CreatedAt,



        //        Materials = design.DesignsMaterials.Select(dm => new MaterialDto
        //        {
        //            PersentageUsed = (double)dm.MeterUsed,
        //            MaterialName = dm.Materials?.Name,

        //        }).ToList(),


        //        Designer = new DesignerPublicDto
        //        {
        //            DesignerName = design.DesignerProfile.DesignerName,
        //        }
        //    }).ToList();
        //}

        //public async Task<IEnumerable<DesignDetailDto?>> GetAllDesignsPagination(int page = 1, int pageSize = 12)
        //{
        //    var designs = await _dbContext.Designs
        //        .AsNoTracking()
        //        .OrderByDescending(d => d.CreatedAt)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(design => new DesignDetailDto
        //        {
        //            DesignId = design.DesignId,
        //            Name = design.Name,
        //            RecycledPercentage = design.RecycledPercentage,
        //            SalePrice = (decimal)design.SalePrice,
        //            ProductScore = design.ProductScore,
        //            CreatedAt = design.CreatedAt,

        //            //DesignTypeName = design.DesignTypes != null ? design.DesignTypes.DesignName : null,


        //            Materials = design.DesignsMaterials
        //                .Select(dm => new MaterialDto
        //                {
        //                    PersentageUsed = (double)dm.MeterUsed,
        //                    MaterialName = dm.Materials != null ? dm.Materials.Name : null
        //                })
        //                .ToList(),

        //            Designer = design.DesignerProfile != null
        //                ? new DesignerPublicDto
        //                {
        //                    DesignerName = design.DesignerProfile.DesignerName
        //                }
        //                : null
        //        })
        //        .ToListAsync();

        //    return designs;
        //}

        //public async Task<bool> UpdateDesignVariants(int designId, UpdateDesignRequest request)
        //{
        //    var design = await _dbContext.Designs
        //        .Include(d => d.DesignsVariants)
        //        .FirstOrDefaultAsync(d => d.DesignId == designId);

        //    if (design == null) return false;

        //    design.Name = request.Name;
        //    design.Description = request.Description;

        //    var existingVariants = design.DesignsVariants.ToList();
        //    foreach (var variantRequest in request.Variants)
        //    {
        //        if (variantRequest.Id.HasValue)
        //        {
        //            var existing = existingVariants.FirstOrDefault(v => v.Id == variantRequest.Id.Value);
        //            if (existing != null)
        //            {
        //                _mapper.Map(variantRequest, existing);
        //            }
        //        }
        //        else
        //        {
        //            var newEntity = _mapper.Map<DesignsVariant>(variantRequest);
        //            newEntity.DesignId = designId;
        //            _dbContext.DesignsVarients.Add(newEntity);
        //        }
        //    }

        //    var updatedIds = request.Variants.Where(v => v.Id.HasValue).Select(v => v.Id.Value).ToHashSet();
        //    var toRemove = existingVariants.Where(v => !updatedIds.Contains(v.Id)).ToList();
        //    _dbContext.DesignsVarients.RemoveRange(toRemove);

        //    await _dbContext.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<DesignModel?> GetDesignById(int id)
        //{
        //    var design = await _designRepository.GetByIdAsync(id);
        //    return _mapper.Map<DesignModel>(design);
        //}

        //public async Task<bool> DeleteDesign(int id)
        //{
        //    var result = _designRepository.Remove(id);
        //    await _designRepository.Commit();
        //    return result != null;
        //}


    }
}