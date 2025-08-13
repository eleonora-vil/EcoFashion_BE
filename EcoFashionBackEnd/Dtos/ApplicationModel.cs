using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Dtos
{
    public class ApplicationModel
    {
        public int ApplicationId { get; set; }
        public int UserId { get; set; }
        public int TargetRoleId { get; set; }
        
        // Portfolio & Profile Images
        public string? AvatarUrl { get; set; } // https://www.ecofashion-example.com/
        public string? PortfolioUrl { get; set; }
        public string? PortfolioFiles { get; set; } // JSON array of file urls
        public string? BannerUrl { get; set; } // https://www.ecofashion-example.com/
        public string? SpecializationUrl { get; set; }
        public string? Bio { get; set; }

        // Social Media
        public string? SocialLinks { get; set; } // JSON object of social media links { instagram: "...", behance: "..." }

        // Identification / Xác minh - 2 hình CCCD riêng biệt
        public string? IdentificationNumber { get; set; }
        public string? IdentificationPictureFront { get; set; } 
        public string? IdentificationPictureBack { get; set; } 
        public bool IsIdentificationVerified { get; set; } = false;

        //Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }    // Thời gian admin xử lý

        // Kết quả xử lý
        public int? ProcessedBy { get; set; }         // Admin ID xử lý
        public string? RejectionReason { get; set; }  // Lý do từ chối (nếu rejected)
        public string? Note { get; set; }

        // Thông tin liên hệ bổ sung cho Designer/Supplier
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        // Thông tin thuế
        public string? TaxNumber { get; set; }

        // Chứng chỉ/giải thưởng
        public string? Certificates { get; set; }

        public string Status { get; set; } = "pending";
        
        // Navigation properties
        public UserModel? User { get; set; }
        public UserRoleModel? Role { get; set; }
        public UserModel? ProcessedByUser { get; set; }
    }
}
