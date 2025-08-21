using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoFashionBackEnd.Services
{
    public class InventoryService
    {
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<ProductInventory, int> _productInventoryRepository;
        private readonly IRepository<DesignerMaterialInventory, int> _designerMaterialInventory;
        private readonly IRepository<MaterialInventoryTransaction, int> _materialInventoryTransactionRepository;
        private readonly IRepository<ProductInventoryTransaction, int> _productInventoryTransactionRepository;

        private readonly IRepository<Warehouse, int> _warehouseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventoryService(
          IRepository<Product, int> productRepository,
          IRepository<ProductInventory, int> productInventoryRepository,
          IRepository<DesignerMaterialInventory, int> designerMaterialInventory,
          IRepository<MaterialInventoryTransaction, int> materialInventoryTransactionRepository,
          IRepository<ProductInventoryTransaction, int> productInventoryTransactionRepository,

          IRepository<Warehouse, int> warehouseRepository,
        IHttpContextAccessor httpContextAccessor
          )
        {
            _productRepository = productRepository;
            _productInventoryRepository = productInventoryRepository;
            _designerMaterialInventory = designerMaterialInventory;
            _materialInventoryTransactionRepository = materialInventoryTransactionRepository;
            _productInventoryTransactionRepository = productInventoryTransactionRepository;
            _warehouseRepository = warehouseRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddProductInventoriesAsync(List<(int productId, int warehouseId, int quantity)> changes)
        {
            var now = DateTime.UtcNow;
            // Lấy User ID từ HttpContext
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                // Xử lý trường hợp không tìm thấy UserId (ví dụ: yêu cầu không xác thực)
                throw new UnauthorizedAccessException("Người dùng không được xác thực.");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                // Xử lý trường hợp UserId không hợp lệ (ví dụ: không phải số nguyên)
                throw new InvalidOperationException("ID người dùng không hợp lệ.");
            }

            // Bước 1: Chuẩn bị dữ liệu và tối ưu hóa truy vấn
            // Lấy tất cả các ProductId và WarehouseId duy nhất từ danh sách thay đổi.
            var productIds = changes.Select(c => c.productId).Distinct().ToList();
            var warehouseIds = changes.Select(c => c.warehouseId).Distinct().ToList();

            // Sử dụng ToDictionaryAsync để tối ưu hóa việc tìm kiếm sau này.
            var existingInventories = await _productInventoryRepository.GetAll()
                .Where(pi => productIds.Contains(pi.ProductId) && warehouseIds.Contains(pi.WarehouseId))
                .ToDictionaryAsync(pi => (pi.ProductId, pi.WarehouseId)); // Khóa là một Tuple để duy nhất

            var newInventories = new List<ProductInventory>();
            var newTransactions = new List<ProductInventoryTransaction>();

            // Bước 2: Xử lý từng thay đổi trong bộ nhớ
            // Duyệt qua từng thay đổi để cập nhật hoặc thêm mới tồn kho.
            foreach (var change in changes)
            {
                var key = (change.productId, change.warehouseId);

                if (existingInventories.TryGetValue(key, out var inventory))
                {
                    // Nếu tồn kho đã có, cập nhật số lượng và thời gian.
                    var oldQuantity = inventory.QuantityAvailable;
                    inventory.QuantityAvailable += change.quantity;
                    inventory.LastUpdated = now;

                    // Bổ sung: Tạo bản ghi lịch sử giao dịch.
                    var transaction = new ProductInventoryTransaction
                    {
                        InventoryId = inventory.InventoryId,
                        QuantityChanged = change.quantity,
                        TransactionType = "Restock", // Có thể là Restock, Return, etc.
                        Notes = $"Nhập kho sản phẩm. Số lượng cũ: {oldQuantity}",
                        PerformedByUserId = userId,
                        TransactionDate = now,
                    };
                    newTransactions.Add(transaction);
                }
                else
                {
                    // Nếu không tìm thấy, tạo bản ghi tồn kho mới và bản ghi lịch sử.
                    var newInventory = new ProductInventory
                    {
                        ProductId = change.productId,
                        WarehouseId = change.warehouseId,
                        QuantityAvailable = change.quantity,
                        LastUpdated = now,
                    };
                    newInventories.Add(newInventory);
                }
            }

            // Thêm các bản ghi tồn kho mới vào repository.
            await _productInventoryRepository.AddRangeAsync(newInventories);

            // Thêm các bản ghi giao dịch mới vào repository.
            await _productInventoryTransactionRepository.AddRangeAsync(newTransactions);

            // Bước 3: Lưu tất cả thay đổi vào database trong một giao dịch duy nhất
            await _productInventoryRepository.Commit();
        }

        public async Task DeductMaterialsAsync(Guid designerId, Dictionary<int, decimal> usageMap)
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                // Xử lý trường hợp không tìm thấy UserId (ví dụ: yêu cầu không xác thực)
                throw new UnauthorizedAccessException("Người dùng không được xác thực.");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                // Xử lý trường hợp UserId không hợp lệ (ví dụ: không phải số nguyên)
                throw new InvalidOperationException("ID người dùng không hợp lệ.");
            }
            var warehouse = await _warehouseRepository
             .FindByCondition(w => w.DesignerId == designerId && w.WarehouseType == "Material")
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                throw new Exception($"Không tìm thấy kho vật liệu cho designerId={designerId}");
            }

            var materialIds = usageMap.Keys.ToList();


            var inventories = await _designerMaterialInventory.GetAll()
                .Where(i => i.WarehouseId == warehouse.WarehouseId && materialIds.Contains(i.MaterialId))
                .ToDictionaryAsync(i => i.MaterialId);

            // Bước 2: Xử lý từng vật liệu cần trừ
            foreach (var materialId in usageMap.Keys)
            {
                var requiredQty = usageMap[materialId];


                if (!inventories.TryGetValue(materialId, out var inventory))
                {
                    throw new Exception($"Không tìm thấy kho vật liệu MaterialId={materialId} của designer");
                }


                if (inventory.Quantity < requiredQty)
                {
                    throw new Exception($"Kho vật liệu không đủ cho MaterialId={materialId}. Yêu cầu: {requiredQty}, Tồn: {inventory.Quantity}");
                }

                var originalQuantity = inventory.Quantity;
                inventory.Quantity -= requiredQty;


                var transaction = new MaterialInventoryTransaction
                {
                    InventoryId = inventory.InventoryId,
                    QuantityChanged = -requiredQty,
                    PerformedByUserId = userId,
                    TransactionType = "Usage",
                    Notes = $"Trừ vật liệu cho sản phẩm",
                };
                _materialInventoryTransactionRepository.AddAsync(transaction);

                _designerMaterialInventory.Update(inventory);
            }


            await _designerMaterialInventory.Commit();
        }

    }
}
