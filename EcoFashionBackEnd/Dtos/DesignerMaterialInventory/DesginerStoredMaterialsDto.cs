using EcoFashionBackEnd.Dtos.Material;

namespace EcoFashionBackEnd.Dtos.DesignerMaterialInventory
{
    public class DesginerStoredMaterialsDto
    {
        public int MaterialId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string MaterialTypeName { get; set; } = "";
        public decimal RecycledPercentage { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime LastUpdated { get; set; }
        public decimal? CarbonFootprint { get; set; }
        public string? CarbonFootprintUnit { get; set; }
        public decimal? WaterUsage { get; set; }
        public string? WaterUsageUnit { get; set; }
        public decimal? WasteDiverted { get; set; }
        public string? WasteDivertedUnit { get; set; }
        public string? ProductionCountry { get; set; }
        public string? ProductionRegion { get; set; }
        public string? ManufacturingProcess { get; set; }
        public string? CertificationDetails { get; set; }
        public DateTime? CertificationExpiryDate { get; set; }
        public decimal? TransportDistance { get; set; }
        public string? TransportMethod { get; set; }
        //public string? ApprovalStatus { get; set; }
        //public string? AdminNote { get; set; }
        //public bool IsAvailable { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? SupplierName { get; set; } 
        public Guid SupplierId { get; set; }

        // Sustainability fields
        public decimal? SustainabilityScore { get; set; }
        //public string? SustainabilityLevel { get; set; }
        //public string? SustainabilityColor { get; set; }

        // Supplier object
        //public SupplierPublicModel? Supplier { get; set; }

        // Sustainability criteria - sử dụng DTO đơn giản
        //public List<MaterialSustainabilityCriterionDto>? SustainabilityCriteria { get; set; } = new();

        // Benchmarks - sử dụng model có sẵn
        //public List<MaterialTypeBenchmarkModel>? Benchmarks { get; set; } = new();
    }
}
