using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Dtos.User
{
    public class UserLoginRequest
    {
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
