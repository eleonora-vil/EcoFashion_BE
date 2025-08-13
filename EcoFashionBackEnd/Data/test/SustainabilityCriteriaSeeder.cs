using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Data.test
{
    public static class SustainabilityCriteriaSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.SustainabilityCriterias.Any())
                return;

            var criteria = new List<SustainabilityCriteria>
            {
                new SustainabilityCriteria
                {
                    Name = "Carbon Footprint",
                    Description = "Lượng khí thải carbon tạo ra trong quá trình sản xuất",
                    Unit = "kg CO2e/mét",
                    Weight = 0.20m, // Equal weight (20%)
                    Thresholds = "0-2:Excellent,2-5:Good,5-8:Average,8+:Needs Improvement",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new SustainabilityCriteria
                {
                    Name = "Water Usage",
                    Description = "Lượng nước sử dụng trong quá trình sản xuất",
                    Unit = "lít/mét",
                    Weight = 0.20m, // Equal weight (20%)
                    Thresholds = "0-50:Excellent,50-100:Good,100-200:Average,200+:Needs Improvement",
                    IsActive = true,
                    DisplayOrder = 2
                },
                new SustainabilityCriteria
                {
                    Name = "Waste Diverted",
                    Description = "Tỷ lệ chất thải được tái chế hoặc xử lý bền vững",
                    Unit = "%",
                    Weight = 0.20m, // Equal weight (20%)
                    Thresholds = "80-100:Excellent,60-80:Good,40-60:Average,0-40:Needs Improvement",
                    IsActive = true,
                    DisplayOrder = 3
                },
                new SustainabilityCriteria
                {
                    Name = "Organic Certification",
                    Description = "Chứng nhận hữu cơ và các tiêu chuẩn bền vững",
                    Unit = "Có/Không",
                    Weight = 0.20m, // Equal weight (20%)
                    Thresholds = "1:Excellent,0:Needs Improvement",
                    IsActive = true,
                    DisplayOrder = 4
                },
                new SustainabilityCriteria
                {
                    Name = "Transport",
                    Description = "Đánh giá tác động môi trường của vận chuyển dựa trên khoảng cách và phương thức",
                    Unit = "%",
                    Weight = 0.20m, // Equal weight with other criteria
                    Thresholds = "80-100:Excellent,60-80:Good,40-60:Average,0-40:Needs Improvement",
                    IsActive = true,
                    DisplayOrder = 5
                }
            };

            await context.SustainabilityCriterias.AddRangeAsync(criteria);
            await context.SaveChangesAsync();
        }
    }
} 