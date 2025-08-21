using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class DesignsVariantSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.DesignsVarients.AnyAsync()) return;

            var designs = await context.Designs.ToListAsync();
            var sizes = await context.Sizes.ToListAsync();

            var colors = new[] { "RED", "BLK", "WHT", "BLU", "GRN" }; // Màu demo

            var variants = new List<DesignsVariant>();
            var random = new Random();

            foreach (var design in designs)
            {
                foreach (var size in sizes)
                {
                    // Tạo 1-3 variant màu cho mỗi size-design
                    int variantsCount = random.Next(1, 4);

                    var shuffledColors = colors.OrderBy(c => random.Next()).Take(variantsCount).ToList();

                    foreach (var color in shuffledColors)
                    {
                        variants.Add(new DesignsVariant
                        {
                            DesignId = design.DesignId,
                            SizeId = size.SizeId,
                            ColorCode = color,
                            Quantity = random.Next(10, 50) // số lượng ngẫu nhiên
                        });
                    }
                }
            }

            await context.DesignsVarients.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }
    }
}
