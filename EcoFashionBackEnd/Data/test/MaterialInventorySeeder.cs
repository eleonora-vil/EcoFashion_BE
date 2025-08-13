using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Data.test
{
    public static class MaterialInventorySeeder
    {
        // Seed 1 warehouse for supplier one and create MaterialStocks for its materials
        public static async Task SeedAsync(AppDbContext context)
        {
            // Find first supplier
            var supplier = await context.Suppliers.FirstOrDefaultAsync();
            if (supplier == null) return;

            // Ensure a default warehouse exists
            var warehouse = await context.Warehouses
                .FirstOrDefaultAsync(w => w.SupplierId == supplier.SupplierId && w.IsDefault);
            if (warehouse == null)
            {
                warehouse = new Warehouse
                {
                    Name = "Kho nhà cung cấp",
                    WarehouseType = "Material",
                    SupplierId = supplier.SupplierId,
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Warehouses.Add(warehouse);
                await context.SaveChangesAsync();
            }

            // Create stock rows for all materials of this supplier if missing
            var materials = await context.Materials
                .Where(m => m.SupplierId == supplier.SupplierId)
                .ToListAsync();

            foreach (var material in materials)
            {
                var exists = await context.MaterialStocks.AnyAsync(s => s.MaterialId == material.MaterialId && s.WarehouseId == warehouse.WarehouseId);
                if (exists) continue;
                context.MaterialStocks.Add(new MaterialStock
                {
                    MaterialId = material.MaterialId,
                    WarehouseId = warehouse.WarehouseId,
                    // Initialise stock equal to material.QuantityAvailable for demo
                    QuantityOnHand = Convert.ToDecimal(material.QuantityAvailable),
                    // Set min threshold to 20% of quantity (rounded down)
                    MinThreshold = Math.Max(0, (int)Math.Floor(material.QuantityAvailable * 0.2m)),
                    LastUpdated = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }
    }
}


