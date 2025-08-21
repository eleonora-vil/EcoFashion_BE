using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace EcoFashionBackEnd.Entities
{
    [Table("WalletTransactions")]
    public class WalletTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [ForeignKey("WalletId")]
        public virtual Wallet Wallet { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public double BalanceBefore { get; set; }

        [Required]
        public double BalanceAfter { get; set; }

        [Required]
        [EnumDataType(typeof(TransactionType))]
        public TransactionType Type { get; set; }

        public string? Description { get; set; }
        public TransactionStatus? Status { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: Liên kết với PaymentTransaction nếu có
        public Guid? PaymentTransactionId { get; set; }

        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction? PaymentTransaction { get; set; }

    }

    public enum TransactionType
    {
        Deposit,      // Nạp tiền
        Withdrawal,   // Rút tiền
        Payment,      // Thanh toán Order
        Refund,       // Hoàn tiền
        Transfer      // Chuyển khoản nội bộ
    }
    public enum TransactionStatus
    {
        Pending,
        Success,
        Fail
    }
}
