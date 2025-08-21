using EcoFashionBackEnd.Common.Payloads.Requests.Product;
using EcoFashionBackEnd.Common.Payloads.Requests.Variant;
using EcoFashionBackEnd.Common.Payloads.Responses.Product;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EcoFashionBackEnd.Dtos.DesignShow;
using EcoFashionBackEnd.Helpers;

namespace EcoFashionBackEnd.Services
{
    public class ProductService
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Design, int> _designRepository;
        private readonly IRepository<Designer, int> _designerRepository;
        private readonly IRepository<DesignsMaterial, int> _designMaterialRepository;
        private readonly IRepository<Material, int> _MaterialRepository;
        private readonly IRepository<DesignsVariant, int> _designVariantRepository;
        private readonly IRepository<DesignImage, int> _designImageRepository;
        private readonly IRepository<DesignerMaterialInventory, int> _designerMaterialInventoryRepository;
        private readonly IRepository<ItemTypeSizeRatio, int> _itemTypeSizeRatioRepository;
        private readonly IRepository<Warehouse, int> _warehouseRepository;
        private readonly CloudService _cloudService;

        private readonly InventoryService _inventoryService;
       
        public ProductService(
           IRepository<Product, int> productRepository,
           IRepository<Design, int> designRepository,
           IRepository<Designer, int> designerRepository,
           IRepository<DesignsMaterial, int> designMaterialRepository,
           IRepository<Material, int> MaterialRepository,
           IRepository<DesignsVariant, int> designVariantRepository,
           IRepository<DesignImage, int> designImageRepository,
           IRepository<ItemTypeSizeRatio, int> itemTypeSizeRatioRepository,
           IRepository<Warehouse, int> warehouseRepository,
            CloudService cloudService,

           InventoryService inventoryService
            )
        {
            _productRepository = productRepository;
            _designRepository = designRepository;
            _designerRepository = designerRepository;
            _designMaterialRepository = designMaterialRepository;
            _MaterialRepository = MaterialRepository;
            _designVariantRepository = designVariantRepository;
            _designImageRepository = designImageRepository;
            _itemTypeSizeRatioRepository = itemTypeSizeRatioRepository;
            _warehouseRepository = warehouseRepository;
            _cloudService = cloudService;
            _inventoryService = inventoryService;
        }


        public async Task<List<int>> CreateProductsAsync(ProductCreateRequest request, Guid designerId)
        {
            // Parse JSON string sang object list
            List<DesignsVariantCreateRequest> variants;
            try
            {
                variants = JsonConvert.DeserializeObject<List<DesignsVariantCreateRequest>>(request.Variants);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Variants JSON không hợp lệ", ex);
            }

            if (variants == null || !variants.Any())
                throw new ArgumentException("Variants không được để trống");

            var warehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);

            var design = await _designRepository.GetAll()
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                .FirstOrDefaultAsync(d => d.DesignId == request.DesignId);
            if (design == null)
                throw new Exception("Design không tồn tại");

            var totalUsageMap = new Dictionary<int, decimal>();

            // Tính nguyên liệu
            foreach (var variantReq in variants)
            {
                var sizeRatio = await _itemTypeSizeRatioRepository.GetAll()
                    .Where(r => r.SizeId == variantReq.SizeId && r.ItemTypeId == design.ItemTypeId)
                    .Select(r => r.Ratio)
                    .FirstOrDefaultAsync();

                foreach (var dm in design.DesignsMaterials)
                {
                    var meterUsed = dm.MeterUsed * (decimal)sizeRatio * variantReq.Quantity;
                    if (!totalUsageMap.ContainsKey(dm.MaterialId))
                        totalUsageMap[dm.MaterialId] = 0;
                    totalUsageMap[dm.MaterialId] += meterUsed;
                }
            }

            //await _inventoryService.DeductMaterialsAsync(designerId, totalUsageMap);

            var createdProductIds = new List<int>();
            var productInventoryChanges = new List<(int productId, int warehouseId, int quantity)>();
            // 3️ Tạo sản phẩm cho từng variant
            foreach (var variant in variants)
            {
                var basicColorName = ColorExchange.ClassifyColorAdvanced(variant.ColorCode);
                var sku = $"{design.DesignId}-S{variant.SizeId}-C{basicColorName.Replace(" ", "").ToUpper()}";

                var existingProduct = await _productRepository
                    .GetAll()
                    .FirstOrDefaultAsync(p => p.SKU == sku);

                if (existingProduct != null)
                {
                    // Nếu đã tồn tại thì chỉ nhập thêm inventory
                    productInventoryChanges.Add((existingProduct.ProductId, warehouseId, variant.Quantity));
                    continue;
                }

                // Nếu chưa tồn tại → tạo mới
                var product = new Product
                {
                    DesignId = design.DesignId,
                    SKU = sku,
                    Price = (decimal)design.SalePrice,
                    ColorCode = variant.ColorCode,
                    SizeId = variant.SizeId,
                };

                await _productRepository.AddAsync(product);
                await _productRepository.Commit(); // Commit ngay để có ProductId cho inventory

                createdProductIds.Add(product.ProductId);
                productInventoryChanges.Add((product.ProductId, warehouseId, variant.Quantity));
            }

            // Upload ảnh nếu có
            if (request.Images?.Any() == true)
            {
                var uploadResults = await _cloudService.UploadImagesAsync(request.Images);
                foreach (var uploadResult in uploadResults)
                {
                    if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
                    {
                        var designImage = new DesignImage
                        {
                            DesignId = design.DesignId,
                            Image = new Image
                            {
                                ImageUrl = uploadResult.SecureUrl.ToString()
                            }
                        };

                        await _designImageRepository.AddAsync(designImage);
                    }
                }
                await _designImageRepository.Commit();
            }

            return createdProductIds;
        }

        public async Task<List<int>> CreateProductsWithExistVariantAsync(CreateProductsFromDesignRequest request, Guid designerId)
        {
            var warehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);

            var design = await _designRepository.GetAll()
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                .Include(d => d.DesignsVariants) // Lấy luôn variant có sẵn
                .FirstOrDefaultAsync(d => d.DesignId == request.DesignId);

            if (design == null)
                throw new Exception("Design không tồn tại");

            if (design.DesignsVariants == null || !design.DesignsVariants.Any())
                throw new Exception("Design chưa có variant nào");

            // Map DB variants sang list
            var variants = design.DesignsVariants.Select(v => new
            {
                v.SizeId,
                v.ColorCode,
                v.Quantity
            }).ToList();

            var totalUsageMap = new Dictionary<int, decimal>();

            // 1️ Tính nguyên liệu cần dùng cho toàn bộ variants
            foreach (var variant in variants)
            {
                var sizeRatio = await _itemTypeSizeRatioRepository.GetAll()
                    .Where(r => r.SizeId == variant.SizeId && r.ItemTypeId == design.ItemTypeId)
                    .Select(r => r.Ratio)
                    .FirstOrDefaultAsync();

                foreach (var dm in design.DesignsMaterials)
                {
                    var meterUsed = dm.MeterUsed * (decimal)sizeRatio * variant.Quantity;
                    if (!totalUsageMap.ContainsKey(dm.MaterialId))
                        totalUsageMap[dm.MaterialId] = 0;
                    totalUsageMap[dm.MaterialId] += meterUsed;
                }
            }

            // 2️ Trừ kho nguyên liệu
            await _inventoryService.DeductMaterialsAsync(designerId, totalUsageMap);

            var createdProductIds = new List<int>();
            var productInventoryChanges = new List<(int productId, int warehouseId, int quantity)>();

            // 3️ Tạo sản phẩm cho từng variant
            foreach (var variant in variants)
            {
                var basicColorName = ColorExchange.ClassifyColorAdvanced(variant.ColorCode);
                var sku = $"{design.DesignId}-S{variant.SizeId}-C{basicColorName.Replace(" ", "").ToUpper()}";

                var existingProduct = await _productRepository
                    .GetAll()
                    .FirstOrDefaultAsync(p => p.SKU == sku);

                if (existingProduct != null)
                {
                    // Nếu đã tồn tại thì chỉ nhập thêm inventory
                    productInventoryChanges.Add((existingProduct.ProductId, warehouseId, variant.Quantity));
                    continue;
                }

                // Nếu chưa tồn tại → tạo mới
                var product = new Product
                {
                    DesignId = design.DesignId,
                    SKU = sku,
                    Price = (decimal)design.SalePrice,
                    ColorCode = variant.ColorCode,
                    SizeId = variant.SizeId,
                };

                await _productRepository.AddAsync(product);
                await _productRepository.Commit(); // Commit ngay để có ProductId cho inventory

                createdProductIds.Add(product.ProductId);
                productInventoryChanges.Add((product.ProductId, warehouseId, variant.Quantity));
            }
            // 4️ Lưu sản phẩm + cộng kho
            await _productRepository.Commit();
            await _inventoryService.AddProductInventoriesAsync(productInventoryChanges);

            // 5️⃣ Upload ảnh (nếu có)
            if (request.Images?.Any() == true)
            {
                var uploadResults = await _cloudService.UploadImagesAsync(request.Images);
                foreach (var uploadResult in uploadResults)
                {
                    if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
                    {
                        var designImage = new DesignImage
                        {
                            DesignId = design.DesignId,
                            Image = new Image
                            {
                                ImageUrl = uploadResult.SecureUrl.ToString()
                            }
                        };

                        await _designImageRepository.AddAsync(designImage);
                    }
                }
                await _designImageRepository.Commit();
            }

            return createdProductIds;
        }






        public async Task<int> GetDefaultProductWarehouseIdForDesigner(Guid designerId)
        {
            var warehouse = await _warehouseRepository.GetAll()
                .FirstOrDefaultAsync(w => w.DesignerId == designerId && w.WarehouseType == "Product");

            if (warehouse == null)
                throw new Exception("Không tìm thấy kho sản phẩm (Product) mặc định cho designer.");

            return warehouse.WarehouseId;
        }


    }
}
