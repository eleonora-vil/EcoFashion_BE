using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Dtos.DesignShow
{
    public class ProductCreateWVariantRequest
    {
        public int DesignId { get; set; }

        // Ảnh upload
        public List<IFormFile> Images { get; set; } = new();
    }
}
