using EcoFashionBackEnd.Common.Payloads.Requests.Product;
using EcoFashionBackEnd.Common.Payloads.Responses.Product;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

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
        private readonly IRepository<DesignerMaterialInventory, int> _designerMaterialInventoryRepository;
        private readonly IRepository<ItemTypeSizeRatio, int> _itemTypeSizeRatioRepository;
        private readonly IRepository<Warehouse, int> _warehouseRepository;
        private readonly InventoryService _inventoryService;
       
        public ProductService(
           IRepository<Product, int> productRepository,
           IRepository<Design, int> designRepository,
           IRepository<Designer, int> designerRepository,
           IRepository<DesignsMaterial, int> designMaterialRepository,
           IRepository<Material, int> MaterialRepository,
           IRepository<DesignsVariant, int> designVariantRepository,
           IRepository<ItemTypeSizeRatio, int> itemTypeSizeRatioRepository,
           IRepository<Warehouse, int> warehouseRepository,
           InventoryService inventoryService
            )
        {
            _productRepository = productRepository;
            _designRepository = designRepository;
            _designerRepository = designerRepository;
            _designMaterialRepository = designMaterialRepository;
            _MaterialRepository = MaterialRepository;
            _designVariantRepository = designVariantRepository;
            _itemTypeSizeRatioRepository = itemTypeSizeRatioRepository;
            _warehouseRepository = warehouseRepository;
            _inventoryService = inventoryService;
        }


        public async Task<List<int>> CreateProductsAsync(ProductCreateRequest request, Guid designerId)
        {
            var warehouseId = await GetDefaultProductWarehouseIdForDesigner(designerId);

            var design = await _designRepository.GetAll()
                .Include(d => d.DesignsMaterials).ThenInclude(dm => dm.Materials)
                //include variant 
                .FirstOrDefaultAsync(d => d.DesignId == request.DesignId);
            if (design == null)
                throw new Exception("Design không tồn tại");

            var totalUsageMap = new Dictionary<int, decimal>();

            foreach (var variantReq in request.Variants)
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

            // Trừ kho vật liệu
            await _inventoryService.DeductMaterialsAsync(designerId, totalUsageMap);

            var createdProductIds = new List<int>();
            var productInventoryChanges = new List<(int productId, int warehouseId, int quantity)>();

            foreach (var variantReq in request.Variants)
            {
                var sizeRatio = await _itemTypeSizeRatioRepository.GetAll()
                    .Where(r => r.SizeId == variantReq.SizeId && r.ItemTypeId == design.ItemTypeId)
                    .Select(r => r.Ratio)
                    .FirstOrDefaultAsync();

                var sku = $"{design.DesignId}-S{variantReq.SizeId}-C{variantReq.ColorCode.Replace(" ", "").ToUpper()}";

                var product = new Product
                {
                    DesignId = request.DesignId,
                    SKU = sku,
                    Price = (decimal)design.SalePrice,
                    ColorCode = variantReq.ColorCode,
                    SizeId = variantReq.SizeId,
                };

                await _productRepository.AddAsync(product);
                createdProductIds.Add(product.ProductId);

                // Cộng kho product tương ứng
                productInventoryChanges.Add((product.ProductId, warehouseId, variantReq.Quantity));
            }

            await _productRepository.Commit();

            // Cập nhật kho sản phẩm theo list product vừa tạo
            await _inventoryService.AddProductInventoriesAsync(productInventoryChanges);

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
