using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        [Required]
        public required int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        // Grouping for multi-seller checkout sessions
        public Guid? OrderGroupId { get; set; }
        [ForeignKey("OrderGroupId")]
        public virtual OrderGroup? OrderGroup { get; set; }
        [Required]
        public required string ShippingAddress { get; set; }
        // Monetary breakdown
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        [Required]
        public required decimal TotalPrice { get; set; }
        [Required]
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus Status { get; set; } = OrderStatus.pending;
        // Payment status separate from fulfillment
        [EnumDataType(typeof(PaymentStatus))]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        // Fulfillment status for shipping lifecycle
        [EnumDataType(typeof(FulfillmentStatus))]
        public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.None;
        // Commission for platform
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionAmount { get; set; }
        public decimal? NetAmount { get; set; }
        // Per-seller routing
        public string? SellerType { get; set; } // "Supplier" or "Designer"
        public Guid? SellerId { get; set; }
        // Expiry to release reserved stock
        public DateTime? ExpiresAt { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
    public enum OrderStatus
    {
        pending,
        processing,
        shipped,
        delivered,
        returned,
    }
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Expired
    }
    public enum FulfillmentStatus
    {
        None,
        Processing,
        Shipped,
        Delivered,
        Canceled
    }
}
