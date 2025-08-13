using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("ProductInventories")]
    public class ProductInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryId { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        public int WarehouseId { get; set; } // Nếu có nhiều kho vật lý
        [ForeignKey(nameof(WarehouseId))]
        public virtual Warehouse Warehouse { get; set; }
        public int QuantityAvailable { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public virtual ICollection<ProductInventoryTransaction> Transactions { get; set; } = new List<ProductInventoryTransaction>();
    }
}
