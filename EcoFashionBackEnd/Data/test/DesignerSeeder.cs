using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcoFashionBackEnd.Data.test
{
    public static class DesignerSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Designers.AnyAsync()) return;

            var designerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "designer@example.com");
            if (designerUser == null) throw new Exception("Designer user not found");

            var now = DateTime.UtcNow;

            var designer = new Designer
            {
                UserId = designerUser.UserId,
                DesignerName = "Designer One",
                Bio = "Nhà thiết kế đam mê thời trang bền vững và nghệ thuật tự nhiên.",
                AvatarUrl = "https://images.pexels.com/photos/32965438/pexels-photo-32965438.jpeg",
                BannerUrl = "https://images.unsplash.com/photo-1580828343064-fde4fc206bc6?q=80&w=2071&auto=format&fit=crop",
                PortfolioUrl = "https://portfolio.designer1.com",
                PortfolioFiles = JsonSerializer.Serialize(new[]
                {
                "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400"
            }),
                Certificates = JsonSerializer.Serialize(new[]
                {
                "Eco Fashion Award 2023"
            }),
                Email = "designer1@example.com",
                PhoneNumber = "0123456789",
                Address = "123 Eco Street, HCMC",
                TaxNumber = "TAX123456",
                IdentificationNumber = "ID123456789",
                //IdentificationPictureFront = "https://images.unsplash.com/photo-1519125323398-675f0ddb6308?w=300",
                //IdentificationPictureBack = "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=300",
                SpecializationUrl = "https://specialization.designer1.com",
                Status = "active",
                CreatedAt = now
            };

            await context.Designers.AddAsync(designer);
            await context.SaveChangesAsync();
            // Tạo Warehouse cho designer vừa tạo
            var warehouses = new List<Warehouse>
    {
        new Warehouse
        {
            DesignerId = designer.DesignerId,
            WarehouseType = "Material"
        },
        new Warehouse
        {
            DesignerId = designer.DesignerId,
            WarehouseType = "Product"
        }
    };

            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();
        }
    }

}