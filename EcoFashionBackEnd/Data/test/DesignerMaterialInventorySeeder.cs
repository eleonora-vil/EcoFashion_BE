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
            var materialWarehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.WarehouseType == "Material");

            if (designer == null || materials.Count == 0 || materialWarehouse == null) return;

            var random = new Random();
            var inventoryList = new List<DesignerMaterialInventory>();

            foreach (var material in materials)
            {
                var quantity = random.Next(50, 201);
                inventoryList.Add(new DesignerMaterialInventory
                {
                    WarehouseId = materialWarehouse.WarehouseId, // Assign the correct WarehouseId
                    MaterialId = material.MaterialId,
                    Quantity = quantity,
                    Cost = (decimal)material.PricePerUnit * 1000 * quantity,
                    LastBuyDate = DateTime.UtcNow.AddDays(-random.Next(600, 900)),
                    Status = "In Stock"
                });
            }

            await context.DesignerMaterialInventories.AddRangeAsync(inventoryList);
            await context.SaveChangesAsync();
        }
    }

}
