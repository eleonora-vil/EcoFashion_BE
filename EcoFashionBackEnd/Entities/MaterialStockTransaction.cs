using System;

namespace EcoFashionBackEnd.Entities
{
    public class MaterialStockTransaction
    {
        public int TransactionId { get; set; }
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }

        // SupplierReceipt, ManualAdjustment, ProductionUse, TransferIn, TransferOut, SalesIssue, PurchaseReturn, SalesReturn
        public string TransactionType { get; set; } = "SupplierReceipt";

        public decimal QuantityChange { get; set; } // +/-
        public decimal BeforeQty { get; set; }
        public decimal AfterQty { get; set; }

        public string? Unit { get; set; } // e.g. "mét"

        // Manual, PurchaseOrder, SalesOrder, ProductionOrder, Transfer
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }

        public string? Note { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Material? Material { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}


