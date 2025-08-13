using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Materials")]
    public class Material
    {
        public Material()
        {
            MaterialImages = new HashSet<MaterialImage>();
            MaterialSustainabilityMetrics = new HashSet<MaterialSustainability>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaterialId { get; set; }

        [ForeignKey("Supplier")]
        public Guid SupplierId { get; set; }
        public virtual Supplier? Supplier { get; set; }

        [ForeignKey("MaterialType")]
        public int TypeId { get; set; }
        public virtual MaterialType? MaterialType { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal RecycledPercentage { get; set; }
        public int QuantityAvailable { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }
        public string? DocumentationUrl { get; set; }
        
        // Sustainability fields  - 3 fields chỉ số để tính toán
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CarbonFootprint { get; set; }
        public string? CarbonFootprintUnit { get; set; } // ví dụ: kg CO2
        [Column(TypeName = "decimal(18,2)")]
        public decimal? WaterUsage { get; set; }
        public string? WaterUsageUnit { get; set; } // ví dụ: lít
        [Column(TypeName = "decimal(18,2)")]
        public decimal? WasteDiverted { get; set; }
        public string? WasteDivertedUnit { get; set; } // ví dụ: kg

        public string? ProductionCountry { get; set; }
        public string? ProductionRegion { get; set; } // Thêm region cụ thể
        public string? ManufacturingProcess { get; set; }

        public string? CertificationDetails { get; set; } // GOTS, OEKO-TEX, USDA Organic, EU Ecolabel, etc.
        public DateTime? CertificationExpiryDate { get; set; }
        
        // Transport information for carbon footprint calculation
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TransportDistance { get; set; } // Distance in km
        public string? TransportMethod { get; set; } // Sea, Air, Land, Rail
        
        // Organic Certification Explanation:
        // - GOTS (Global Organic Textile Standard): https://global-standard.org/
        // - OEKO-TEX Standard 100: https://www.oeko-tex.com/
        // - USDA Organic: https://www.usda.gov/topics/organic
        // - EU Ecolabel: https://ec.europa.eu/environment/ecolabel/
        // - Soil Association: https://www.soilassociation.org/
        // - RWS (Responsible Wool Standard): https://textileexchange.org/standards/rws/
        // - GRS (Global Recycled Standard): https://textileexchange.org/standards/grs/

        public string? ApprovalStatus { get; set; } // Pending, Approved, Rejected
        public string? AdminNote { get; set; }

        public bool IsAvailable { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<MaterialImage> MaterialImages { get; set; }
        public virtual ICollection<MaterialSustainability> MaterialSustainabilityMetrics { get; set; }
    }
}