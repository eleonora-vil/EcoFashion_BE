using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("ItemTypeSizeRatios")]
    public class ItemTypeSizeRatio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ItemTypeId { get; set; }
        [ForeignKey(nameof(ItemTypeId))]
        public virtual ItemType ItemType { get; set; }

        public int SizeId { get; set; }
        [ForeignKey(nameof(SizeId))]
        public virtual Size Size { get; set; }

        // Hệ số áp dụng theo size cho loại sản phẩm
        public float Ratio { get; set; }
    }
}