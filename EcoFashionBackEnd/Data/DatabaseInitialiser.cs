//// DatabaseInitialiser.cs – version đầy đủ với dữ liệu mẫu cho tất cả vai trò + 12 Designs + 12 Materials

//using EcoFashionBackEnd.Entities;
//using EcoFashionBackEnd.Helpers;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;

//namespace EcoFashionBackEnd.Data
//{
//    public interface IDataaseInitialiser
//    {
//        Task InitialiseAsync();
//        Task SeedAsync();
//        Task TrySeedAsync();
//    }

//    public class DatabaseInitialiser : IDataaseInitialiser
//    {
//        private readonly AppDbContext _context;

//        public DatabaseInitialiser(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task InitialiseAsync()
//        {
//            try
//            {
//                await _context.Database.MigrateAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Migration error: " + ex.Message);
//                throw;
//            }
//        }

//        public async Task SeedAsync()
//        {
//            try
//            {
//                await TrySeedAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Seed error: " + ex.Message);
//                throw;
//            }
//        }

//        public async Task TrySeedAsync()
//        {
//            if (_context.UserRoles.Any() || _context.Users.Any() || _context.DesignsColors.Any()) return;

//            var now = DateTime.UtcNow;

//            // User Roles
//            var adminRole = new UserRole { RoleName = "admin", Description = "System administrator" };
//            var designerRole = new UserRole { RoleName = "designer", Description = "Fashion designer" };
//            var supplierRole = new UserRole { RoleName = "supplier", Description = "Material supplier" };
//            var customerRole = new UserRole { RoleName = "customer", Description = "Customer user" };
//            await _context.UserRoles.AddRangeAsync(adminRole, designerRole, supplierRole, customerRole);
//            await _context.SaveChangesAsync();

//            // Users
//            var users = new List<User>
//            {
//                new User { Email = "admin@example.com", PasswordHash = SecurityUtil.Hash("admin"), FullName = "Admin User", RoleId = adminRole.RoleId, Status = UserStatus.Active },
//                new User { Email = "designer@example.com", PasswordHash = SecurityUtil.Hash("designer"), FullName = "Designer One", RoleId = designerRole.RoleId, Status = UserStatus.Active },
//                new User { Email = "supplier@example.com", PasswordHash = SecurityUtil.Hash("supplier"), FullName = "Supplier One", RoleId = supplierRole.RoleId, Status = UserStatus.Active },
//                new User { Email = "customer@example.com", PasswordHash = SecurityUtil.Hash("customer"), FullName = "Customer One", RoleId = customerRole.RoleId, Status = UserStatus.Active }
//            };
//            await _context.Users.AddRangeAsync(users);
//            await _context.SaveChangesAsync();

//            var designerUser = users.First(u => u.Email == "designer@example.com");
//            var supplierUser = users.First(u => u.Email == "supplier@example.com");
//            var customerUser = users.First(u => u.Email == "customer@example.com");

//            // Designer
//            var designer = new Designer
//            {
//                UserId = designerUser.UserId,
//                DesignerName = "Designer One",
//                Bio = "Nhà thiết kế đam mê thời trang bền vững và nghệ thuật tự nhiên.",
//                AvatarUrl = "https://images.pexels.com/photos/32965438/pexels-photo-32965438.jpeg",
//                BannerUrl = "https://images.unsplash.com/photo-1580828343064-fde4fc206bc6?q=80&w=2071&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
//                PortfolioUrl = "https://portfolio.designer1.com",
//                PortfolioFiles = JsonSerializer.Serialize(new[] { "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400" }),
//                Certificates = JsonSerializer.Serialize(new[] { "Eco Fashion Award 2023" }),
//                Email = "designer1@example.com",
//                PhoneNumber = "0123456789",
//                Address = "123 Eco Street, HCMC",
//                TaxNumber = "TAX123456",
//                IdentificationNumber = "ID123456789",
//                //IdentificationPictureFront = "https://images.unsplash.com/photo-1519125323398-675f0ddb6308?w=300",
//                //IdentificationPictureBack = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=300",
//                SpecializationUrl = "https://specialization.designer1.com",
//                Status = "active",
//                CreatedAt = now
//            };
//            await _context.Designers.AddAsync(designer);
//            await _context.SaveChangesAsync();

//            // Supplier
//            var supplier = new Supplier
//            {
//                UserId = supplierUser.UserId,
//                SupplierName = "Supplier One",
//                Bio = "Chuyên cung cấp chất liệu thời trang thân thiện với môi trường.",
//                AvatarUrl = "https://plus.unsplash.com/premium_photo-1675179040377-24f37aeb2146?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
//                BannerUrl = "https://plus.unsplash.com/premium_photo-1675179040377-24f37aeb2146?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
//                PortfolioUrl = "https://supplier1.com",
//                PortfolioFiles = JsonSerializer.Serialize(new[] { "https://images.unsplash.com/photo-1504198453319-5ce911bafcde?w=400" }),
//                Certificates = JsonSerializer.Serialize(new[] { "Green Supplier Certificate" }),
//                Email = "supplier1@example.com",
//                PhoneNumber = "0987654321",
//                Address = "456 Green Lane, HCMC",
//                TaxNumber = "TAXSUP123",
//                IdentificationNumber = "SUPID987654321",
//                //IdentificationPictureFront = "https://images.unsplash.com/photo-1519125323398-675f0ddb6308?w=300",
//                //IdentificationPictureBack = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=300",
//                SpecializationUrl = "https://specialization.supplier1.com",
//                Status = "active",
//                CreatedAt = DateTime.UtcNow
//            };
//            await _context.SaveChangesAsync();

//            //// Seed DesignsColor
//            //var colors = new List<DesignsColor>
//            //{
//            //    new DesignsColor { ColorName = "Red", ColorCode = "#FF0000" },
//            //    new DesignsColor { ColorName = "Blue", ColorCode = "#0000FF" },
//            //    new DesignsColor { ColorName = "Green", ColorCode = "#008000" }
//            //};
//            //await _context.DesignsColors.AddRangeAsync(colors);
//            //await _context.SaveChangesAsync();

//            //// Seed DesignsSize
//            //var sizes = new List<DesignsSize>
//            //{
//            //    new DesignsSize { SizeName = "S", SizeDescription = "Small" },
//            //    new DesignsSize { SizeName = "M", SizeDescription = "Medium" },
//            //    new DesignsSize { SizeName = "L", SizeDescription = "Large" }
//            //};
//            //await _context.DesignsSizes.AddRangeAsync(sizes);
//            //await _context.SaveChangesAsync();

//            //// Seed DesignType
//            //var designTypes = new List<DesignsType>
//            //{
//            //   // new DesignsType { DesignName = "T-Shirt", SizeId = sizes.First(s => s.SizeName == "M").Id, Meter = 1.5f },
//            //    // new DesignsType { DesignName = "Jeans", SizeId = sizes.First(s => s.SizeName == "L").Id, Meter = 2.5f }
//            //};
//            //await _context.DesignsTypes.AddRangeAsync(designTypes);
//            //await _context.SaveChangesAsync();

//            //// Seed a Design
//            //var design = new Design
//            //{
//            //    DesignerId = designer.DesignerId,

//            //    RecycledPercentage = 50.0f,
//            //    CareInstructions = "Machine wash cold.",
//            //    Price = 25.99m,
//            //    Quantity = 100,
//            //    ProductScore = 4,
//            //    Status = "approved",
//            //    CreatedAt = DateTime.UtcNow
//            //};
//            //await _context.Designs.AddAsync(design);
//            //await _context.SaveChangesAsync();

//            // Seed Images
//            //var images = new List<Image>
//            //{
//            //    new Image { ImageUrl = "https://images.unsplash.com/photo-1465101046530-73398c7f28ca?w=400" },
//            //    new Image { ImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=400" }
//            //};
//            //await _context.Images.AddRangeAsync(images);
//            //await _context.SaveChangesAsync();

//            // Seed DesignImages 
//            //var designImages = new List<DesignImage>
//            //{
//            //    new DesignImage { DesignId = design.DesignId, ImageId = images[0].ImageId },
//            //    new DesignImage { DesignId = design.DesignId, ImageId = images[1].ImageId }
//            //};
//            //await _context.DesignImages.AddRangeAsync(designImages);
//            //await _context.SaveChangesAsync();
//        }
//    }

//    public static class DatabaseInitialiserExtension
//    {
//        public static async Task InitialiseDatabaseAsync(this WebApplication app)
//        {
//            using var scope = app.Services.CreateScope();
//            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitialiser>();
//            await initializer.InitialiseAsync();
//            await initializer.SeedAsync();
//        }
//    }
//}
