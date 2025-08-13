using EcoFashionBackEnd.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Dtos
{
    public class UserModel
    {
        public int UserId { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(10)]
        public string? Phone { get; set; }

        [StringLength(30, MinimumLength = 8)]
        public string? Username { get; set; }

        [Required]
        public  string? PasswordHash { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        public int RoleId { get; set; } 
                                        

        [Required]
        [EnumDataType(typeof(UserStatus))]
        public UserStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public enum UserStatus
    {
        Pending,
        Active,
        Rejected,
        Inactive
    }
}
