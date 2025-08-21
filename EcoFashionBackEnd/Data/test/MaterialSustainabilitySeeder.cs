using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class MaterialSustainabilitySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.MaterialSustainabilities.Any())
                return;

            // Lấy tất cả materials
            var materials = await context.Materials.ToListAsync();
            if (!materials.Any())
                throw new Exception("No materials found. Please run MaterialSeeder first.");

            var materialSustainabilities = new List<MaterialSustainability>();

            foreach (var material in materials)
            {
                // Carbon Footprint (CriterionId = 1)
                if (material.CarbonFootprint.HasValue)
                {
                    materialSustainabilities.Add(new MaterialSustainability
                    {
                        MaterialId = material.MaterialId,
                        CriterionId = 1,
                        Value = material.CarbonFootprint.Value
                    });
                }

                // Water Usage (CriterionId = 2)
                if (material.WaterUsage.HasValue)
                {
                    materialSustainabilities.Add(new MaterialSustainability
                    {
                        MaterialId = material.MaterialId,
                        CriterionId = 2,
                        Value = material.WaterUsage.Value
                    });
                }

                // Waste Diverted (CriterionId = 3)
                if (material.WasteDiverted.HasValue)
                {
                    materialSustainabilities.Add(new MaterialSustainability
                    {
                        MaterialId = material.MaterialId,
                        CriterionId = 3,
                        Value = material.WasteDiverted.Value
                    });
                }

                // Organic Certification (CriterionId = 4)
                // Check if material has any recognized sustainability certification
                var hasOrganicCert = HasRecognizedCertification(material.CertificationDetails);
                materialSustainabilities.Add(new MaterialSustainability
                {
                    MaterialId = material.MaterialId,
                    CriterionId = 4,
                    Value = hasOrganicCert ? 100m : 0m  // Use 100 scale to match business logic
                });

                // Transport (CriterionId = 5) - Calculated from TransportDistance and TransportMethod
                // Transport score is calculated dynamically in SustainabilityService
                // No need to store transport data in MaterialSustainability table
                // Transport will be calculated using CalculateTransportScore method
            }

            await context.MaterialSustainabilities.AddRangeAsync(materialSustainabilities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if certification details contain any recognized sustainability certification
        /// This matches the same logic used in SustainabilityService.HasOrganicCertification()
        /// </summary>
        private static bool HasRecognizedCertification(string? certificationDetails)
        {
            if (string.IsNullOrWhiteSpace(certificationDetails))
                return false;

            var details = certificationDetails.ToUpperInvariant();
            
            // Tier 1: Comprehensive sustainability standards
            if (details.Contains("GOTS") || 
                details.Contains("CRADLE TO CRADLE") || 
                details.Contains("USDA ORGANIC") || 
                details.Contains("BLUESIGN"))
                return true;

            // Tier 2: High-value specialized standards
            if (details.Contains("OCS") || 
                details.Contains("EU ECOLABEL") || 
                details.Contains("FAIRTRADE") || 
                details.Contains("BCI") || 
                details.Contains("BETTER COTTON") ||
                details.Contains("OEKO-TEX") ||  // Matches "OEKO-TEX Standard 100", etc.
                details.Contains("RWS") || 
                details.Contains("ECO PASSPORT"))
                return true;

            // Tier 3: Material-specific recycling standards
            if (details.Contains("GRS") || 
                details.Contains("RCS") || 
                details.Contains("RECYCLED CLAIM"))
                return true;

            return false;
        }
    }
}
