using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("DesignFeatures")]
    public class DesignFeature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeatureId { get; set; }

        public int DesignId { get; set; }
        [ForeignKey(nameof(DesignId))]
        public virtual Design Design { get; set; }

        public bool ReduceWaste { get; set; }
        public bool LowImpactDyes { get; set; }
        public bool Durable { get; set; }
        public bool EthicallyManufactured { get; set; }
    }
}
