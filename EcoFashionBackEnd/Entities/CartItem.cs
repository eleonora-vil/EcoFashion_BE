using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
	[Table("CartItems")]
	public class CartItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int CartItemId { get; set; }

		[ForeignKey("Cart")]
		public int CartId { get; set; }
		public virtual Cart Cart { get; set; } = null!;

		// Hiện tại chỉ hỗ trợ Supplier bán Material
		// Đặt tên tường minh để tránh nhầm lẫn
		// Có thể mở rộng sau này cho Designer/Product?
		public int MaterialId { get; set; }

		public int Quantity { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal UnitPriceSnapshot { get; set; }

		public DateTime AddedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}


