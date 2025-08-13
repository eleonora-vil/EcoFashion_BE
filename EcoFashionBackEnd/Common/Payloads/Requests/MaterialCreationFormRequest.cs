using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class MaterialCreationFormRequest
    {
        [Required]
        public Guid SupplierId { get; set; }

        [Required]
        public int TypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal RecycledPercentage { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantityAvailable { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PricePerUnit { get; set; }

        [Url]
        public string? DocumentationUrl { get; set; }

        // Sustainability fields
        [Range(0, double.MaxValue)]
        public decimal? CarbonFootprint { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? WaterUsage { get; set; }

        [Range(0, 100)]
        public decimal? WasteDiverted { get; set; }

        public bool HasOrganicCertification { get; set; } = false;

        [StringLength(100)]
        public string? OrganicCertificationType { get; set; }

        [StringLength(100)]
        public string? ProductionCountry { get; set; }

        [StringLength(100)]
        public string? ProductionRegion { get; set; }

        [StringLength(200)]
        public string? ManufacturingProcess { get; set; }

        public bool IsCertified { get; set; } = false;

        [StringLength(500)]
        public string? CertificationDetails { get; set; }

        public DateTime? CertificationExpiryDate { get; set; }

        [StringLength(200)]
        public string? QualityStandards { get; set; }

        // Transport information (optional - can be auto-calculated)
        [Range(0, double.MaxValue)]
        public decimal? TransportDistance { get; set; }

        [StringLength(50)]
        public string? TransportMethod { get; set; } // Sea, Air, Land, Rail

        // Sustainability criteria values
        public List<MaterialSustainabilityCriterionRequest> SustainabilityCriteria { get; set; } = new();

        public bool IsAvailable { get; set; } = true;
    }

    // Request DTO for sustainability criteria values
    public class MaterialSustainabilityCriterionRequest
    {
        public int CriterionId { get; set; }
        public decimal Value { get; set; }
    }
} 