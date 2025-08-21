using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Warehouses")]
    public class Warehouse
    {
        public int WarehouseId { get; set; }

        // Tên kho
        public string? Name { get; set; }

        // Phân loại kho: "Material" | "Product" | ... (dùng cho tách không gian)
        public required string WarehouseType { get; set; }

        // Nếu là kho của nhà cung cấp, SupplierId sẽ có giá trị.
        // Nếu null → kho hệ thống/global
        public Guid? SupplierId { get; set; }

        // kho của nhà thiết kế sẽ có giá trị
        public Guid? DesignerId { get; set; }

        // Navigation tới Designer (nếu là kho của designer)
        public Designer? Designer { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
        // Navigation tới ProductInventories (nếu dùng cho kho sản phẩm)
        public ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
        public virtual ICollection<DesignerMaterialInventory> MaterialInventories { get; set; } = new List<DesignerMaterialInventory>();
    }
}
