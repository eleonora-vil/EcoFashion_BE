using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("DesignsRating")]
    [PrimaryKey(nameof(DesignIdPk), nameof(UserIdPk))] // Định nghĩa composite key ở đây
    public class DesignsRating
    {
        [ForeignKey("DesignId")]
        public int DesignId { get; set; }
        public virtual Design Design { get; set; }

        public int UserId { get; set; }

        public float RatingScore { get; set; }

        [Column(Order = 0)]
        public int DesignIdPk { get; set; } // Phần 1 của Composite Key

        [Column(Order = 1)]
        public int UserIdPk { get; set; } // Phần 2 của Composite Key
    }
}