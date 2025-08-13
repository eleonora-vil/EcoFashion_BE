namespace EcoFashionBackEnd.Dtos
{
    public class MaterialModel
    {
        public int MaterialId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal RecycledPercentage { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? DocumentationUrl { get; set; }
        public decimal? CarbonFootprint { get; set; }
        public string? CarbonFootprintUnit { get; set; }
        public decimal? WaterUsage { get; set; }
        public string? WaterUsageUnit { get; set; }
        public decimal? WasteDiverted { get; set; }
        public string? WasteDivertedUnit { get; set; }
        public string? ProductionCountry { get; set; }
        public string? ManufacturingProcess { get; set; }
        public string? CertificationDetails { get; set; }
        public DateTime? CertificationExpiryDate { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? AdminNote { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public MaterialTypeModel? MaterialType { get; set; }
        public SupplierModel? Supplier { get; set; }
    }
}
