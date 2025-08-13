namespace EcoFashionBackEnd.Dtos
{
    public class SupplierModel
    {
        public Guid SupplierId { get; set; }
        public int UserId { get; set; }

        // Thông tin nhà cung cấp
        public string? SupplierName { get; set; }
        public string? AvatarUrl { get; set; } // URL của ảnh đại diện

        // Giới thiệu chuyên môn
        public string? Bio { get; set; }
        public string? SpecializationUrl { get; set; }

        // Portfolio
        public string? PortfolioUrl { get; set; } // URL của portfolio ngoài
        public string? PortfolioFiles { get; set; } // JSON array các ảnh portfolio files
        public string? BannerUrl { get; set; } // URL của banner

        // Thông tin liên hệ
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Thông tin thuế (chỉ hiển thị cho admin/chính chủ)
        public string? TaxNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? IdentificationPictureFront { get; set; }
        public string? IdentificationPictureBack { get; set; }

        // Trạng thái & tracking
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Đánh giá và chứng chỉ
        public double? Rating { get; set; } // Điểm đánh giá trung bình
        public int? ReviewCount { get; set; } // Số lượng đánh giá
        public string? Certificates { get; set; } // JSON array các chứng chỉ/giải thưởng

        // Navigation properties
        public UserModel? User { get; set; }
    }

    // DTO for public landing pages (ẩn thông tin sensitive)
    public class SupplierPublicModel
    {
        public Guid SupplierId { get; set; }

        // Thông tin công khai
        public string? SupplierName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? SpecializationUrl { get; set; }

        // Portfolio
        public string? PortfolioUrl { get; set; }
        public string? PortfolioFiles { get; set; }
        public string? BannerUrl { get; set; }

        // Contact info (có thể ẩn email/phone tùy setting)
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Public metrics
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public string? Certificates { get; set; }
        public DateTime CreatedAt { get; set; }

        // User info
        public string? UserFullName { get; set; }
    }

    // DTO for listing/search pages
    public class SupplierSummaryModel
    {
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? BannerUrl { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
