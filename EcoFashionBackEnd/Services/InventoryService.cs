using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class InventoryService
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<ProductInventory, int> _productInventoryRepository;
        private readonly IRepository<DesignerMaterialInventory, int> _designerMaterialInventory;


        public InventoryService(
          IRepository<Product, int> productRepository,
          IRepository<ProductInventory, int> productInventoryRepository,
          IRepository<DesignerMaterialInventory, int> designerMaterialInventory

          )
        {
            _productRepository = productRepository;
            _productInventoryRepository = productInventoryRepository;
            _designerMaterialInventory = designerMaterialInventory;
        }

        public async Task AddProductInventoriesAsync(List<(int productId, int warehouseId, int quantity)> changes)
        {
            var now = DateTime.UtcNow;
            // Bước 1: Chuẩn bị dữ liệu và tối ưu hóa truy vấn
            // Lấy tất cả các ProductId và WarehouseId duy nhất từ danh sách thay đổi.
            // Lấy hết tất cả các inventory cần update cùng lúc để giảm số query
            var productIds = changes.Select(c => c.productId).Distinct().ToList();
            var warehouseIds = changes.Select(c => c.warehouseId).Distinct().ToList();

            var existingInventories = await _productInventoryRepository.GetAll()
                .Where(pi => productIds.Contains(pi.ProductId) && warehouseIds.Contains(pi.WarehouseId))
                .ToListAsync();
            // Bước 2: Xử lý từng thay đổi trong bộ nhớ
            // Duyệt qua từng thay đổi để cập nhật hoặc thêm mới tồn kho.
            foreach (var change in changes)
            {
                // Tìm bản ghi tồn kho tương ứng trong danh sách đã tải.
                var inventory = existingInventories
                    .FirstOrDefault(pi => pi.ProductId == change.productId && pi.WarehouseId == change.warehouseId);

                if (inventory == null)
                {
                    // Nếu không tìm thấy, tạo bản ghi tồn kho mới.
                    inventory = new ProductInventory
                    {
                        ProductId = change.productId,
                        WarehouseId = change.warehouseId,
                        QuantityAvailable = change.quantity,
                        LastUpdated = now,
                    };
                    // Thêm bản ghi mới vào repository để theo dõi.
                    await _productInventoryRepository.AddAsync(inventory);
                    // Thêm vào danh sách đã tải để sử dụng lại nếu cần.
                    existingInventories.Add(inventory);
                }
                else
                {
                    // Nếu tồn kho đã có, cập nhật số lượng và thời gian.
                    inventory.QuantityAvailable += change.quantity;
                    inventory.LastUpdated = now;
                    // Cập nhật bản ghi vào repository để theo dõi thay đổi.
                    _productInventoryRepository.Update(inventory);
                }
            }
            // Bước 3: Lưu tất cả thay đổi vào database
            await _productInventoryRepository.Commit();
        }

        public async Task DeductMaterialsAsync(Guid designerId, Dictionary<int, decimal> usageMap)
        {
            // Bước 1: Chuẩn bị dữ liệu và tối ưu hóa truy vấn
            // Lấy danh sách các MaterialId cần trừ từ dictionary.

            var materialIds = usageMap.Keys.ToList();
            // Thực hiện một truy vấn DUY NHẤT để lấy tất cả các bản ghi tồn kho vật liệu
            // của nhà thiết kế cụ thể và các vật liệu liên quan.
            var inventories = await _designerMaterialInventory.GetAll()
                .Where(i => i.DesignerId == designerId && materialIds.Contains(i.MaterialId))
                .ToListAsync();
            // Bước 2: Xử lý từng vật liệu cần trừ
            foreach (var materialId in usageMap.Keys)
            {

                // Tìm bản ghi tồn kho vật liệu tương ứng trong bộ nhớ.
                var inventory = inventories.FirstOrDefault(i => i.MaterialId == materialId);
                // Kiểm tra logic nghiệp vụ:
                if (inventory == null)// Nếu không tìm thấy, báo lỗi.
                    throw new Exception($"Không tìm thấy kho vật liệu MaterialId={materialId} của designer");

                var requiredQty = (int)usageMap[materialId];
                if (inventory.Quantity < requiredQty)// Nếu số lượng tồn kho không đủ, báo lỗi.
                    throw new Exception($"Kho vật liệu không đủ cho MaterialId={materialId}");

                // Nếu hợp lệ, trừ số lượng và đánh dấu để cập nhật.
                inventory.Quantity -= requiredQty;
                _designerMaterialInventory.Update(inventory);
            }
            // Bước 3: Lưu tất cả thay đổi vào database
            await _designerMaterialInventory.Commit();
        }

    }
}
