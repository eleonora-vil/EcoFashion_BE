using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class DesignMaterialSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.DesignsMaterials.AnyAsync()) return;

            var designs = await context.Designs.ToListAsync();
            var materials = await context.Materials.ToListAsync();

            var designMaterials = new List<DesignsMaterial>();

            int materialIndex = 0;

            foreach (var design in designs)
            {
                var numMaterials = Random.Shared.Next(2, 4);
                var usedMaterials = materials.Skip(materialIndex).Take(numMaterials).ToList();

                float totalPercentage = 100f;
                float remaining = totalPercentage;

                foreach (var material in usedMaterials)
                {
                    float percent;
                    if (material == usedMaterials.Last())
                    {
                        percent = remaining; 
                    }
                    else
                    {
                        percent = (float)Math.Round(Random.Shared.NextDouble() * (remaining / 2), 1, MidpointRounding.AwayFromZero);
                        remaining -= percent;
                    }

                    designMaterials.Add(new DesignsMaterial
                    {
                        DesignId = design.DesignId,
                        MaterialId = material.MaterialId,
                        MeterUsed = Random.Shared.Next(1, 10)
                    });
                }

                materialIndex = (materialIndex + 1) % materials.Count; 
            }

            await context.DesignsMaterials.AddRangeAsync(designMaterials);
            await context.SaveChangesAsync();
        }
    }


}