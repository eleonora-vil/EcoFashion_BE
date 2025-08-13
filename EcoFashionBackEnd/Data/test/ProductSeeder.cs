using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class ProductSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            // Giả sử đã có designId, sizeId, variantId trong db
            // Lấy mẫu data từ db cho reference
            var designs = await context.Designs.Take(3).ToListAsync();
            var sizes = await context.Sizes.ToListAsync();
            var variants = await context.DesignsVarients.ToListAsync();
            var warehouses = await context.Warehouses.Where(w => w.WarehouseType == "Product").ToListAsync();

            var products = new List<Product>();
            var productInventories = new List<ProductInventory>();
            var random = new Random();

            int skuCounter = 1;

            foreach (var design in designs)
            {
                foreach (var size in sizes)
                {
                    // Optional: tìm variant tương ứng theo design và size
                    var variant = variants.FirstOrDefault(v => v.DesignId == design.DesignId && v.SizeId == size.SizeId);

                    // Màu sắc mẫu
                    var colors = new[] { "RED", "BLK", "WHT" };

                    foreach (var color in colors)
                    {
                        var sku = $"D{design.DesignId}-S{size.SizeId}-C{color}";
                        var price = design.SalePrice ?? 500000m + random.Next(0, 200000);

                        var product = new Product
                        {
                            DesignId = design.DesignId,
                            VariantId = variant?.Id,
                            SizeId = size.SizeId,
                            ColorCode = color,
                            SKU = sku,
                            Price = price
                        };

                        products.Add(product);
                    }
                }
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Sau khi lưu products, tạo ProductInventory cho mỗi product với kho Product mặc định
            var defaultProductWarehouse = warehouses.FirstOrDefault();
            if (defaultProductWarehouse == null) throw new Exception("Không tìm thấy kho Product mặc định.");

            foreach (var product in products)
            {
                var quantity = random.Next(5, 20); // số lượng tồn kho random

                productInventories.Add(new ProductInventory
                {
                    ProductId = product.ProductId,
                    WarehouseId = defaultProductWarehouse.WarehouseId,
                    QuantityAvailable = quantity,
                    LastUpdated = DateTime.UtcNow
                });
            }

            await context.ProductInventories.AddRangeAsync(productInventories);
            await context.SaveChangesAsync();
        }
    }

}
