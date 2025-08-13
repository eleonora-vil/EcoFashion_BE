using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Sizes")]
    public class Size
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SizeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SizeName { get; set; } = string.Empty; // Ví dụ: S, M, L, XL

        [MaxLength(255)]
        public string? SizeDescription { get; set; } // Mô tả nếu cần

        public virtual ICollection<DesignsVariant> Variants { get; set; } = new List<DesignsVariant>();
        public virtual ICollection<ItemTypeSizeRatio> TypeSizeRatios { get; set; } = new List<ItemTypeSizeRatio>();
    }
}