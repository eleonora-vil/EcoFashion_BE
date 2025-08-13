using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class ApplicationSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Applications.AnyAsync()) return;

            // Tạo customer user nếu chưa có
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@example.com");
            if (customerUser == null)
            {
                var customerRole = await context.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "customer");
                if (customerRole == null) return;

                customerUser = new User
                {
                    Email = "customer@example.com",
                    PasswordHash = SecurityUtil.Hash("customer"),
                    FullName = "Customer Example",
                    RoleId = customerRole.RoleId,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
                await context.Users.AddAsync(customerUser);
                await context.SaveChangesAsync();
            }

            var applications = new List<Application>
            {
                new Application
                {
                    UserId = customerUser.UserId,
                    TargetRoleId = 2, // Designer
                    Status = ApplicationStatus.pending,
                    Bio = "Tôi là một nhà thiết kế thời trang đam mê với thời trang bền vững.",
                    PhoneNumber = "0123456789",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    IdentificationNumber = "123456789012",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Note = "Portfolio rất ấn tượng với các thiết kế bền vững"
                },
                new Application
                {
                    UserId = customerUser.UserId,
                    TargetRoleId = 3, // Supplier
                    Status = ApplicationStatus.pending,
                    Bio = "Cung cấp các loại vải bền vững và thân thiện môi trường.",
                    PhoneNumber = "0987654321",
                    Address = "456 Đường XYZ, Quận 2, TP.HCM",
                    IdentificationNumber = "987654321098",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    Note = "Có chứng chỉ GOTS và OEKO-TEX"
                },
                new Application
                {
                    UserId = customerUser.UserId,
                    TargetRoleId = 2, // Designer
                    Status = ApplicationStatus.approved,
                    Bio = "Nhà thiết kế chuyên về thời trang công sở bền vững.",
                    PhoneNumber = "0555666777",
                    Address = "789 Đường DEF, Quận 3, TP.HCM",
                    IdentificationNumber = "112233445566",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    ProcessedAt = DateTime.UtcNow.AddDays(-2),
                    Note = "Đã phê duyệt - Thiết kế rất chuyên nghiệp"
                }
            };

            await context.Applications.AddRangeAsync(applications);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {applications.Count} applications");
        }
    }
} 