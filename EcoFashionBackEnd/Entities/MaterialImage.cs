using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("MaterialImages")]
    public class MaterialImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaterialImageId { get; set; }

        public int MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual Material Material { get; set; }

        public int ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual Image Image { get; set; }
    }
}
