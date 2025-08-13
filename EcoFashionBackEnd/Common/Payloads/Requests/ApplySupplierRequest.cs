using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class ApplySupplierRequest
    {
    // Profile images for profile page
        public IFormFile? AvatarFile { get; set; } // Ảnh đại diện
        public IFormFile? BannerFile { get; set; } // Ảnh banner/cover
        
        public string? PortfolioUrl { get; set; }
        
        // Hỗ trợ upload multiple portfolio files
        public List<IFormFile>? PortfolioFiles { get; set; }
        
        public string? SpecializationUrl { get; set; }
        public string? Bio { get; set; }
        
        // Social media links as JSON string
        [MaxLength(500)]
        public string? SocialLinks { get; set; } // {"instagram": "url", "behance": "url", "facebook": "url"}
        
        public string? IdentificationNumber { get; set; }
        public IFormFile? IdentificationPictureFront { get; set; } 
        public IFormFile? IdentificationPictureBack { get; set; } 
        
        public string? Note { get; set; }

        // Thông tin liên hệ bổ sung cho Designer
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        // Thông tin thuế
        public string? TaxNumber { get; set; }

        // Chứng chỉ/giải thưởng
        public string? Certificates { get; set; }
    }
}
 