using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("Wallets")]
    public class Wallet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WalletId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public double Balance { get; set; } = 0;

       
        [EnumDataType(typeof(WalletStatus))]
        public WalletStatus? Status { get; set; } = WalletStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; }

        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
    }

    public enum WalletStatus
    {
        Active,
        Locked,
        Inactive
    }
}
