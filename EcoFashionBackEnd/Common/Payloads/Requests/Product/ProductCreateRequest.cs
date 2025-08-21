using EcoFashionBackEnd.Common.Payloads.Requests.Variant;

namespace EcoFashionBackEnd.Common.Payloads.Requests.Product
{
    public class ProductCreateRequest
    {
        public int DesignId { get; set; }

        // Nhận từ form-data dạng text, parse JSON trong service
        public string Variants { get; set; }

        // Ảnh upload
        public List<IFormFile> Images { get; set; }
    }
}
