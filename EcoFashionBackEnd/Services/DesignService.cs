using AutoMapper;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.DesignShow;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EcoFashionBackEnd.Services
{
    public class DesignService
    {
        #region injection
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
        #endregion
        public async Task<List<DesignWithProductInfoDto>> GetDesignsWithProductsAsync()
        {
            return await _designRepository.GetAll().AsNoTracking()
                .Where(d => d.Products.Any()) // Only designs with products
                .Select(d => new DesignWithProductInfoDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,
                    DesignImageUrls = d.DesignImages
                        .Select(di => di.Image.ImageUrl)
                        .ToList(),
                    Materials = d.DesignsMaterials
                        .Select(dm => new MaterialDto
                        {
                            MaterialId = dm.MaterialId,
                            MaterialName = dm.Materials.Name,
                            MeterUsed = (decimal)dm.MeterUsed
                        })
                        .ToList(),
                    ProductCount = d.Products.Count
                })
                .ToListAsync();
        }

        public async Task<List<DesignSummaryDto>> GetDesignsWithProductsByDesignerAsync(Guid designerId)
        {
            var designs = await _designRepository.GetAll().AsNoTracking()
                .Where(d => d.DesignerId == designerId && d.Products.Any())
                .Include(d => d.DesignImages)
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                .Select(d => new DesignSummaryDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,
                    DesignImageUrls = d.DesignImages.Select(di => di.Image.ImageUrl).ToList(),
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

            var designDetailDto = await _designRepository.GetAll().AsNoTracking()
                .Where(d => d.DesignId == designId && d.DesignerId == designerId)
                .Select(d => new DesignDetailDto
                {
                    DesignId = d.DesignId,
                    DesignerId = d.DesignerId,
                    Name = d.Name,
                    Description = d.Description,
                    RecycledPercentage = d.RecycledPercentage,
                    SalePrice = d.SalePrice,
                    ItemTypeId = d.ItemTypes != null ? d.ItemTypes.ItemTypeId : 0,
                    ItemTypeName = d.ItemTypes.TypeName,
                    CarbonFootprint = d.CarbonFootprint,
                    WaterUsage = d.WaterUsage,
                    WasteDiverted = d.WasteDiverted,
                    CareInstruction = d.CareInstruction,
                    Feature = d.DesignFeatures == null ? null : new DesignFeatureDto
                    {
                        ReduceWaste = d.DesignFeatures.ReduceWaste,
                        LowImpactDyes = d.DesignFeatures.LowImpactDyes,
                        Durable = d.DesignFeatures.Durable,
                        EthicallyManufactured = d.DesignFeatures.EthicallyManufactured
                    },
                    Products = d.Products.Select(p => new ProductDto
                    {
                        ProductId = p.ProductId,
                        SKU = p.SKU,
                        Price = p.Price,
                        ColorCode = p.ColorCode,
                        SizeId = p.SizeId,
                        SizeName = p.Size.SizeName,
                        QuantityAvailable = p.Inventories
                            .Where(pi => pi.WarehouseId == productWarehouseId)
                            .Select(pi => pi.QuantityAvailable)
                            .FirstOrDefault()
                    }).ToList(),
                    DesignImages = d.DesignImages.Select(di => di.Image.ImageUrl).ToList(),
                    Materials = d.DesignsMaterials.Select(dm => new MaterialDto
                    {
                        MaterialId = dm.MaterialId,
                        MaterialName = dm.Materials.Name,
                        MeterUsed = dm.MeterUsed,
                        Certificates = dm.Materials.CertificationDetails,
                        Description = dm.Materials.Description
                    }).ToList(),
                    Designer = new DesignerPublicDto
                    {
                        DesignerId = d.DesignerProfile.DesignerId,
                        DesignerName = d.DesignerProfile.DesignerName,
                        AvatarUrl = d.DesignerProfile.AvatarUrl,
                        Bio = d.DesignerProfile.Bio,
                        SpecializationUrl = d.DesignerProfile.SpecializationUrl,
                        PortfolioUrl = d.DesignerProfile.PortfolioUrl,
                        BannerUrl = d.DesignerProfile.BannerUrl,
                        Certificates = d.DesignerProfile.Certificates,
                        CreateAt = d.DesignerProfile.CreatedAt
                    }
                })
                .FirstOrDefaultAsync();

            if (designDetailDto == null)
                throw new Exception("Design không tồn tại hoặc không thuộc Designer này.");

            return designDetailDto;
        }


        public async Task<List<DesignWithProductInfoDto>> GetDesignsWithProductsPaginationAsync(int page, int pageSize)
        {
            var designs = await _dbContext.Designs
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DesignWithProductInfoDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,
                    DesignImageUrls = d.DesignImages
                    .Select(di => di.Image.ImageUrl)
                    .ToList(),
                    Materials = d.DesignsMaterials
                    .Select(dm => new MaterialDto
                    {
                        MaterialId = dm.MaterialId,
                        MaterialName = dm.Materials.Name,
                        MeterUsed = (decimal)dm.MeterUsed
                    })
                    .ToList(),
                    ProductCount = d.Products.Count,
                    Designer = d.DesignerProfile != null
                        ? new DesignerPublicDto
                        {
                            DesignerId = d.DesignerId,
                            DesignerName = d.DesignerProfile.DesignerName
                        }
                        : null
                })
                .ToListAsync();

            return designs;
        }


        public async Task<List<DesignWithProductInfoDto>> GetDesignsWithDesignerPaginationAsync(Guid designerId, int page, int pageSize)
        {
            var designs = await _dbContext.Designs
                .AsNoTracking()
                .Where(d => d.DesignerId == designerId)
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DesignWithProductInfoDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,
                    DesignImageUrls = d.DesignImages
                    .Select(di => di.Image.ImageUrl)
                    .ToList(),
                    Materials = d.DesignsMaterials
                    .Select(dm => new MaterialDto
                    {
                        MaterialId = dm.MaterialId,
                        MaterialName = dm.Materials.Name,
                        MeterUsed = (decimal)dm.MeterUsed
                    })
                    .ToList(),
                    ProductCount = d.Products.Count,
                    Designer = d.DesignerProfile != null
                        ? new DesignerPublicDto
                        {
                            DesignerId = d.DesignerId,
                            DesignerName = d.DesignerProfile.DesignerName
                        }
                        : null
                })
                .ToListAsync();

            return designs;
        }


        public async Task<List<DesignSummaryDto>> GetDesignsWithoutProductsByDesignerIdAsync(Guid designerId)
        {
            return await _designRepository.GetAll()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(d => d.DesignerId == designerId )
                .Select(d => new DesignSummaryDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,

                    // Only select URLs, not entire image entity
                    DesignImageUrls = d.DesignImages
                        .Select(di => di.Image.ImageUrl)
                        .ToList(),

                    // Keep material selection minimal
                    Materials = d.DesignsMaterials
                        .Select(dm => new MaterialDto
                        {
                            MaterialId = dm.MaterialId,
                            MaterialName = dm.Materials.Name,
                            MeterUsed = dm.MeterUsed
                        }).ToList(),

                    // Select only necessary variant data
                    DesignsVariants = d.DesignsVariants
                        .Select(dv => new DesignVariantsDto
                        {
                            Id = dv.Id,
                            DesignId = dv.DesignId,
                        }).ToList(),
                         // Only select URLs, not entire image entity
                    DrafSketches = d.DraftSketches
                        .Select(di => di.Image.ImageUrl)
                        .ToList(),
                })
                .ToListAsync();
        }


        public async Task<List<DesignWithProductInfoDto>> GetDesignsWithProductsAndDesignerIdAsync(Guid designerId)
        {
            var productWarehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);
            return await _designRepository.GetAll().AsNoTracking()
                .Where(d => d.Products.Any()) // Only designs with products
                .Where(d => d.DesignerId == designerId)
                .Select(d => new DesignWithProductInfoDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    RecycledPercentage = d.RecycledPercentage,
                    ItemTypeName = d.ItemTypes.TypeName,
                    SalePrice = d.SalePrice,
                    DesignImageUrls = d.DesignImages
                        .Select(di => di.Image.ImageUrl)
                        .ToList(),
                    Materials = d.DesignsMaterials
                        .Select(dm => new MaterialDto
                        {
                            MaterialId = dm.MaterialId,
                            MaterialName = dm.Materials.Name,
                            MeterUsed = (decimal)dm.MeterUsed
                        })
                        .ToList(),
                    ProductCount = d.Products.Count,
                })
                .ToListAsync();
        }


        public async Task<List<ProductDto>> GetProductsByDesignAsync(int designId, Guid designerId)
        {
            var productWarehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);

            var products = await _designRepository.GetAll().AsNoTracking()
                .Where(d => d.DesignId == designId && d.DesignerId == designerId)
                .SelectMany(d => d.Products.Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    Price = p.Price,
                    ColorCode = p.ColorCode,
                    SizeId = p.SizeId,
                    SizeName = p.Size.SizeName,
                    QuantityAvailable = p.Inventories
                        .Where(pi => pi.WarehouseId == productWarehouseId)
                        .Select(pi => pi.QuantityAvailable)
                        .FirstOrDefault()
                }))
                .ToListAsync();

            if (!products.Any())
                throw new Exception("Không tìm thấy sản phẩm cho thiết kế này hoặc không thuộc Designer này.");

            return products;
        }

        public async Task<int> GetDefaultProductWarehouseIdForDesigner(Guid designerId)
        {
            var warehouse = await _warehouseRepository.GetAll()
                .FirstOrDefaultAsync(w => w.DesignerId == designerId && w.WarehouseType == "Product");

            if (warehouse == null)
                throw new Exception("Không tìm thấy kho sản phẩm (Product) mặc định cho designer.");

            return warehouse.WarehouseId;
        }

        public async Task<bool> UpdateProductBasicInfoAsync(UpdateProductDto request, Guid designerId)
        {
            // Tìm sản phẩm dựa trên ID
            var design = await _designRepository
                .GetAll()
                .Where(d => d.DesignId == request.DesignId&&d.DesignerId ==designerId)
                .Include(d => d.DesignFeatures)
                .Include(d => d.DesignImages)
                .FirstOrDefaultAsync();

            if (design == null)
            {
                return false; // Sản phẩm không tồn tại
            }

            // Cập nhật thông tin cơ bản
            design.Name = request.Name;
            design.Description = request.Description;
            design.CareInstruction = request.CareInstruction;

            // Cập nhật DesignFeatures
            // Giả định DesignFeatures là một đối tượng duy nhất
            if (design.DesignFeatures == null)
            {
                design.DesignFeatures = new DesignFeature
                {
                    DesignId = request.DesignId,
                    ReduceWaste = request.DesignFeatures.ReduceWaste,
                    LowImpactDyes = request.DesignFeatures.LowImpactDyes,
                    Durable = request.DesignFeatures.Durable,
                    EthicallyManufactured = request.DesignFeatures.EthicallyManufactured
                };
            }
            else
            {
                design.DesignFeatures.ReduceWaste = request.DesignFeatures.ReduceWaste;
                design.DesignFeatures.LowImpactDyes = request.DesignFeatures.LowImpactDyes;
                design.DesignFeatures.Durable = request.DesignFeatures.Durable;
                design.DesignFeatures.EthicallyManufactured = request.DesignFeatures.EthicallyManufactured;
            }

            if (request.DesignImages != null && request.DesignImages.Any())
            {
                var oldImages = _designImageRepository.GetAll().Where(s => s.DesignId == request.DesignId);
                _designImageRepository.RemoveRange(oldImages);
                await _designImageRepository.Commit();

                var uploadResults = await _cloudService.UploadImagesAsync(request.DesignImages);
                var newImages = uploadResults
                    .Where(u => !string.IsNullOrWhiteSpace(u?.SecureUrl?.ToString()))
                    .Select(u => new DesignImage
                    {
                        DesignId = request.DesignId,
                        Image = new Image { ImageUrl = u.SecureUrl.ToString() }
                    }).ToList();

                if (newImages.Any())
                {
                    await _designImageRepository.AddRangeAsync(newImages);
                    await _designImageRepository.Commit();
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _designRepository.Update(design);
            return true;
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
        //        if (imageFiles?.Any() == true)
        //        {
        //            var uploadResults = await _cloudService.UploadImagesAsync(imageFiles);
        //            foreach (var uploadResult in uploadResults)
        //            {
        //                if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
        //                {
        //                    var designImage = new DesignImage
        //                    {
        //                        DesignId = design.DesignId,
        //                        Image = new Image
        //                        {
        //                            ImageUrl = uploadResult.SecureUrl.ToString()
        //                        }
        //                    };

        //    await _designImageRepository.AddAsync(designImage);
        //}
        //                else
        //                {
        //                    Console.WriteLine(" Upload failed or returned null SecureUrl.");
        //                }
        //            }
        //        }

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