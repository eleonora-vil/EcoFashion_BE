using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public int DesignId { get; set; }
        [ForeignKey(nameof(DesignId))]
        public virtual Design Design { get; set; }

        public int SizeId { get; set; }
        [ForeignKey(nameof(SizeId))]
        public virtual Size Size { get; set; }

        [MaxLength(50)]
        public string ColorCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public virtual ICollection<ProductInventory> Inventories { get; set; } = new List<ProductInventory>();
    }

}
