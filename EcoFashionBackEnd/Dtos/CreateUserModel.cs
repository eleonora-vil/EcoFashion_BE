using EcoFashionBackEnd.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Dtos
{
    public class CreateUserModel
    {
        public int UserId { get; set; }

        [MaxLength(255)]
        public string? UserName { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        [MaxLength(50)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(100)]
        public string? Gender { get; set; }

        
        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }
        public string? OTPCode { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        public DateTime? CreateDate { get; set; }


        public string? Status { get; set; }

        public int RoleID { get; set; }
    }
}
