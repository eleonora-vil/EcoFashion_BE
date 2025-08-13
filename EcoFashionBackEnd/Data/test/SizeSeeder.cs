using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class SizeSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Sizes.AnyAsync()) return;

            var sizes = new[]
            {
            new Size { SizeName = "S", SizeDescription = "Small" },
            new Size { SizeName = "M", SizeDescription = "Medium" },
            new Size { SizeName = "L", SizeDescription = "Large" },
            new Size { SizeName = "XL", SizeDescription = "Extra Large" }
        };

            await context.Sizes.AddRangeAsync(sizes);
            await context.SaveChangesAsync();
        }
    }
}
