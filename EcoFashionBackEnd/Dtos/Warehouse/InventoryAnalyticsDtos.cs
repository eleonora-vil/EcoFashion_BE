using System;

namespace EcoFashionBackEnd.Dtos.Warehouse
{
    public class InventorySummaryDto
    {
        public int TotalDistinctMaterials { get; set; }
        public decimal TotalOnHand { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int LowStockCount { get; set; }
        public int StockoutCount { get; set; }
    }

    public class MovementPointDto
    {
        public DateTime Date { get; set; }
        public decimal InQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal NetQty { get; set; }
    }

    public class LowStockItemDto
    {
        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal MinThreshold { get; set; }
        public decimal Difference { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal EstimatedValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SupplierReceiptPointDto
    {
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public string? SupplierName { get; set; }
    }
}


