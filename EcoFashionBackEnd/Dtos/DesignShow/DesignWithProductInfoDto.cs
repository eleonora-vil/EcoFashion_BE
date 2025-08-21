using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Dtos.DesignShow
{
    public class DesignWithProductInfoDto
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public float RecycledPercentage { get; set; }
        public string? ItemTypeName { get; set; }
        public decimal? SalePrice { get; set; }
        public List<string> DesignImageUrls { get; set; } = new List<string>();
        public List<MaterialDto> Materials { get; set; } = new List<MaterialDto>();
        public int ProductCount { get; set; }// bỏ chưa cần 
        public DesignerPublicDto Designer { get; set; }
        public List<ProductDto> Products { get; set; }
    }
}
