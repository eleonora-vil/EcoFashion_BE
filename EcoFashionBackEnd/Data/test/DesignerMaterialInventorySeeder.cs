using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class DesignerMaterialInventorySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.DesignerMaterialInventories.AnyAsync()) return;

            var designer = await context.Designers.FirstOrDefaultAsync();
            var materials = await context.Materials.ToListAsync();

            if (designer == null || materials.Count == 0) return;

            var random = new Random();
            var inventoryList = new List<DesignerMaterialInventory>();

            foreach (var material in materials)
            {
                inventoryList.Add(new DesignerMaterialInventory
                {
                    DesignerId = designer.DesignerId,
                    MaterialId = material.MaterialId,
                    Quantity = random.Next(50, 201), // từ 50 đến 200 mét
                    Cost = (decimal)(random.NextDouble() * 50 + 10), // từ 10.0 đến 60.0
                    LastBuyDate = DateTime.UtcNow.AddDays(-random.Next(1, 90)), // trong 3 tháng gần nhất
                    Status = "In Stock"
                });
            }

            await context.DesignerMaterialInventories.AddRangeAsync(inventoryList);
            await context.SaveChangesAsync();
        }
    }

}
