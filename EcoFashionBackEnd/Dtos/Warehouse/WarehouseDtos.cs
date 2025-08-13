using System;

namespace EcoFashionBackEnd.Dtos.Warehouse
{
    public class WarehouseDto
    {
        public int WarehouseId { get; set; }
        public string? Name { get; set; }
        public string Type { get; set; } = "Material";
        public Guid? SupplierId { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }

    public class MaterialStockDto
    {
        public int StockId { get; set; }
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal MinThreshold { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? Note { get; set; }

        // Convenience display
        public string? MaterialName { get; set; }
        public string? WarehouseName { get; set; }
        public string? Unit { get; set; }
        public string? ImageUrl { get; set; }

        // From Material entity for quick display
        public int QuantityAvailable { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? WarehouseType { get; set; }
    }

    public class MaterialStockTransactionDto
    {
        public int TransactionId { get; set; }
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }
        public string TransactionType { get; set; } = "SupplierReceipt";
        public decimal QuantityChange { get; set; }
        public decimal BeforeQty { get; set; }
        public decimal AfterQty { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }
        public string? Note { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? MaterialName { get; set; }
        public string? WarehouseName { get; set; }
        public string? SupplierName { get; set; }
        public string? ImageUrl { get; set; }
        public string? WarehouseType { get; set; }
    }

    // Requests
    public class ReceiveMaterialRequest
    {
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Note { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }
    }

    public class AdjustMaterialRequest
    {
        public int MaterialId { get; set; }
        public int WarehouseId { get; set; }
        public decimal TargetQuantity { get; set; }
        public string? Unit { get; set; }
        public string? Note { get; set; }
    }

    public class TransferMaterialRequest
    {
        public int MaterialId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Note { get; set; }
        public string? ReferenceId { get; set; }
    }
}


