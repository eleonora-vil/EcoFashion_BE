using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Dtos.DesignShow
{
    public class DesignSummaryDto
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public float RecycledPercentage { get; set; }
        public int? ItemTypeId { get; set; }
        public decimal? SalePrice { get; set; }
        public List<string> DesignImages { get; set; }
        public List<MaterialDto> Materials { get; set; }
    }
}
