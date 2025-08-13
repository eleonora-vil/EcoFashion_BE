using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("OrderGroups")]
    public class OrderGroup
    {
        [Key]
        public Guid OrderGroupId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [EnumDataType(typeof(OrderGroupStatus))]
        public OrderGroupStatus Status { get; set; } = OrderGroupStatus.InProgress;

        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum OrderGroupStatus
    {
        InProgress,
        Completed,
        Abandoned
    }
}


