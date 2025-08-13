using System.ComponentModel.DataAnnotations;

namespace EcoFashionBackEnd.Dtos
{
    public class UserRoleModel
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }
    }
}
