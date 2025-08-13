using System;

namespace EcoFashionBackEnd.Entities
{
    public class MaterialStock
    {
        public int StockId { get; set; }
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }

        public decimal QuantityOnHand { get; set; }
        public decimal MinThreshold { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        // Navigation
        public Material? Material { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}


