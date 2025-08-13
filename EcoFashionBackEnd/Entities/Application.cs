using EcoFashionBackEnd.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum ApplicationStatus
{
    pending,
    approved,
    rejected
}

 [Table("Applications")]
 public class Application
 {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ApplicationId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User? User { get; set; }

    [ForeignKey("Role")]
    public int TargetRoleId { get; set; }
    public virtual UserRole? Role { get; set; }
     
     // Portfolio & Profile Images
    public string? AvatarUrl { get; set; } // Ảnh đại diện cho profile page
    public string? PortfolioUrl { get; set; }
    public string? PortfolioFiles { get; set; } // JSON array of file urls
    public string? BannerUrl { get; set; } // Ảnh banner cho landing page
    public string? SpecializationUrl { get; set; }
    public string? Bio { get; set; }

    // Social Media
    public string? SocialLinks { get; set; } // JSON object of social media links { instagram: "...", behance: "..." }

    // Identification / Xác minh
    public string? IdentificationNumber { get; set; }
    public string? IdentificationPictureFront { get; set; } // URL của hình mặt trước CCCD
    public string? IdentificationPictureBack { get; set; }  // URL của hình mặt sau CCCD
    public bool IsIdentificationVerified { get; set; } = false;

    //Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }    // Thời gian admin xử lý

    // Kết quả xử lý
    public int? ProcessedBy { get; set; }         // Admin ID xử lý
    public string? RejectionReason { get; set; }  // Lý do từ chối (nếu rejected)
    public string? Note { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.pending;

    // Thông tin liên hệ bổ sung cho Designer/Supplier
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    // Thông tin thuế
    public string? TaxNumber { get; set; }

    // Chứng chỉ/giải thưởng
    public string? Certificates { get; set; }
}