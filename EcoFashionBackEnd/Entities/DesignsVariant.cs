using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("DesignsVariants")]
    public class DesignsVariant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DesignId { get; set; }
        [ForeignKey(nameof(DesignId))]
        public virtual Design Design { get; set; }

        public int SizeId { get; set; }
        [ForeignKey(nameof(SizeId))]
        public virtual Size Size { get; set; }

        // Mã màu hoặc tên màu
        [MaxLength(50)]
        public string ColorCode { get; set; }
        public int Quantity { get; set; }


    }
}