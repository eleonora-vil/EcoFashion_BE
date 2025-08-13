using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var adminRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "admin");
            var designerRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "designer");
            var supplierRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "supplier");
            var customerRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "customer");

            if (adminRole == null || designerRole == null || supplierRole == null || customerRole == null) return;

            var users = new List<User>
        {
            new User { Email = "admin@example.com", PasswordHash = SecurityUtil.Hash("admin"), FullName = "Admin User", RoleId = adminRole.RoleId, Status = UserStatus.Active },
            new User { Email = "designer@example.com", PasswordHash = SecurityUtil.Hash("designer"), FullName = "Designer One", RoleId = designerRole.RoleId, Status = UserStatus.Active },
            new User { Email = "supplier@example.com", PasswordHash = SecurityUtil.Hash("supplier"), FullName = "Supplier One", RoleId = supplierRole.RoleId, Status = UserStatus.Active },
            new User { Email = "customer@example.com", PasswordHash = SecurityUtil.Hash("customer"), FullName = "Customer One", RoleId = customerRole.RoleId, Status = UserStatus.Active }
        };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }

}