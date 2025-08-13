using AutoMapper;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Requests.DessignDraft;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos.DesignDraft;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EcoFashionBackEnd.Services
{
    public class DesignDraftService
    {
        #region injection
        private readonly IRepository<Design, int> _designRepository;
        private readonly IRepository<Designer, int> _designerRepository;
        private readonly IRepository<DesignsMaterial, int> _designMaterialRepository;
        private readonly IRepository<Material, int> _MaterialRepository;
        private readonly IRepository<DesignsVariant, int> _designVariantRepository;
        private readonly IRepository<DesignerMaterialInventory, int> _designerMaterialInventoryRepository;
        private readonly IRepository<DraftPart, int> _draftPartRepository;
        private readonly IRepository<DraftSketch, int> _draftSketchRepository;
        private readonly IRepository<Image, int> _imageRepository;
        private readonly IRepository<ItemTypeSizeRatio, int> _itemTypeSizeRatioRepository;
        private readonly IRepository<ItemType, int> _itemTypeRepository;

        private readonly CloudService _cloudService;
        private readonly IMapper _mapper;

        public DesignDraftService(
            IRepository<Design, int> designRepository,
            IRepository<Designer, int> designerRepository,
            IRepository<DesignsMaterial, int> designMaterialRepository,
            IRepository<Material, int> MaterialRepository,
            IRepository<DesignsVariant, int> designVariantRepository,
            IRepository<DraftPart, int> draftPartRepository,
            IRepository<DraftSketch, int> draftSketchRepository,
            IRepository<Image, int> imageRepository,
            IRepository<ItemTypeSizeRatio, int> itemTypeSizeRatioRepository,
            IRepository<ItemType, int> itemTypeRepository,

            CloudService cloudService,
            IMapper mapper)
        {
            _designRepository = designRepository;
            _designerRepository = designerRepository;
            _designMaterialRepository = designMaterialRepository;
            _MaterialRepository = MaterialRepository;
            _designVariantRepository = designVariantRepository;
            _draftPartRepository = draftPartRepository;
            _draftSketchRepository = draftSketchRepository;
            _imageRepository = imageRepository;
            _itemTypeSizeRatioRepository = itemTypeSizeRatioRepository;
            _itemTypeRepository = itemTypeRepository;

            _cloudService = cloudService;
            _mapper = mapper;
        }
        #endregion
        public async Task<int> CreateDraftDesignAsync(DraftDesignCreateRequest request, Guid designerId)
        {
            // 1. Tạo Design
            var design = new Design
            {
                DesignerId = designerId,
                Name = request.Name,
                Description = request.Description,
                RecycledPercentage = request.RecycledPercentage,
                CreatedAt = DateTime.UtcNow,
                CarbonFootprint = request.TotalCarbon,
                WaterUsage = request.TotalWater,
                WasteDiverted = request.TotalWaste,
                LaborCostPerHour = request.LaborCostPerHour,
                LaborHours = request.LaborHours,
                UnitPrice = request.UnitPrice,
                SalePrice = request.SalePrice,
                ItemTypeId = request.DesignTypeId,
            };

            await _designRepository.AddAsync(design);
            await _designRepository.Commit();

            // 2. Parse DraftPartsJson
            try
            {
                var draftParts = JsonConvert.DeserializeObject<List<DraftPartDto>>(request.DraftPartsJson ?? "[]");
                if (draftParts != null && draftParts.Any())
                {
                    var draftPartEntities = draftParts.Select(part => new DraftPart
                    {
                        DesignId = design.DesignId,
                        Name = part.Name,
                        Length = (decimal)part.Length,
                        Width = (decimal)part.Width,
                        Quantity = part.Quantity,
                        MaterialId = part.MaterialId,
                    }).ToList();

                    await _draftPartRepository.AddRangeAsync(draftPartEntities);
                }
            }
            catch (JsonException ex)
            {
                throw new Exception("Lỗi khi parse DraftPartsJson: phải là một mảng [].", ex);
            }

            // 3. Upload Sketch Images
            if (request.SketchImages != null && request.SketchImages.Any())
            {
                var uploadResults = await _cloudService.UploadImagesAsync(request.SketchImages);
                foreach (var uploadResult in uploadResults)
                {
                    if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
                    {
                        var designDraftImage = new DraftSketch
                        {
                            DesignId = design.DesignId,
                            Image = new Image
                            {
                                ImageUrl = uploadResult.SecureUrl.ToString()
                            }
                        };
                        await _draftSketchRepository.AddAsync(designDraftImage);
                    }
                }
            }

            await _draftPartRepository.Commit();
            await _draftSketchRepository.Commit();

            // 4. Parse MaterialsJson
            try
            {
                var materials = JsonConvert.DeserializeObject<List<DesignMaterialRequest>>(request.MaterialsJson ?? "[]");
                if (materials != null && materials.Any())
                {
                    var newMaterials = materials.Select(m => new DesignsMaterial
                    {
                        DesignId = design.DesignId,
                        MaterialId = m.MaterialId,
                        MeterUsed = (int)m.MeterUsed
                    }).ToList();

                    await _designMaterialRepository.AddRangeAsync(newMaterials);
                    await _designMaterialRepository.Commit();
                }
            }
            catch (JsonException ex)
            {
                throw new Exception("Lỗi khi parse MaterialsJson: phải là một mảng [].", ex);
            }

            return design.DesignId;
        }




        public async Task<List<DesignDraftDto>> GetAllDraftsAsync(Guid designerId)
        {
            return await _designRepository
                .GetAll()
                .AsNoTracking()
                .Where(d => d.DesignerId == designerId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DesignDraftDto
                {
                    DesignId = d.DesignId,
                    Name = d.Name,
                    CreatedAt = d.CreatedAt,
                    Description= d.Description,
                    UnitPrice = d.UnitPrice,
                    SalePrice = d.SalePrice,
                    LaborCostPerHour = d.LaborCostPerHour,  
                    LaborHours  = d.LaborHours,
                    RecycledPercentage = d.RecycledPercentage,
                    TotalCarbon = (float)d.CarbonFootprint,
                    TotalWaste = (float)d.WasteDiverted,
                    TotalWater = (float)d.WasteDiverted,
 
                    // Ảnh sketch
                    SketchImageUrls = d.DraftSketches
                        .Select(ds => ds.Image.ImageUrl)
                        .ToList(),

                    // Danh sách materials
                    Materials = d.DesignsMaterials
                        .Select(m => new DesignMaterialDto
                        {
                            MaterialId = m.MaterialId,
                            MeterUsed = m.MeterUsed,
                            MaterialName = m.Materials.Name
                        }).ToList(),

                    // Danh sách parts
                    DraftParts = d.DraftParts
                        .Select(p => new DraftPartDto
                        {
                            Name = p.Name,
                            Length = (float)p.Length,
                            Width = (float)p.Width,
                            Quantity = p.Quantity,
                            MaterialId = p.MaterialId,
                        }).ToList()
                })
                .ToListAsync();
        }



        public async Task<DraftDesignDetailDto?> GetDraftDetailAsync(int designId, Guid designerId)
        {
            var design = await _designRepository
                .GetAll()
                .AsNoTracking()
                .Where(d => d.DesignId == designId && d.DesignerId == designerId)
                .Include(d => d.DraftParts)
                .Include(d => d.DesignsMaterials)
                    .ThenInclude(dm => dm.Materials)
                .Include(d => d.DraftSketches)
                    .ThenInclude(ds => ds.Image)
                .FirstOrDefaultAsync();

            if (design == null)
                return null;

            return new DraftDesignDetailDto
            {
                DesignId = design.DesignId,
                Name = design.Name,
                Description = design.Description,
                RecycledPercentage = design.RecycledPercentage,
                TotalCarbon = (float)design.CarbonFootprint,
                TotalWater = (float)design.WaterUsage,
                TotalWaste = (float)design.WasteDiverted,

                DraftParts = design.DraftParts.Select(p => new DraftPartDto
                {
                    Name = p.Name,
                    Length = (float)p.Length,
                    Width = (float)p.Width,
                    Quantity = p.Quantity,
                    MaterialId = p.MaterialId,
                }).ToList(),

                Materials = design.DesignsMaterials.Select(m => new DesignMaterialDto
                {
                    MaterialId = m.MaterialId,
                    MaterialName = m.Materials.Name,
                    MeterUsed = m.MeterUsed
                }).ToList(),

                SketchImageUrls = design.DraftSketches
                    .Select(ds => ds.Image.ImageUrl)
                    .ToList()
            };
        }

        public async Task UpdateDraftDesignAsync(  DraftDesignUpdateRequest request, Guid designerId)
        {
         
            // 1. Update Design info
            var design = new Design
            {
                DesignerId = designerId,
                Name = request.Name,
                Description = request.Description,
                RecycledPercentage = request.RecycledPercentage,
                CarbonFootprint = request.TotalCarbon,
                WaterUsage = request.TotalWater,
                WasteDiverted = request.TotalWaste,
                LaborCostPerHour = request.LaborCostPerHour,
                LaborHours = request.LaborHours,
                UnitPrice = request.UnitPrice,
                SalePrice = request.SalePrice,
                ItemTypeId = request.DesignTypeId
            };
            _designRepository.Update(design);
            await _designRepository.Commit();

            // 2. Replace DraftParts
            var oldParts = _draftPartRepository.GetAll().Where(p => p.DesignId == request.DesignId);
            _draftPartRepository.RemoveRange(oldParts);
            await _draftPartRepository.Commit();

            var draftParts = JsonConvert.DeserializeObject<List<DraftPartDto>>(request.DraftPartsJson ?? "[]") ?? new();
            if (draftParts.Any())
            {
                var draftPartEntities = draftParts.Select(p => new DraftPart
                {
                    DesignId = request.DesignId,
                    Name = p.Name,
                    Length = (decimal)p.Length,
                    Width = (decimal)p.Width,
                    Quantity = p.Quantity,
                    MaterialId = p.MaterialId
                }).ToList();
                await _draftPartRepository.AddRangeAsync(draftPartEntities);
                await _draftPartRepository.Commit();
            }

            // 3. Replace Sketch Images (only if new images provided)
            if (request.SketchImages != null && request.SketchImages.Any())
            {
                var oldSketches = _draftSketchRepository.GetAll().Where(s => s.DesignId == request.DesignId);
                _draftSketchRepository.RemoveRange(oldSketches);
                await _draftSketchRepository.Commit();

                var uploadResults = await _cloudService.UploadImagesAsync(request.SketchImages);
                var newSketches = uploadResults
                    .Where(u => !string.IsNullOrWhiteSpace(u?.SecureUrl?.ToString()))
                    .Select(u => new DraftSketch
                    {
                        DesignId = request.DesignId,
                        Image = new Image { ImageUrl = u.SecureUrl.ToString() }
                    }).ToList();

                if (newSketches.Any())
                {
                    await _draftSketchRepository.AddRangeAsync(newSketches);
                    await _draftSketchRepository.Commit();
                }
            }

            // 4. Replace Materials
            var oldMaterials = _designMaterialRepository.GetAll().Where(m => m.DesignId == request.DesignId);
            _designMaterialRepository.RemoveRange(oldMaterials);
            await _designMaterialRepository.Commit();

            var materials = JsonConvert.DeserializeObject<List<DesignMaterialRequest>>(request.MaterialsJson ?? "[]") ?? new();
            if (materials.Any())
            {
                var newMaterials = materials.Select(m => new DesignsMaterial
                {
                    DesignId = request.DesignId,
                    MaterialId = m.MaterialId,
                    MeterUsed = (int)m.MeterUsed
                }).ToList();
                await _designMaterialRepository.AddRangeAsync(newMaterials);
                await _designMaterialRepository.Commit();
            }
        }

        public async Task<FabricUsageResponse> CalculateFabricUsageByMaterialAsync(int designId)
        {
            var design = await _designRepository.GetByIdAsync(designId);
            if (design == null) throw new Exception("Design không tồn tại.");

            var materials = _designMaterialRepository
                .GetAll()
                .Where(m => m.DesignId == designId)
                .Select(m => new { m.MaterialId, m.MeterUsed, m.Materials.Name })
                .ToList();

            if (!materials.Any()) return new FabricUsageResponse
            {
                DesignId = design.DesignId,
                DesignName = design.Name,
                MaterialsUsage = new List<MaterialFabricUsageDto>(),
            };

            var variants = _designVariantRepository
                .GetAll()
                .Where(v => v.DesignId == designId)
                .ToList();

            if (!variants.Any()) return new FabricUsageResponse
            {
                DesignId = design.DesignId,
                DesignName = design.Name,
                MaterialsUsage = new List<MaterialFabricUsageDto>(),
            };

            var usageMap = new Dictionary<int, decimal>();

            foreach (var variant in variants)
            {
                var ratio = _itemTypeSizeRatioRepository
                    .GetAll()
                    .Where(r => r.ItemTypeId == design.ItemTypeId && r.SizeId == variant.SizeId)
                    .Select(r => r.Ratio)
                    .FirstOrDefault();

                if (ratio == 0) continue;

                foreach (var material in materials)
                {
                    var meterForVariant = material.MeterUsed
                        * (decimal)ratio
                        * variant.Quantity;

                    if (!usageMap.ContainsKey(material.MaterialId))
                        usageMap[material.MaterialId] = 0;

                    usageMap[material.MaterialId] += meterForVariant;
                }
            }

            var resultList = usageMap.Select(kvp => new MaterialFabricUsageDto
            {
                MaterialId = kvp.Key,
                TotalMeters = kvp.Value
            }).ToList();

            return new FabricUsageResponse
            {
                DesignId = design.DesignId,
                DesignName = design.Name,
                MaterialsUsage = resultList,
            };
        }


        public async Task<IEnumerable<ItemTypeDto>> GetAllItemTypesAsync()
        {
            return await _itemTypeRepository
                .GetAll()
                .AsNoTracking()
                .Select(it => new ItemTypeDto
                {
                    ItemTypeId = it.ItemTypeId,
                    TypeName = it.TypeName,
                    Description = it.Description
                })
                .ToListAsync();
        }


        public async Task<IEnumerable<ItemTypeSizeRatioDto>> GetAllItemTypeSizeRatiosAsync()
        {
            return await _itemTypeSizeRatioRepository
                .GetAll()
                .AsNoTracking()
                .Include(itsr => itsr.ItemType)
                .Include(itsr => itsr.Size)
                .Select(itsr => new ItemTypeSizeRatioDto
                {
                    Id = itsr.Id,
                    TypeName = itsr.ItemType.TypeName,
                    SizeName = itsr.Size.SizeName,
                    Ratio = itsr.Ratio
                })
                .ToListAsync();
        }


    }
}

