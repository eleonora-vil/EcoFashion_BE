using EcoFashionBackEnd.Dtos.DesignShow;
using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Dtos.Design
{
    public class DesignDetailDto
    {
        public int DesignId { get; set; }
        public Guid DesignerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float RecycledPercentage { get; set; }
        public decimal? SalePrice { get; set; }
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public float? CarbonFootprint { get; set; }
        public float? WaterUsage { get; set; }
        public float? WasteDiverted { get; set; }
        public string CareInstruction { get; set; }
        public DesignFeatureDto Feature { get; set; }
        public List<ProductDto> Products { get; set; }
        public List<string> DesignImages { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public DesignerPublicDto Designer { get; set; }
    }
}
