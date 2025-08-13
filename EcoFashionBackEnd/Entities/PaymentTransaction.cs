using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("PaymentTransactions")]
    public class PaymentTransaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int OrderId { get; set; }             

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public string? VnPayTransactionId { get; set; }  
        public string? VnPayResponseCode { get; set; }   
        public long Amount { get; set; }               
        public string? BankCode { get; set; }         
        public string? CardType { get; set; }          

        public string? Status { get; set; }            
        public string? Message { get; set; }            

        public string? OrderType { get; set; }       
        public string? PaymentType { get; set; }       
 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        // Extended fields for robust VNPay flow
        [MaxLength(64)]
        public string? TxnRef { get; set; }
        public string? PayUrl { get; set; }
        public string? Provider { get; set; } // e.g., VNPAY
        public string? MerchantCode { get; set; }
        public string? ReturnPayload { get; set; }
        public string? IpnPayload { get; set; }
    }

}
