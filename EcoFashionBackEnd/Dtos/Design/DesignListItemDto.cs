namespace EcoFashionBackEnd.Dtos.Design
{
    public class DesignListItemDto
    {
        public int DesignId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DesignTypeName { get; set; }
        public List<string>? ImageUrls { get; set; }

        public float RecycledPercentage { get; set; }
        public DateTime CreatedAt { get; set; }

        public float? AvgRating { get; set; }
        public int? ReviewCount { get; set; }
        public DesignFeatureDto? Feature { get; set; }

        public List<VariantDto>? Variants { get; set; }
        public List<MaterialDto>? Materials { get; set; }
    }
}
