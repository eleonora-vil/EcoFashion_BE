using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
	[Table("Carts")]
	public class Cart
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int CartId { get; set; }

		// Có thể null cho khách ẩn danh; dùng SessionKey khi chưa đăng nhập
		public int? UserId { get; set; }
		[ForeignKey("UserId")]
		public virtual User? User { get; set; }

		// Khoá nhận diện thiết bị/phiên cho khách (tuỳ chọn)
		[MaxLength(100)]
		public string? SessionKey { get; set; }

		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? ExpiresAt { get; set; }

		public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
	}
}


