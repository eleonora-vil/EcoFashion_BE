using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class MaterialTypeBenchmarkSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.MaterialTypesBenchmarks.Any())
                return;

            // Get all material types and criteria
            var materialTypes = await context.MaterialTypes.ToListAsync();
            var criteria = await context.SustainabilityCriterias.ToListAsync();

            if (!materialTypes.Any() || !criteria.Any())
                throw new Exception("MaterialTypes and SustainabilityCriteria must be seeded first.");

            var benchmarks = new List<MaterialTypeBenchmark>();

            foreach (var materialType in materialTypes)
            {
                foreach (var criterion in criteria)
                {
                    decimal benchmarkValue = GetBenchmarkValue(materialType.TypeId, criterion.CriterionId);
                    benchmarks.Add(new MaterialTypeBenchmark
                    {
                        TypeId = materialType.TypeId,
                        CriteriaId = criterion.CriterionId,
                        Value = benchmarkValue
                    });
                }
            }

            await context.MaterialTypesBenchmarks.AddRangeAsync(benchmarks);
            await context.SaveChangesAsync();
        }

        private static decimal GetBenchmarkValue(int typeId, int criterionId)
        {
            // Map based on material type name and criterion name
            var materialTypeName = GetMaterialTypeName(typeId);
            var criterionName = GetCriterionName(criterionId);

            return (materialTypeName, criterionName) switch
            {
                // Organic Cotton benchmarks (Source: Higg Index MSI, Textile Exchange)
                ("Organic Cotton", "Carbon Footprint") => 1.2m, // kg CO2e/mét (converted from kg)
                ("Organic Cotton", "Water Usage") => 35m, // L/mét (converted from liters/kg)
                ("Organic Cotton", "Waste Diverted") => 85m, // Industry standard for organic cotton
                ("Organic Cotton", "Organic Certification") => 1m, // GOTS standard
                ("Organic Cotton", "Transport") => 0m, // Transport calculated dynamically

                // Recycled Cotton benchmarks (Source: Higg Index MSI, GRS Standard)
                ("Recycled Cotton", "Carbon Footprint") => 1.0m, // kg CO2e/mét (converted from kg)
                ("Recycled Cotton", "Water Usage") => 20m, // L/mét (converted from liters/kg)
                ("Recycled Cotton", "Waste Diverted") => 90m, // GRS standard for recycled materials
                ("Recycled Cotton", "Organic Certification") => 0m, // Recycled cotton not organic
                ("Recycled Cotton", "Transport") => 0m, // Transport calculated dynamically

                // Hemp benchmarks (Source: Higg Index MSI, Textile Exchange)
                ("Hemp", "Carbon Footprint") => 2.2m, // Higg MSI: Hemp 2.2 kg CO2e/kg
                ("Hemp", "Water Usage") => 75m, // Higg MSI: Hemp 75 liters/kg
                ("Hemp", "Waste Diverted") => 85m, // Industry standard for hemp
                ("Hemp", "Organic Certification") => 0m, // Hemp not always organic
                ("Hemp", "Transport") => 0m, // Transport calculated dynamically

                // Recycled Polyester benchmarks (Source: Higg Index MSI, GRS Standard)
                ("Recycled Polyester", "Carbon Footprint") => 1.6m, // Higg MSI: Recycled polyester 1.6 kg CO2e/kg
                ("Recycled Polyester", "Water Usage") => 50m, // Higg MSI: Recycled polyester 50 liters/kg
                ("Recycled Polyester", "Waste Diverted") => 88m, // GRS standard for recycled materials
                ("Recycled Polyester", "Organic Certification") => 0m, // Recycled polyester not organic
                ("Recycled Polyester", "Transport") => 0m, // Transport calculated dynamically

                // Bamboo benchmarks (Source: Higg Index MSI, EU Ecolabel)
                ("Bamboo", "Carbon Footprint") => 2.8m, // Higg MSI: Bamboo viscose 2.8 kg CO2e/kg
                ("Bamboo", "Water Usage") => 95m, // Higg MSI: Bamboo viscose 95 liters/kg
                ("Bamboo", "Waste Diverted") => 82m, // EU Ecolabel standard
                ("Bamboo", "Organic Certification") => 0m, // Bamboo not always organic
                ("Bamboo", "Transport") => 0m, // Transport calculated dynamically

                // Tencel benchmarks (Source: Higg Index MSI, EU Ecolabel)
                ("Tencel", "Carbon Footprint") => 2.1m, // Higg MSI: Tencel 2.1 kg CO2e/kg
                ("Tencel", "Water Usage") => 70m, // Higg MSI: Tencel 70 liters/kg
                ("Tencel", "Waste Diverted") => 85m, // EU Ecolabel standard
                ("Tencel", "Organic Certification") => 0m, // Tencel not organic
                ("Tencel", "Transport") => 0m, // Transport calculated dynamically

                // Recycled Wool benchmarks (Source: Higg Index MSI, RWS Standard)
                ("Recycled Wool", "Carbon Footprint") => 1.9m, // Higg MSI: Recycled wool 1.9 kg CO2e/kg
                ("Recycled Wool", "Water Usage") => 65m, // Higg MSI: Recycled wool 65 liters/kg
                ("Recycled Wool", "Waste Diverted") => 92m, // RWS standard for recycled materials
                ("Recycled Wool", "Organic Certification") => 0m, // Recycled wool not organic
                ("Recycled Wool", "Transport") => 0m, // Transport calculated dynamically

                // Organic Silk benchmarks (Source: Higg Index MSI, GOTS Standard)
                ("Organic Silk", "Carbon Footprint") => 2.4m, // Higg MSI: Silk 2.4 kg CO2e/kg
                ("Organic Silk", "Water Usage") => 80m, // Higg MSI: Silk 80 liters/kg
                ("Organic Silk", "Waste Diverted") => 78m, // GOTS standard for organic silk
                ("Organic Silk", "Organic Certification") => 1m, // GOTS standard
                ("Organic Silk", "Transport") => 0m, // Transport calculated dynamically

                // Recycled Nylon benchmarks (Source: Higg Index MSI, GRS Standard)
                ("Recycled Nylon", "Carbon Footprint") => 1.4m, // Higg MSI: Recycled nylon 1.4 kg CO2e/kg
                ("Recycled Nylon", "Water Usage") => 45m, // Higg MSI: Recycled nylon 45 liters/kg
                ("Recycled Nylon", "Waste Diverted") => 85m, // GRS standard for recycled materials
                ("Recycled Nylon", "Organic Certification") => 0m, // Recycled nylon not organic
                ("Recycled Nylon", "Transport") => 0m, // Transport calculated dynamically

                // Organic Linen benchmarks (Source: Higg Index MSI, GOTS Standard)
                ("Organic Linen", "Carbon Footprint") => 2.8m, // Higg MSI: Linen 2.8 kg CO2e/kg
                ("Organic Linen", "Water Usage") => 90m, // Higg MSI: Linen 90 liters/kg
                ("Organic Linen", "Waste Diverted") => 82m, // GOTS standard for organic linen
                ("Organic Linen", "Organic Certification") => 1m, // GOTS standard
                ("Organic Linen", "Transport") => 0m, // Transport calculated dynamically

                // Recycled Denim benchmarks (Source: Higg Index MSI, GRS Standard)
                ("Recycled Denim", "Carbon Footprint") => 1.3m, // Higg MSI: Recycled denim 1.3 kg CO2e/kg
                ("Recycled Denim", "Water Usage") => 40m, // Higg MSI: Recycled denim 40 liters/kg
                ("Recycled Denim", "Waste Diverted") => 92m, // GRS standard for recycled materials
                ("Recycled Denim", "Organic Certification") => 0m, // Recycled denim not organic
                ("Recycled Denim", "Transport") => 0m, // Transport calculated dynamically

                // Organic Alpaca benchmarks (Source: Higg Index MSI, RWS Standard)
                ("Organic Alpaca", "Carbon Footprint") => 2.1m, // Higg MSI: Alpaca 2.1 kg CO2e/kg
                ("Organic Alpaca", "Water Usage") => 70m, // Higg MSI: Alpaca 70 liters/kg
                ("Organic Alpaca", "Waste Diverted") => 80m, // RWS standard for organic alpaca
                ("Organic Alpaca", "Organic Certification") => 1m, // RWS standard
                ("Organic Alpaca", "Transport") => 0m, // Transport calculated dynamically

                _ => 0m // Default value
            };
        }

        private static string GetMaterialTypeName(int typeId)
        {
            return typeId switch
            {
                1 => "Organic Cotton",
                2 => "Recycled Cotton",
                3 => "Hemp",
                4 => "Recycled Polyester",
                5 => "Bamboo",
                6 => "Tencel",
                7 => "Recycled Wool",
                8 => "Organic Silk",
                9 => "Recycled Nylon",
                10 => "Organic Linen",
                11 => "Recycled Denim",
                12 => "Organic Alpaca",
                _ => "Unknown"
            };
        }

        private static string GetCriterionName(int criterionId)
        {
            return criterionId switch
            {
                1 => "Carbon Footprint",
                2 => "Water Usage",
                3 => "Waste Diverted",
                4 => "Organic Certification",
                5 => "Transport",
                _ => "Unknown"
            };
        }
    }
}
