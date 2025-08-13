    using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("Designs")]
    public class Design
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DesignId { get; set; }

        [ForeignKey("DesignerProfile")]
        public Guid DesignerId { get; set; }
        public virtual Designer DesignerProfile { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public float RecycledPercentage { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }

        public int ProductScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ItemTypes")]
        public int? ItemTypeId { get; set; }

        public float? CarbonFootprint { get; set; }
        public float? WaterUsage { get; set; }
        public float? WasteDiverted { get; set; }

        public float? LaborHours { get; set; }
        public decimal? LaborCostPerHour { get; set; }
        public string CareInstruction { get; set; }

        public virtual ItemType ItemTypes { get; set; }
        public virtual DesignFeature DesignFeatures { get; set; }
        public virtual ICollection<DesignsVariant> DesignsVariants { get; set; } = new List<DesignsVariant>();
        public virtual ICollection<DesignsMaterial> DesignsMaterials { get; set; } = new List<DesignsMaterial>();
        public virtual ICollection<DraftSketch> DraftSketches { get; set; } = new List<DraftSketch>();
        public virtual ICollection<DraftPart> DraftParts { get; set; } = new List<DraftPart>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<DesignImage> DesignImages { get; set; } = new List<DesignImage>();

    }
   
}
