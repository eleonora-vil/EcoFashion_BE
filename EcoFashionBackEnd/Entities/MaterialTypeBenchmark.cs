using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("MaterialTypeBenchmarks")]
    public class MaterialTypeBenchmark
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BenchmarkId { get; set; }
        public virtual MaterialType MaterialType { get; set; } = null!;
        [ForeignKey("MaterialType")]
        public int TypeId { get; set; }
        public virtual SustainabilityCriteria SustainabilityCriteria { get; set; } = null!;
        [ForeignKey("SustainabilityCriteria")]
        public int CriteriaId { get; set; }
        public decimal Value { get; set; }
    }
}
