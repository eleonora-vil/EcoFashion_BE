using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class WarehouseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Warehouses.AnyAsync()) return;

            // Lấy designer vừa seed, ví dụ chỉ có 1 cái
            var designer = await context.Designers.FirstOrDefaultAsync();
            if (designer == null) throw new Exception("Chưa có designer để tạo warehouse");

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
