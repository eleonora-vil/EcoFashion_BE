using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class ItemTypeSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.ItemTypes.AnyAsync()) return;

            var itemTypes = new[]
            {
            new ItemType { TypeName = "Áo", Description = "Các loại áo như áo thun, áo hoodie" },
            new ItemType { TypeName = "Quần", Description = "Các loại quần như quần jeans, quần shorts" },
            new ItemType { TypeName = "Váy", Description = "Các loại váy nữ" },
            new ItemType { TypeName = "Đầm", Description = "Các loại đầm thời trang" }
        };

            await context.ItemTypes.AddRangeAsync(itemTypes);
            await context.SaveChangesAsync();
        }
    }
}
