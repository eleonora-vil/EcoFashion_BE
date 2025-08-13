using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoFashionBackEnd.Entities
{
    [Table("Designer")]
    public class Designer
    {
        [Key]
        public Guid DesignerId { get; set; } = Guid.NewGuid();
        //Thông tin người dùng
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        //Thông tin nhà thiết kế
        [StringLength(100)]
        public string? DesignerName { get; set; }
        public string? AvatarUrl { get; set; } //URL của ảnh đại diện

        //Giới thiệu chuyên môn
        public string? Bio { get; set; }
        public string? SpecializationUrl { get; set; }

        //Portfolio
        public string? PortfolioUrl { get; set; } //URL của portfolio ngoài
        public string? PortfolioFiles { get; set; } //JSON array các ảnh portfolio files
        public string? BannerUrl { get; set; } //URL của banner

        //Thông tin liên hệ
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        //Thông tin thuế
        public string? TaxNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationPictureFront { get; set; }
        public string? IdentificationPictureBack { get; set; }


        // Trạng thái & tracking
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Optional
        public double? Rating { get; set; } // Điểm đánh giá trung bình
        public int? ReviewCount { get; set; } // Số lượng đánh giá
        public string? Certificates { get; set; } // JSON array các chứng chỉ/giải thưởng
        public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    }
}
