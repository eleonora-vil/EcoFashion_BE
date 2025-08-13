using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace EcoFashionBackEnd.Entities
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }
        [Required]
        public required int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        // Cho phép null để chỉ có một loại ID
        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        public int? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual Material? Material { get; set; }
        [MaxLength(1000)]
        public string? Comment { get; set; }
        [Required]
        public required decimal RatingScore { get; set; }
    }
}
