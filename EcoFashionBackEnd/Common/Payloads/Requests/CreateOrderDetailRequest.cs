using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateOrderDetailRequest
    {
        public int OrderId { get; set; }
        public int? DesignId { get; set; }
        public int? MaterialId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public OrderDetailType Type { get; set; }
    }
}
