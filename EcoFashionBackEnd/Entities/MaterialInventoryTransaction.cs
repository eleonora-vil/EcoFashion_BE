using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("MaterialInventoryTransactions")]
    public class MaterialInventoryTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public int InventoryId { get; set; }
        [ForeignKey(nameof(InventoryId))]
        public virtual DesignerMaterialInventory MaterialInventory { get; set; }

        public int? PerformedByUserId { get; set; }
        [ForeignKey(nameof(PerformedByUserId))]
        public virtual User User { get; set; }
        public decimal QuantityChanged { get; set; }
        public decimal? BeforeQty { get; set; }
        public decimal? AfterQty { get; set; }
        public string TransactionType { get; set; }
        public string Notes { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
