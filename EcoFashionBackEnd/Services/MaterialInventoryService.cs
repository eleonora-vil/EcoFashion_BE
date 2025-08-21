using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Dtos.Warehouse;

namespace EcoFashionBackEnd.Services
{
    public class MaterialInventoryService
    {
        private readonly AppDbContext _dbContext;

        public MaterialInventoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MaterialStockDto>> GetStocksAsync(Guid? supplierId, int? materialId, int? warehouseId)
        {
            var query = _dbContext.MaterialStocks
                .Include(s => s.Material)
                .ThenInclude(m => m.MaterialImages)
                .ThenInclude(mi => mi.Image)
                .Include(s => s.Warehouse)
                .AsQueryable();

            // Only show stocks for materials that are approved and available
            query = query.Where(s => s.Material != null && s.Material.IsAvailable && s.Material.ApprovalStatus == "Approved");

            if (materialId.HasValue) query = query.Where(s => s.MaterialId == materialId.Value);
            if (warehouseId.HasValue) query = query.Where(s => s.WarehouseId == warehouseId.Value);
            if (supplierId.HasValue) query = query.Where(s => s.Warehouse!.SupplierId == supplierId);

            var list = await query.ToListAsync();
            return list.Select(s => new MaterialStockDto
            {
                StockId = s.StockId,
                MaterialId = s.MaterialId,
                WarehouseId = s.WarehouseId,
                QuantityOnHand = s.QuantityOnHand,
                MinThreshold = s.MinThreshold,
                LastUpdated = s.LastUpdated,
                Note = s.Note,
                MaterialName = s.Material?.Name,
                WarehouseName = s.Warehouse?.Name,
                Unit = "mét", // display unit for materials
                ImageUrl = s.Material?.MaterialImages?.Select(mi => mi.Image!.ImageUrl).FirstOrDefault(),
                QuantityAvailable = s.Material?.QuantityAvailable ?? 0,
                PricePerUnit = s.Material?.PricePerUnit ?? 0
            }).ToList();
        }

        public async Task<List<MaterialStockTransactionDto>> GetTransactionsAsync(Guid? supplierId, int? materialId, int? warehouseId, string? type, DateTime? from, DateTime? to)
        {
            var query = _dbContext.MaterialStockTransactions
                .Include(t => t.Material)!
                    .ThenInclude(m => m.MaterialImages)!
                    .ThenInclude(mi => mi.Image)
                .Include(t => t.Warehouse)
                .AsQueryable();

            if (materialId.HasValue) query = query.Where(t => t.MaterialId == materialId.Value);
            if (warehouseId.HasValue) query = query.Where(t => t.WarehouseId == warehouseId.Value);
            if (!string.IsNullOrWhiteSpace(type)) 
            {
                if (Enum.TryParse<MaterialTransactionType>(type, true, out var transactionType))
                {
                    query = query.Where(t => t.TransactionType == transactionType);
                }
            }
            if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
            if (supplierId.HasValue) query = query.Where(t => t.Warehouse!.SupplierId == supplierId);

            var list = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            // Join supplier names via Warehouses
            var warehouseIdToSupplierName = await _dbContext.Warehouses
                .GroupJoin(_dbContext.Suppliers, w => w.SupplierId, s => s.SupplierId, (w, sg) => new { w.WarehouseId, SupplierName = sg.Select(x => x.SupplierName).FirstOrDefault() })
                .ToDictionaryAsync(x => x.WarehouseId, x => x.SupplierName);

            return list.Select(t => new MaterialStockTransactionDto
            {
                TransactionId = t.TransactionId,
                MaterialId = t.MaterialId,
                WarehouseId = t.WarehouseId,
                TransactionType = t.TransactionType.ToString(),
                QuantityChange = t.QuantityChange,
                BeforeQty = t.BeforeQty,
                AfterQty = t.AfterQty,
                Unit = t.Unit,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                Note = t.Note,
                CreatedByUserId = t.CreatedByUserId,
                CreatedAt = t.CreatedAt,
                MaterialName = t.Material?.Name,
                WarehouseName = t.Warehouse?.Name,
                SupplierName = warehouseIdToSupplierName.TryGetValue(t.WarehouseId, out var sn) ? sn : null,
                ImageUrl = t.Material?.MaterialImages?.Select(mi => mi.Image!.ImageUrl).FirstOrDefault(),
                // Expose WarehouseType for FE filters/search
                // Map from entity (column Type)
                // Using null-conditional to avoid NRE if not included
                WarehouseType = t.Warehouse?.WarehouseType
            }).ToList();
        }

        public async Task<bool> ReceiveAsync(int materialId, int warehouseId, decimal quantity, string? unit, string? note, string? referenceType, string? referenceId, int? userId)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive");

            using var tx = await _dbContext.Database.BeginTransactionAsync();

            var stock = await _dbContext.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialId == materialId && s.WarehouseId == warehouseId);
            if (stock == null)
            {
                stock = new MaterialStock
                {
                    MaterialId = materialId,
                    WarehouseId = warehouseId,
                    QuantityOnHand = 0,
                    MinThreshold = 0
                };
                _dbContext.MaterialStocks.Add(stock);
                await _dbContext.SaveChangesAsync();
            }

            var before = stock.QuantityOnHand;
            var after = before + quantity;

            var tr = new MaterialStockTransaction
            {
                MaterialId = materialId,
                WarehouseId = warehouseId,
                TransactionType = MaterialTransactionType.SupplierReceipt,
                QuantityChange = quantity,
                BeforeQty = before,
                AfterQty = after,
                Unit = unit,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Note = note,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.MaterialStockTransactions.Add(tr);

            stock.QuantityOnHand = after;
            stock.LastUpdated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Update Material.QuantityAvailable as sum of stocks
            var total = await _dbContext.MaterialStocks
                .Where(s => s.MaterialId == materialId)
                .SumAsync(s => s.QuantityOnHand);
            var material = await _dbContext.Materials.FindAsync(materialId);
            if (material != null)
            {
                material.QuantityAvailable = (int)total;
                material.LastUpdated = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return true;
        }

        /// <summary>
        /// Trừ inventory khi có order được hoàn thành (payment success)
        /// </summary>
        /// <param name="materialId">ID của material cần trừ</param>
        /// <param name="warehouseId">ID của warehouse (thường là warehouse của supplier)</param>
        /// <param name="quantity">Số lượng cần trừ (số dương)</param>
        /// <param name="unit">Đơn vị (mét, kg, etc.)</param>
        /// <param name="note">Ghi chú</param>
        /// <param name="referenceType">Loại tham chiếu (ví dụ: "Order")</param>
        /// <param name="referenceId">ID của order</param>
        /// <param name="userId">ID user thực hiện (có thể null cho system)</param>
        /// <returns>True nếu thành công</returns>
        public async Task<bool> DeductAsync(int materialId, int warehouseId, decimal quantity, string? unit, string? note, string? referenceType, string? referenceId, int? userId)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive");

            using var tx = await _dbContext.Database.BeginTransactionAsync();

            var stock = await _dbContext.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialId == materialId && s.WarehouseId == warehouseId);
            
            if (stock == null)
            {
                throw new InvalidOperationException($"No stock found for MaterialId {materialId} in WarehouseId {warehouseId}");
            }

            var before = stock.QuantityOnHand;
            var after = before - quantity;

            // Cho phép âm số (overdraft) nhưng log warning
            if (after < 0)
            {
                // Log warning nhưng vẫn tiếp tục
                Console.WriteLine($"WARNING: Stock going negative for MaterialId {materialId}. Before: {before}, After: {after}");
            }

            var tr = new MaterialStockTransaction
            {
                MaterialId = materialId,
                WarehouseId = warehouseId,
                TransactionType = MaterialTransactionType.CustomerSale, // Sử dụng enum thay vì "OrderSale"
                QuantityChange = -quantity, // Số âm cho deduction
                BeforeQty = before,
                AfterQty = after,
                Unit = unit,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Note = note,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.MaterialStockTransactions.Add(tr);

            stock.QuantityOnHand = after;
            stock.LastUpdated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Update Material.QuantityAvailable as sum of stocks
            var total = await _dbContext.MaterialStocks
                .Where(s => s.MaterialId == materialId)
                .SumAsync(s => s.QuantityOnHand);
            var material = await _dbContext.Materials.FindAsync(materialId);
            if (material != null)
            {
                material.QuantityAvailable = (int)total;
                material.LastUpdated = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            await tx.CommitAsync();
            return true;
        }
        
        /// <summary>
        /// Phương thức tổng quát để thực hiện transaction inventory với logging đầy đủ
        /// </summary>
        /// <param name="materialId">ID của material</param>
        /// <param name="warehouseId">ID của warehouse</param>
        /// <param name="transactionType">Loại transaction</param>
        /// <param name="quantityChange">Thay đổi số lượng (+ hoặc -)</param>
        /// <param name="unit">Đơn vị (mét, kg, etc.)</param>
        /// <param name="note">Ghi chú</param>
        /// <param name="referenceType">Loại tham chiếu</param>
        /// <param name="referenceId">ID tham chiếu</param>
        /// <param name="userId">ID user thực hiện</param>
        /// <returns>True nếu thành công</returns>
        public async Task<bool> CreateTransactionAsync(
            int materialId, 
            int warehouseId, 
            MaterialTransactionType transactionType,
            decimal quantityChange, 
            string? unit = "mét", 
            string? note = null, 
            string? referenceType = null, 
            string? referenceId = null, 
            int? userId = null)
        {
            if (quantityChange == 0) 
                throw new ArgumentException("Quantity change cannot be zero");

            using var tx = await _dbContext.Database.BeginTransactionAsync();

            var stock = await _dbContext.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialId == materialId && s.WarehouseId == warehouseId);
            
            if (stock == null)
            {
                throw new InvalidOperationException($"No stock found for MaterialId {materialId} in WarehouseId {warehouseId}");
            }

            var before = stock.QuantityOnHand;
            var after = before + quantityChange; // quantityChange có thể âm hoặc dương

            // Cảnh báo nếu inventory âm (cho phép overdraft)
            if (after < 0)
            {
                Console.WriteLine($"WARNING: Stock going negative for MaterialId {materialId}. Before: {before}, After: {after}");
            }

            // Tạo transaction log với thông tin chi tiết
            var transaction = new MaterialStockTransaction
            {
                MaterialId = materialId,
                WarehouseId = warehouseId,
                TransactionType = transactionType,
                QuantityChange = quantityChange,
                BeforeQty = before,
                AfterQty = after,
                Unit = unit,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Note = note ?? $"{transactionType.GetDescription()} - {Math.Abs(quantityChange)} {unit}",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.MaterialStockTransactions.Add(transaction);

            // Cập nhật stock
            stock.QuantityOnHand = after;
            stock.LastUpdated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Cập nhật Material.QuantityAvailable
            await UpdateMaterialQuantityAvailableAsync(materialId);

            await tx.CommitAsync();
            
            Console.WriteLine($"✅ {transactionType.GetDescription()}: MaterialId {materialId}, Change: {quantityChange}, Before: {before}, After: {after}");
            return true;
        }
        
        /// <summary>
        /// Cập nhật Material.QuantityAvailable từ tổng của tất cả warehouses
        /// </summary>
        private async Task UpdateMaterialQuantityAvailableAsync(int materialId)
        {
            var total = await _dbContext.MaterialStocks
                .Where(s => s.MaterialId == materialId)
                .SumAsync(s => s.QuantityOnHand);
                
            var material = await _dbContext.Materials.FindAsync(materialId);
            if (material != null)
            {
                material.QuantityAvailable = (int)Math.Max(0, total); // Không để âm ở Material level
                material.LastUpdated = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }
        
        /// <summary>
        /// Xử lý trả hàng từ customer (tăng inventory)
        /// </summary>
        public async Task<bool> ProcessCustomerReturnAsync(
            int materialId, 
            int warehouseId, 
            decimal returnQuantity, 
            string orderId, 
            string? returnReason = null, 
            int? userId = null)
        {
            var note = $"Trả hàng từ đơn #{orderId}" + 
                      (string.IsNullOrEmpty(returnReason) ? "" : $" - Lý do: {returnReason}");
                      
            return await CreateTransactionAsync(
                materialId: materialId,
                warehouseId: warehouseId,
                transactionType: MaterialTransactionType.CustomerReturn,
                quantityChange: returnQuantity, // Số dương - tăng inventory
                unit: "mét",
                note: note,
                referenceType: "CustomerReturn", 
                referenceId: orderId,
                userId: userId
            );
        }
        
        /// <summary>
        /// Xử lý điều chỉnh thủ công (có thể tăng hoặc giảm)
        /// </summary>
        public async Task<bool> ProcessManualAdjustmentAsync(
            int materialId, 
            int warehouseId, 
            decimal adjustmentQuantity, 
            string reason, 
            int userId)
        {
            var note = $"Điều chỉnh thủ công - {reason}";
                      
            return await CreateTransactionAsync(
                materialId: materialId,
                warehouseId: warehouseId,
                transactionType: MaterialTransactionType.ManualAdjustment,
                quantityChange: adjustmentQuantity, // Có thể + hoặc -
                unit: "mét",
                note: note,
                referenceType: "ManualAdjustment", 
                referenceId: $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                userId: userId
            );
        }
    }
}


