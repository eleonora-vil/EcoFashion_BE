using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Data.test
{
    public static class DesignImageSeeder
    {

        public static async Task SeedAsync(AppDbContext context)
        {
            var url1 = "https://res.cloudinary.com/dnzg0mgoi/image/upload/v1752820137/ic95dsembewnfwgb6jor.jpg";
            var url2 = "https://res.cloudinary.com/dnzg0mgoi/image/upload/v1752820093/au80dylupwlhtkrdyibj.webp";
            var url3 = "https://res.cloudinary.com/dnzg0mgoi/image/upload/v1752820085/bsik9lhysub0hq0kfwwa.webp";
            var url4 = "https://res.cloudinary.com/dnzg0mgoi/image/upload/v1752820075/zotgx2jv300aqhqyg8ch.webp";

            if (await context.DesignImages.AnyAsync() ) return;

            var designs = await context.Designs.OrderBy(d => d.DesignId).ToListAsync();

            // 12 nhóm URL tương ứng 12 thiết kế
            var imageUrls = new List<List<string>>
            {
           
                new() { url1, url2, url3 },
                new() { url1, url3, url4 },
                new() { url1, url2, url4 },
                new() { url2, url3, url4 },
                new() { url3, url1, url2 },
                new() { url2, url4, url1 },
                new() { url4, url3, url2 },
                new() { url4, url1, url3 },
                new() { url3, url2, url1 },
                new() { url2, url1, url3 },
                new() { url1, url4, url2 },
                new() { url3, url4, url1 },

            };

            // Safety check
            if (designs.Count != imageUrls.Count)
                throw new Exception("Số lượng design và nhóm image URL không khớp.");

            var allImageEntities = new List<Image>();
            var allDesignImages = new List<DesignImage>();

            for (int i = 0; i < designs.Count; i++)
            {
                var design = designs[i];
                var urlsForThisDesign = imageUrls[i];

                foreach (var url in urlsForThisDesign)
                {
                    var image = new Image { ImageUrl = url };
                    allImageEntities.Add(image);

                    // Không có ImageId ở đây vì chưa save, nên phải xử lý sau
                    allDesignImages.Add(new DesignImage
                    {
                        Design = design,
                        Image = image
                    });
                }
            }

            // Save all images, EF sẽ gán ImageId tự động
            await context.Images.AddRangeAsync(allImageEntities);
            await context.SaveChangesAsync();

            // Save relation sau khi có ImageId
            await context.DesignImages.AddRangeAsync(allDesignImages);
            await context.SaveChangesAsync();
        }
    }


}
