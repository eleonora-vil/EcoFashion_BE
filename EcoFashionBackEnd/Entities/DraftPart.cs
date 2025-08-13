using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("DraftParts")]
    public class DraftPart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartId { get; set; }

        // FK đến bản Draft (Design)

        public int DesignId { get; set; }
        [ForeignKey("DesignId")]
        public virtual Design Design { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Tên part: "Sleeve", "Collar"...

        [Column(TypeName = "decimal(10,2)")]
        public decimal Length { get; set; } // cm

        [Column(TypeName = "decimal(10,2)")]
        public decimal Width { get; set; }  // cm

        public int Quantity { get; set; }

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        public MaterialStatus MaterialStatus { get; set; } = MaterialStatus.Main;
    }

    public enum MaterialStatus
    {
        Main,       // Vật liệu chính
        Supplement, // Vật liệu phụ trợ
        Other       // Vật liệu khác
    }
}

