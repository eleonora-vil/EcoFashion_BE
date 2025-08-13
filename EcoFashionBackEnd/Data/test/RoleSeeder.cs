using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.UserRoles.AnyAsync()) return;

            var roles = new List<UserRole>
        {
            new UserRole { RoleName = "admin" ,Description = "System administrator"},
            new UserRole { RoleName = "designer", Description = "Fashion designer"  },
            new UserRole { RoleName = "supplier", Description = "Material supplier"  },
            new UserRole { RoleName = "customer", Description = "Customer user"  }
        };

            await context.UserRoles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }

}