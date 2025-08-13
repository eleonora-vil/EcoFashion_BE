using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("DraftSketches")]
    public class DraftSketch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SketchImageId { get; set; }

        [ForeignKey("Design")]
        public int DesignId { get; set; }
        public virtual Design Design { get; set; }

        [ForeignKey("Image")]
        public int ImageId { get; set; }
        public virtual Image Image { get; set; }

    }
}
