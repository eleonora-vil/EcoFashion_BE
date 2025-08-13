using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos.Blog;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Exceptions;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class BlogService
    {
        private readonly IRepository<Blog, int> _blogRepository;
        private readonly IRepository<BlogImage, int> _blogImageRepository;
        private readonly AppDbContext _dbContext;
        private readonly CloudService _cloudService;
        private readonly IMapper _mapper;
        public BlogService(IRepository<Blog, int> blogRepository,  IRepository<BlogImage, int> blogImageRepository, AppDbContext dbContext, CloudService cloudService, IMapper mapper)
        {
            _blogRepository = blogRepository;
            _blogImageRepository = blogImageRepository;
            _dbContext = dbContext;
            _cloudService = cloudService;
            _mapper = mapper;
        }
        public async Task<IEnumerable<BlogDetailDto>> GetAllBlogsAsync()
        {
            var blogs = await _dbContext.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogImages)
                    .ThenInclude(bi => bi.Image)
                .ToListAsync();
            var userIds = blogs.Select(b => b.UserID).Distinct().ToList();
            // Load all designers and suppliers for the relevant users
            var designers = await _dbContext.Designers
                .Where(d => userIds.Contains(d.UserId))
                .ToDictionaryAsync(d => d.UserId);
            var suppliers = await _dbContext.Suppliers
                .Where(s => userIds.Contains(s.UserId))
                .ToDictionaryAsync(d => d.UserId);
            var blogDtos = blogs.Select(blog =>
            {
                var userId = blog.UserID;
                designers.TryGetValue(userId, out var designer);
                suppliers.TryGetValue(userId, out var supplier);

                string displayName = designer?.DesignerName ?? supplier?.SupplierName ?? "Unknown";
                string avatarUrl = designer?.AvatarUrl ?? supplier?.AvatarUrl ?? "default-avatar.png";

                return new BlogDetailDto
                {
                    BlogId = blog.BlogId,
                    UserID = userId,
                    UserName = displayName,
                    AvatarUrl = avatarUrl,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrls = [.. blog.BlogImages.Select(bi => bi.Image.ImageUrl) ?? []],
                    CreatedAt = blog.CreatedAt,
                    LastUpdatedAt = blog.LastUpdatedAt,
                };
            });
            return blogDtos;
        }
        public async Task<BlogDetailDto?> GetByIdAsync(int blogId)
        {
            var blog = await _dbContext.Blogs
                .Include(b => b.User)
                .Include(b => b.BlogImages)
                    .ThenInclude(bi => bi.Image)
                .FirstOrDefaultAsync(b => b.BlogId == blogId);

            if (blog == null)
                return null;

            var userId = blog.UserID;

            // Try to find the user in Designer or Supplier
            var designer = await _dbContext.Designers
                .FirstOrDefaultAsync(d => d.UserId == userId);

            var supplier = await _dbContext.Suppliers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            // Extract name and avatar from available role
            string userName = designer?.DesignerName ?? supplier?.SupplierName ?? "Unknown";
            string avatarUrl = designer?.AvatarUrl ?? supplier?.AvatarUrl ?? "default-avatar.png";

            return new BlogDetailDto
            {
                BlogId = blog.BlogId,
                UserID = userId,
                UserName = userName,
                AvatarUrl = avatarUrl,
                Title = blog.Title,
                Content = blog.Content,
                ImageUrls = [.. blog.BlogImages.Select(bi => bi.Image.ImageUrl) ?? []],
                CreatedAt = blog.CreatedAt,
                LastUpdatedAt = blog.LastUpdatedAt,
            };
        }
        public async Task<int> CreateBlogAsync(CreateBlogRequest request, int userId, List<IFormFile> imageFiles)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Tiêu đề không được để trống.");
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new ArgumentException("Nội dung không được để trống.");
            await CheckUserPermissionAsync(userId);
            var blog = new Blog
            {
                UserID = userId,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
            };
            await _blogRepository.AddAsync(blog);
            await _blogRepository.Commit();

            if (imageFiles?.Any() == true)
            {
                var uploadResults = await _cloudService.UploadImagesAsync(imageFiles);
                var blogImages = new List<BlogImage>();
                foreach (var uploadResult in uploadResults) 
                    if (!string.IsNullOrWhiteSpace(uploadResult?.SecureUrl?.ToString()))
                    {
                        var image = new Image
                        {
                            ImageUrl = uploadResult.SecureUrl.ToString()
                        };
                        _dbContext.Images.Add(image);
                        blogImages.Add(new BlogImage
                        {
                            BlogId = blog.BlogId,
                            Image = image,
                        });
                    }
                    else
                    {
                        Console.WriteLine("Upload failed or returned null SecureUrl.");
                    }
                await _blogImageRepository.AddRangeAsync(blogImages);
            }
            await _blogRepository.Commit();
            return blog.BlogId;
        }
        public async Task<bool> UpdateBlogAsync(int blogId, int userId, UpdateBlogRequest request)
        {
            await CheckUserPermissionAsync(userId);
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null)
                throw new ArgumentException("Bài viết không tồn tại");
            if (blog.UserID != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa blog này.");
            if (!string.IsNullOrWhiteSpace(request.Title))
                blog.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Content))
                blog.Content = request.Content;
            blog.LastUpdatedAt = DateTime.UtcNow;

            _dbContext.Blogs.Update(blog);
            await _blogRepository.Commit();
            return true;
        }
        public async Task<bool> DeleteBlogAsync(int blogId, int userId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null)
                throw new NotFoundException("Blog không tồn tại.");

            if (blog.UserID != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa blog này.");

            _blogRepository.Remove(blogId);
            await _blogRepository.Commit();
            return true;
        }
        private async Task CheckUserPermissionAsync(int userId)
        {
            var isDesigner = await _dbContext.Designers.AnyAsync(d => d.UserId == userId);
            var isSupplier = await _dbContext.Suppliers.AnyAsync(s => s.UserId == userId);
            if (!isDesigner && !isSupplier)
                throw new ArgumentException("Người dùng không có quyền tạo");
        }
        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            // Tìm review theo ID
            var review = await _dbContext.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đánh giá.");
            }

            // Kiểm tra quyền sở hữu
            if (review.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này.");
            }

            // Xóa review
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();

            return true;
        }

    }
}
