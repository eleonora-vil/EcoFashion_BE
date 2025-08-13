namespace EcoFashionBackEnd.Common.Payloads.Responses.Product
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public int DesignId { get; set; }
        public string SKU { get; set; } = "";
        public decimal Price { get; set; }
        public int SizeId { get; set; }
        public string ColorCode { get; set; } = "";
        public string CareInstruction { get; set; } = "";
    }
}
