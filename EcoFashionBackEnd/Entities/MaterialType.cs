using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("MaterialTypes")]
    public class MaterialType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int TypeId { get; set; }

        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsOrganic { get; set; } = false;
        public bool IsRecycled { get; set; } = false;
        public string? SustainabilityNotes { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}