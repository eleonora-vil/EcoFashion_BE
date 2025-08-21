namespace EcoFashionBackEnd.Dtos.Wallet
{
    public class nested_DTO
    {
        // DTOs
        public class OrderDetailDto
        {
            public int OrderDetailId { get; set; }
            public int? DesignId { get; set; }
            public int? MaterialId { get; set; }
            public string Type { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal => Quantity * UnitPrice;
        }

        public class OrderDto
        {
            public int OrderId { get; set; }
            public Guid SellerId { get; set; }
            public string SellerType { get; set; }
            public decimal TotalPrice { get; set; }
            public List<OrderDetailDto> Details { get; set; } = new();
        }

        public class OrderGroupDto
        {
            public Guid OrderGroupId { get; set; }
            public int UserId { get; set; }
            public decimal TotalOrders { get; set; }
            public List<OrderDto> Orders { get; set; } = new();
        }

    }
}
