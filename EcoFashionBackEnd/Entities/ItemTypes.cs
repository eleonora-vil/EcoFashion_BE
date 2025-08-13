    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("ItemTypes")]
    public class ItemType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty; // Ví dụ: "Áo thun", "Váy đầm", "Quần jeans"

        [MaxLength(255)]
        public string? Description { get; set; } // Mô tả 

        public virtual ICollection<Design> Designs { get; set; } = new List<Design>();

        public virtual ICollection<ItemTypeSizeRatio> TypeSizeRatios { get; set; } = new List<ItemTypeSizeRatio>();
    }
}