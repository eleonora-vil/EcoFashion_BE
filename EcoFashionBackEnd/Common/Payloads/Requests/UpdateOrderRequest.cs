using EcoFashionBackEnd.Entities;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class UpdateOrderRequest
    {
        public string? ShippingAddress { get; set; }
        public decimal? TotalPrice { get; set; }
        [EnumDataType(typeof(OrderStatus))]
        public OrderStatus? Status { get; set; } = OrderStatus.pending;
    }
}
