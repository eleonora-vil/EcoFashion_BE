namespace EcoFashionBackEnd.Common.Payloads.Requests;

public class UpdateOrderDetailRequest
{
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Status { get; set; }
}