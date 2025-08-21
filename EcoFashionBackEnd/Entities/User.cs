using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Phone {  get; set; }

        [StringLength(30, MinimumLength = 8)]
        public string? Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        // OTP Fields for email verification
        [StringLength(6)]
        public string? OTPCode { get; set; }
        
        public DateTime? OTPExpiresAt { get; set; }

        // [ForeignKey("UserRole")]
        public int RoleId { get; set; }
        public virtual UserRole? UserRole { get; set; }

        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserStatus Status { get; set; } = UserStatus.Pending;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set;}
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public virtual Wallet Wallet { get; set; }
    }


    public enum UserStatus
    {
        Pending,
        Active,
        Rejected,
        Inactive
    }
}
