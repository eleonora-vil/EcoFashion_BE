using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Dtos.Warehouse;

namespace EcoFashionBackEnd.Services
{
    public class InventoryAnalyticsService
    {
        private readonly AppDbContext _db;

        public InventoryAnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<InventorySummaryDto> GetSummaryAsync(DateTime? from, DateTime? to, Guid? supplierId = null, int? warehouseId = null, int? materialId = null)
        {
            var stocks = _db.MaterialStocks
                .Include(s => s.Material)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (supplierId.HasValue) stocks = stocks.Where(s => s.Warehouse!.SupplierId == supplierId);
            if (warehouseId.HasValue) stocks = stocks.Where(s => s.WarehouseId == warehouseId);
            if (materialId.HasValue) stocks = stocks.Where(s => s.MaterialId == materialId);

            var list = await stocks.ToListAsync();

            var totalDistinctMaterials = list.Select(s => s.MaterialId).Distinct().Count();
            var totalOnHand = list.Sum(s => s.QuantityOnHand);
            var totalValue = list.Sum(s => (s.Material?.PricePerUnit ?? 0) * s.QuantityOnHand);
            var lowStockCount = list.Count(s => s.QuantityOnHand < s.MinThreshold);
            var stockoutCount = list.Count(s => s.QuantityOnHand <= 0);

            return new InventorySummaryDto
            {
                TotalDistinctMaterials = totalDistinctMaterials,
                TotalOnHand = totalOnHand,
                TotalInventoryValue = totalValue,
                LowStockCount = lowStockCount,
                StockoutCount = stockoutCount
            };
        }

        public async Task<List<MovementPointDto>> GetMovementsAsync(DateTime from, DateTime to, Guid? supplierId = null, int? warehouseId = null, int? materialId = null)
        {
            var tx = _db.MaterialStockTransactions
                .Include(t => t.Warehouse)
                .AsQueryable();

            tx = tx.Where(t => t.CreatedAt >= from && t.CreatedAt <= to);
            if (supplierId.HasValue) tx = tx.Where(t => t.Warehouse!.SupplierId == supplierId);
            if (warehouseId.HasValue) tx = tx.Where(t => t.WarehouseId == warehouseId);
            if (materialId.HasValue) tx = tx.Where(t => t.MaterialId == materialId);

            var list = await tx
                .Select(t => new
                {
                    // Quy đổi múi giờ VN để nhóm theo ngày địa phương
                    Date = t.CreatedAt.AddHours(7).Date,
                    InQty = t.TransactionType == MaterialTransactionType.SupplierReceipt ? t.QuantityChange : 0,
                    OutQty = t.TransactionType != MaterialTransactionType.SupplierReceipt ? t.QuantityChange : 0,
                })
                .ToListAsync();

            var grouped = list
                .GroupBy(x => x.Date)
                .OrderBy(g => g.Key)
                .Select(g => new MovementPointDto
                {
                    Date = g.Key,
                    InQty = g.Sum(i => i.InQty),
                    OutQty = g.Sum(i => i.OutQty),
                    NetQty = g.Sum(i => i.InQty) - g.Sum(i => i.OutQty)
                })
                .ToList();

            return grouped;
        }

        public async Task<List<LowStockItemDto>> GetLowStockAsync(Guid? supplierId = null, int? warehouseId = null, int? materialTypeId = null, int limit = 50)
        {
            var q = _db.MaterialStocks
                .Include(s => s.Material)
                .Include(s => s.Warehouse)
                .AsQueryable();

            if (supplierId.HasValue) q = q.Where(s => s.Warehouse!.SupplierId == supplierId);
            if (warehouseId.HasValue) q = q.Where(s => s.WarehouseId == warehouseId);
            if (materialTypeId.HasValue) q = q.Where(s => s.Material!.TypeId == materialTypeId);

            var list = await q
                .Where(s => s.QuantityOnHand < s.MinThreshold)
                .OrderBy(s => s.QuantityOnHand - s.MinThreshold)
                .Take(limit)
                .ToListAsync();

            return list.Select(s => new LowStockItemDto
            {
                MaterialId = s.MaterialId,
                MaterialName = s.Material?.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name,
                QuantityOnHand = s.QuantityOnHand,
                MinThreshold = s.MinThreshold,
                Difference = s.QuantityOnHand - s.MinThreshold,
                PricePerUnit = s.Material?.PricePerUnit ?? 0,
                EstimatedValue = (s.Material?.PricePerUnit ?? 0) * s.QuantityOnHand,
                LastUpdated = s.LastUpdated
            }).ToList();
        }

        public async Task<List<SupplierReceiptPointDto>> GetReceiptsBySupplierAsync(DateTime fromDate, DateTime toDate, int? materialId = null)
        {
            // Build query via joins (left join supplier)
            var query = from t in _db.MaterialStockTransactions
                        join w in _db.Warehouses on t.WarehouseId equals w.WarehouseId
                        join s in _db.Suppliers on w.SupplierId equals s.SupplierId into sgrp
                        from s in sgrp.DefaultIfEmpty()
                        where t.TransactionType == MaterialTransactionType.SupplierReceipt && t.CreatedAt >= fromDate && t.CreatedAt <= toDate
                        select new { t.CreatedAt, t.QuantityChange, SupplierName = s != null ? s.SupplierName : null, t.MaterialId };

            if (materialId.HasValue)
            {
                query = query.Where(x => x.MaterialId == materialId.Value);
            }

            var rows = await query
                .GroupBy(x => new { Date = x.CreatedAt.AddHours(7).Date, x.SupplierName })
                .Select(g => new SupplierReceiptPointDto
                {
                    Date = g.Key.Date,
                    Quantity = g.Sum(x => x.QuantityChange),
                    SupplierName = g.Key.SupplierName
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return rows;
        }
    }
}


