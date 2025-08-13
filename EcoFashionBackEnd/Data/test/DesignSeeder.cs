using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{

    public static class DesignSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var designNames = new[]
            {
            "Eco Hoodie", "Organic T-Shirt", "Sustainable Jeans", "Linen Blouse",
            "Recycled Jacket", "Eco Dress", "Vegan Leather Bag", "Bamboo Shirt",
            "RPET Shorts", "Hemp Hat", "Green Socks", "Bio Cotton Skirt"
        };

            if (await context.Designs.AnyAsync()) return;

            var designer = await context.Designers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == "designer@example.com");

            if (designer == null) throw new Exception("Designer not found");

            var itemTypes = await context.ItemTypes.ToListAsync();
            if (itemTypes.Count == 0)
                throw new Exception("ItemTypes chưa seed hoặc không tồn tại");

            var random = new Random();
            int minPrice = 500_000;
            int maxPrice = 1_000_000;

            var designs = new List<Design>();
            var designFeatures = new List<DesignFeature>();

            for (int i = 0; i < designNames.Length; i++)
            {
                var design = new Design
                {
                    Name = designNames[i],
                    Description = $"This is a sustainable design: {designNames[i]}",
                    DesignerId = designer.DesignerId,
                    RecycledPercentage = 100 - i, // Giảm dần
                    SalePrice = random.Next(minPrice, maxPrice + 1),
                    ProductScore = 5,
                    CreatedAt = DateTime.UtcNow,
                    CareInstruction = "Wash cold, hang dry",
                    ItemTypeId = itemTypes[i % itemTypes.Count].ItemTypeId,
                    CarbonFootprint = (float)Math.Round(random.NextDouble() * 50, 2),
                    WaterUsage = (float)Math.Round(random.NextDouble() * 100, 2),
                    WasteDiverted = (float)Math.Round(random.NextDouble() * 20, 2),
                    LaborHours = (float)Math.Round(random.NextDouble() * 10, 2),
                    LaborCostPerHour = (decimal)Math.Round(random.NextDouble() * 50, 2)
                };

                designs.Add(design);

                var feature = new DesignFeature
                {
                    Design = design, // Gán đối tượng design luôn
                    ReduceWaste = random.Next(0, 2) == 1,
                    LowImpactDyes = random.Next(0, 2) == 1,
                    Durable = random.Next(0, 2) == 1,
                    EthicallyManufactured = random.Next(0, 2) == 1
                };

                designFeatures.Add(feature);
            }

            await context.Designs.AddRangeAsync(designs);
            await context.DesignFeatures.AddRangeAsync(designFeatures);

            await context.SaveChangesAsync();
        }
    }

}


