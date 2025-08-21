using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("DesignerMaterialInventories")]
    public class DesignerMaterialInventory
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryId { get; set; }
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Cost { get; set; }
        public DateTime LastBuyDate { get; set; }
        public string? Status { get; set; } // e.g., "In Stock", "Out of Stock"

        public virtual ICollection<MaterialInventoryTransaction> MaterialInventoryTransactions { get; set; } = new List<MaterialInventoryTransaction>();
    }
}
