using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("ProductInventoryTransactions")]
    public class ProductInventoryTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public int InventoryId { get; set; }
        [ForeignKey(nameof(InventoryId))]
        public virtual ProductInventory ProductInventory { get; set; }

        public int PerformedByUserId { get; set; }
        [ForeignKey(nameof(PerformedByUserId))]
        public virtual User User { get; set; } 
        public int QuantityChanged { get; set; }  // Số lượng nhập (+) hoặc xuất (-)

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public string TransactionType { get; set; }  // Ví dụ: "Sale", "Restock", "Adjustment", "Return"

        public string Notes { get; set; }  // Ghi chú thêm nếu cần
    }
}
