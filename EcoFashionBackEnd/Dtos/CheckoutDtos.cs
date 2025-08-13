namespace EcoFashionBackEnd.Dtos
{
    public class CreateSessionRequest
    {
        public List<CartItemDto> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public int HoldMinutes { get; set; } = 30; // thời gian giữ hàng
    }

    public class CartItemDto
    {
        public string ItemType { get; set; } = string.Empty; // material/design
        public int? MaterialId { get; set; }
        public int? DesignId { get; set; }
        public Guid? SellerId { get; set; }
        public string SellerType { get; set; } = string.Empty; // Supplier/Designer
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateSessionResponse
    {
        public Guid OrderGroupId { get; set; }
        public List<CheckoutOrderDto> Orders { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }

    public class CheckoutOrderDto
    {
        public int OrderId { get; set; }
        public string SellerType { get; set; } = string.Empty;
        public Guid? SellerId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}


