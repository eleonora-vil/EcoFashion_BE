namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateOrderRequest
    {
        public required string ShippingAddress { get; set; }
        public required decimal TotalPrice { get; set; }
    }
}
