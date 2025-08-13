using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("BlogImages")]
    public class BlogImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlogImageId { get; set; }
        public int BlogId { get; set; }
        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; } = null!;
        public int ImageId { get; set; }
        [ForeignKey("ImageId")]
        public virtual Image Image { get; set; } = null!;
    }
}
