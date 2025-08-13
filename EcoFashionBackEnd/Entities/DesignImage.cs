using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace EcoFashionBackEnd.Entities
{
    [Table("DesignImages")]
    public class DesignImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DesignImageId { get; set; }

        public int DesignId { get; set; }
        [ForeignKey("DesignId")]
        public virtual Design Design { get; set; }

        public int ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual Image Image { get; set; }
    }
}