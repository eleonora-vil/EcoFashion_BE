using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Dtos
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public required string ShippingAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public required string Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? FulfillmentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string? SellerName { get; set; }
        public string? SellerType { get; set; }
        public string? SellerAvatarUrl { get; set; }
    }
}
