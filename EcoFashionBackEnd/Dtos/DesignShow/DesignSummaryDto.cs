using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Dtos.DesignShow
{
    public class DesignSummaryDto
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public float RecycledPercentage { get; set; }
        public string? ItemTypeName { get; set; }
        public decimal? SalePrice { get; set; }
        public List<string> DesignImageUrls { get; set; }
        public List<string> DrafSketches { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public List<DesignVariantsDto> DesignsVariants { get; set; }
    }
}
