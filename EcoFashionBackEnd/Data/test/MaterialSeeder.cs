using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class MaterialSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Materials.Any())
                return;

            // Lấy Supplier One
            var supplier = await context.Suppliers.FirstOrDefaultAsync();
            if (supplier == null)
                throw new Exception("Supplier not found. Please run SupplierSeeder first.");

            var materials = new List<Material>
            {
                // Supplier One: Material 1 - Organic Cotton
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 1, // Organic Cotton
                    Name = "Premium Organic Cotton Fabric",
                    Description = "High-quality organic cotton fabric suitable for sustainable fashion. GOTS certified with excellent breathability and softness.",
                    RecycledPercentage = 0m, // Organic cotton typically 0% recycled (Source: GOTS standard)
                    QuantityAvailable = 500,
                    PricePerUnit = 85m, // 85.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/organic-cotton-gots.pdf",
                    CarbonFootprint = 0.8m, // kg CO2e/mét (tốt hơn benchmark 1.2)
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 25m, // L/mét (tốt hơn benchmark 35)
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 88m, // Better than benchmark 85
                    WasteDivertedUnit = "%",
                    ProductionCountry = "India",
                    ProductionRegion = "Gujarat",
                    ManufacturingProcess = "Organic farming, mechanical processing",
                    CertificationDetails = "GOTS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 3500m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 2 - Recycled Cotton
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 2, // Recycled Cotton
                    Name = "Eco-Friendly Recycled Cotton Blend",
                    Description = "Sustainable recycled cotton fabric made from post-consumer waste. Reduces landfill waste and water consumption.",
                    RecycledPercentage = 85m, // GRS minimum standard (Source: GRS Standard)
                    QuantityAvailable = 300,
                    PricePerUnit = 72m, // 72.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/recycled-cotton-grs.pdf",
                    CarbonFootprint = 0.7m, // kg CO2e/mét (tốt hơn benchmark 1.0)
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 18m, // L/mét (tốt hơn benchmark 20)
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 92m, // Better than benchmark 90
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Turkey",
                    ProductionRegion = "Istanbul",
                    ManufacturingProcess = "Post-consumer waste processing",
                    CertificationDetails = "GRS, RCS",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
                    TransportDistance = 2800m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 3 - Hemp
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 3, // Hemp
                    Name = "Natural Hemp Fabric",
                    Description = "Durable and sustainable hemp fabric with excellent strength and breathability. Naturally pest-resistant and low water usage.",
                    RecycledPercentage = 0m, // Hemp typically 0% recycled (Source: Textile Exchange)
                    QuantityAvailable = 200,
                    PricePerUnit = 95m, // 95.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/hemp-natural.pdf",
                    CarbonFootprint = 1.1m, // Much better than benchmark 2.2
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 35m, // Much better than benchmark 75
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 96m, // Better than benchmark 85
                    WasteDivertedUnit = "%",
                    ProductionCountry = "China",
                    ProductionRegion = "Yunnan",
                    ManufacturingProcess = "Natural fiber processing",
                    CertificationDetails = "OEKO-TEX, EU ECOLABEL",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(3),
                    TransportDistance = 1200m,
                    TransportMethod = "Land",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 4 - Recycled Polyester
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 4, // Recycled Polyester
                    Name = "Recycled PET Polyester Fabric",
                    Description = "High-performance recycled polyester made from PET bottles. Excellent durability and moisture-wicking properties.",
                    RecycledPercentage = 90m, // GRS minimum standard (Source: GRS Standard)
                    QuantityAvailable = 400,
                    PricePerUnit = 65m, // 65.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/recycled-polyester-grs.pdf",
                    CarbonFootprint = 1.4m, // Better than benchmark 1.6
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 48m, // Better than benchmark 50
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 89m, // Better than benchmark 88
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Taiwan",
                    ProductionRegion = "Taiwan",
                    ManufacturingProcess = "PET bottle recycling",
                    CertificationDetails = "GRS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 4200m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 5 - Bamboo
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 5, // Bamboo Viscose
                    Name = "Bamboo Viscose Fabric",
                    Description = "Soft and silky bamboo viscose fabric with excellent moisture absorption and antibacterial properties.",
                    RecycledPercentage = 0m, // Bamboo typically 0% recycled (Source: EU Ecolabel)
                    QuantityAvailable = 250,
                    PricePerUnit = 78m, // 78.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/bamboo-viscose-oeko.pdf",
                    CarbonFootprint = 1.7m, // Much better than benchmark 2.8
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 42m, // Much better than benchmark 95
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 94m, // Better than benchmark 82
                    WasteDivertedUnit = "%",
                    ProductionCountry = "China",
                    ProductionRegion = "Beijing",
                    ManufacturingProcess = "Bamboo pulp processing",
                    CertificationDetails = "OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
                    TransportDistance = 1800m,
                    TransportMethod = "Land",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 6 - Tencel
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 6, // Tencel
                    Name = "Tencel Lyocell Fabric",
                    Description = "Eco-friendly Tencel fabric made from sustainably sourced wood pulp. Excellent drape and moisture management.",
                    RecycledPercentage = 0m, // Tencel typically 0% recycled (Source: EU Ecolabel)
                    QuantityAvailable = 180,
                    PricePerUnit = 120m, // 120.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/tencel-lyocell-oeko.pdf",
                    CarbonFootprint = 1.9m, // Better than benchmark 2.1
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 65m, // Better than benchmark 70
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 87m, // Better than benchmark 85
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Austria",
                    ProductionRegion = "Vienna",
                    ManufacturingProcess = "Lyocell process",
                    CertificationDetails = "OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 8500m,
                    TransportMethod = "Air",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 7 - Recycled Wool
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 7, // Recycled Wool
                    Name = "Recycled Wool Blend Fabric",
                    Description = "Warm and durable recycled wool fabric made from post-consumer wool garments. Excellent insulation properties.",
                    RecycledPercentage = 88m, // RWS minimum standard (Source: RWS Standard)
                    QuantityAvailable = 120,
                    PricePerUnit = 150m, // 150.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/recycled-wool-rws.pdf",
                    CarbonFootprint = 1.7m, // Better than benchmark 1.9
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 58m, // Better than benchmark 65
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 94m, // Better than benchmark 92
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Italy",
                    ProductionRegion = "Milan",
                    ManufacturingProcess = "Post-consumer wool processing",
                    CertificationDetails = "RWS, OEKO-TEX",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
                    TransportDistance = 9500m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 8 - Organic Silk
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 8, // Organic Silk
                    Name = "Organic Silk Fabric",
                    Description = "Luxurious organic silk fabric with natural sheen and softness. Ethically produced without harming silkworms.",
                    RecycledPercentage = 0m, // Organic silk typically 0% recycled (Source: GOTS Standard)
                    QuantityAvailable = 80,
                    PricePerUnit = 250m, // 250.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/organic-silk-gots.pdf",
                    CarbonFootprint = 2.2m, // Better than benchmark 2.4
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 75m, // Better than benchmark 80
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 80m, // Better than benchmark 78
                    WasteDivertedUnit = "%",
                    ProductionCountry = "India",
                    ProductionRegion = "Mumbai",
                    ManufacturingProcess = "Ahimsa silk production",
                    CertificationDetails = "GOTS, OEKO-TEX",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 3800m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 9 - Recycled Nylon
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 9, // Recycled Nylon
                    Name = "Recycled Nylon Fabric",
                    Description = "High-performance recycled nylon fabric made from fishing nets and industrial waste. Excellent durability and stretch.",
                    RecycledPercentage = 80m, // GRS minimum standard (Source: GRS Standard)
                    QuantityAvailable = 150,
                    PricePerUnit = 95m, // 95.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/recycled-nylon-grs.pdf",
                    CarbonFootprint = 1.2m, // Better than benchmark 1.4
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 40m, // Better than benchmark 45
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 87m, // Better than benchmark 85
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Spain",
                    ProductionRegion = "Barcelona",
                    ManufacturingProcess = "Fishing net recycling",
                    CertificationDetails = "GRS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
                    TransportDistance = 12000m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 10 - Organic Linen
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 10, // Organic Linen
                    Name = "Organic Linen Fabric",
                    Description = "Natural organic linen fabric with excellent breathability and moisture absorption. Perfect for summer clothing.",
                    RecycledPercentage = 0m, // Organic linen typically 0% recycled (Source: GOTS Standard)
                    QuantityAvailable = 100,
                    PricePerUnit = 180m, // 180.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/organic-linen-gots.pdf",
                    CarbonFootprint = 2.5m, // Better than benchmark 2.8
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 85m, // Better than benchmark 90
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 85m, // Better than benchmark 82
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Belgium",
                    ProductionRegion = "Brussels",
                    ManufacturingProcess = "Organic flax processing",
                    CertificationDetails = "GOTS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 10500m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 11 - Recycled Denim
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 11, // Recycled Denim
                    Name = "Recycled Denim Fabric",
                    Description = "Sustainable recycled denim fabric made from post-consumer jeans. Reduces water and energy consumption significantly.",
                    RecycledPercentage = 85m, // GRS minimum standard (Source: GRS Standard)
                    QuantityAvailable = 200,
                    PricePerUnit = 85m, // 85.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/recycled-denim-grs.pdf",
                    CarbonFootprint = 1.1m, // Better than benchmark 1.3
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 35m, // Better than benchmark 40
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 94m, // Better than benchmark 92
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Turkey",
                    ProductionRegion = "Istanbul",
                    ManufacturingProcess = "Post-consumer denim processing",
                    CertificationDetails = "GRS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(1),
                    TransportDistance = 2900m,
                    TransportMethod = "Sea",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },

                // Supplier One: Material 12 - Organic Alpaca
                new Material
                {
                    SupplierId = supplier.SupplierId,
                    TypeId = 12, // Organic Alpaca
                    Name = "Organic Alpaca Wool Fabric",
                    Description = "Luxurious organic alpaca wool fabric with exceptional softness and warmth. Ethically sourced from free-range alpacas.",
                    RecycledPercentage = 0m, // Organic alpaca typically 0% recycled (Source: RWS Standard)
                    QuantityAvailable = 60,
                    PricePerUnit = 350m, // 350.000 VNĐ/mét (đơn vị: nghìn VNĐ)
                    DocumentationUrl = "https://example.com/certificates/organic-alpaca-rws.pdf",
                    CarbonFootprint = 1.9m, // Better than benchmark 2.1
                    CarbonFootprintUnit = "kg CO2e/mét",
                    WaterUsage = 65m, // Better than benchmark 70
                    WaterUsageUnit = "lít/mét",
                    WasteDiverted = 82m, // Better than benchmark 80
                    WasteDivertedUnit = "%",
                    ProductionCountry = "Peru",
                    ProductionRegion = "Lima",
                    ManufacturingProcess = "Free-range alpaca farming",
                    CertificationDetails = "RWS, OEKO-TEX Standard 100",
                    CertificationExpiryDate = DateTime.UtcNow.AddYears(2),
                    TransportDistance = 18500m,
                    TransportMethod = "Air",
                    ApprovalStatus = "Approved",
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                }
            };

            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();
        }
    }
}
