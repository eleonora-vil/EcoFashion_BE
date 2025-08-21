namespace EcoFashionBackEnd.Dtos.Cart
{
	public class CartItemDto
	{
		public int CartItemId { get; set; }
		public int MaterialId { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPriceSnapshot { get; set; }

		// Enriched from Material/Supplier for display/grouping
		public decimal CurrentPrice { get; set; }
		public string? MaterialName { get; set; }
		public string? ImageUrl { get; set; }
		public string? UnitLabel { get; set; }
		public System.Guid SupplierId { get; set; }
		public string? SupplierName { get; set; }
	}

	public class CartDto
	{
		public int CartId { get; set; }
		public bool IsActive { get; set; }
		public System.DateTime UpdatedAt { get; set; }
		public List<CartItemDto> Items { get; set; } = new();
		public decimal Subtotal => Items.Sum(i => (i.CurrentPrice > 0 ? i.CurrentPrice : i.UnitPriceSnapshot) * i.Quantity);
	}
}


