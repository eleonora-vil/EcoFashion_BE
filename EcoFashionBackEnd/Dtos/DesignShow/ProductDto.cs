namespace EcoFashionBackEnd.Dtos.DesignShow
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public string ColorCode { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
