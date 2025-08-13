using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class DesignDetailResponse
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? RecycledPercentage { get; set; }
        public string CareInstructions { get; set; }
        public decimal? SalePrice { get; set; }
        public double? ProductScore { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? DesignTypeName { get; set; }
        public List<string> ImageUrls { get; set; } = new();

        public DesignFeatureDto? Feature { get; set; }

        public List<VariantDto> Variants { get; set; } = new();
        public List<MaterialDto> Materials { get; set; } = new();

        public double? AvgRating { get; set; }
        public int? ReviewCount { get; set; }

        public DesignerPublicDto Designer { get; set; }
    }

}
