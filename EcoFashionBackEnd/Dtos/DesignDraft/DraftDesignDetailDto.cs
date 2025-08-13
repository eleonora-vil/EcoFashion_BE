namespace EcoFashionBackEnd.Dtos.DesignDraft
{
    public class DraftDesignDetailDto
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float RecycledPercentage { get; set; }
        public int DesignTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public float? LaborHours { get; set; }
        public decimal? LaborCostPerHour { get; set; }
        // Stats
        public float TotalCarbon { get; set; }
        public float TotalWater { get; set; }
        public float TotalWaste { get; set; }

        // Parts
        public List<DraftPartDto> DraftParts { get; set; } = new();

        // Materials
        public List<DesignMaterialDto> Materials { get; set; } = new();

        // Sketches
        public List<string> SketchImageUrls { get; set; } = new();
    }
}
