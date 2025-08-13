using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class ItemTypeSizeRatioSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.ItemTypeSizeRatios.AnyAsync()) return;

            var itemTypes = await context.ItemTypes.ToListAsync();
            var sizes = await context.Sizes.ToListAsync();

            var ratios = new List<ItemTypeSizeRatio>();

            // Định nghĩa ratio cho từng loại và size
            var ratiosMap = new Dictionary<string, Dictionary<string, float>>
            {
                ["Áo"] = new Dictionary<string, float> { ["S"] = 0.85f, ["M"] = 1.0f, ["L"] = 1.1f, ["XL"] = 1.2f },
                ["Quần"] = new Dictionary<string, float> { ["S"] = 0.9f, ["M"] = 1.0f, ["L"] = 1.3f, ["XL"] = 1.4f },
                ["Váy"] = new Dictionary<string, float> { ["S"] = 0.8f, ["M"] = 1.0f, ["L"] = 1.2f, ["XL"] = 1.3f },
                ["Đầm"] = new Dictionary<string, float> { ["S"] = 0.82f, ["M"] = 1.0f, ["L"] = 1.15f, ["XL"] = 1.25f }
            };

            foreach (var itemType in itemTypes)
            {
                if (!ratiosMap.ContainsKey(itemType.TypeName)) continue;

                var sizeRatios = ratiosMap[itemType.TypeName];

                foreach (var size in sizes)
                {
                    if (sizeRatios.TryGetValue(size.SizeName, out var ratio))
                    {
                        ratios.Add(new ItemTypeSizeRatio
                        {
                            ItemTypeId = itemType.ItemTypeId,
                            SizeId = size.SizeId,
                            Ratio = ratio
                        });
                    }
                }
            }

            await context.ItemTypeSizeRatios.AddRangeAsync(ratios);
            await context.SaveChangesAsync();
        }
    }

}
