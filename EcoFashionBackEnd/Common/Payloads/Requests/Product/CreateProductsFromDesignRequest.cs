namespace EcoFashionBackEnd.Common.Payloads.Requests.Product
{
    public class CreateProductsFromDesignRequest
    {
        public int DesignId { get; set; }

        // Nếu có ảnh để đẩy lên design (optional)
        public List<IFormFile> Images { get; set; } = new();
    }
}
