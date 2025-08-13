using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services;

public class ReviewService
{
    private readonly IRepository<Review, int> _reviewRepository;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    public ReviewService(IRepository<Review, int> reviewRepository, AppDbContext dbContext, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<IEnumerable<ReviewModel>> GetAllReviewsAsync()
    {
        return await _dbContext.Reviews
            .Include(r => r.User) // load user
            .Select(r => new ReviewModel
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                UserName = r.User.Username, // from User table
                MaterialId = r.MaterialId,
                ProductId = r.ProductId,
                Comment = r.Comment,
                RatingScore = r.RatingScore
            })
            .ToListAsync();
    }
    public async Task<ReviewModel?> GetReviewByIdAsync(int reviewId)
    {
        return await _dbContext.Reviews
            .Include(r => r.User) // load User
            .Where(r => r.ReviewId == reviewId)
            .Select(r => new ReviewModel
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                UserName = r.User.Username,
                MaterialId = r.MaterialId,
                ProductId = r.ProductId,
                Comment = r.Comment,
                RatingScore = r.RatingScore
            })
            .FirstOrDefaultAsync();
    }
    public async Task<int> CreateReviewAsync(CreateReviewRequest request, int userId)
    {
        // check do dai comment
        if (!string.IsNullOrEmpty(request.Comment) && request.Comment.Length > 1000)
            throw new ArgumentException("Bình luận không được vượt quá 1000 ký tự.");
        // check rating score
        if (request.RatingScore < 1 || request.RatingScore > 5)
            throw new ArgumentException("Điểm đánh giá phải nằm trong khoảng từ 1 đến 5.");
        if ((request.ProductId.HasValue && request.MaterialId.HasValue) ||
            (!request.ProductId.HasValue && !request.MaterialId.HasValue))
        {
            throw new ArgumentException("Phải chọn hoặc Sản phẩm hoặc Vật liệu, nhưng không được chọn cả hai hoặc bỏ trống cả hai.");
        }
        // Kiểm tra ItemId có tồn tại trong bảng Sản phẩm hoặc Vật liệu hay không
        bool itemExists = await _dbContext.Products.AnyAsync(p => p.ProductId == request.ProductId)
                          || await _dbContext.Materials.AnyAsync(m => m.MaterialId == request.MaterialId);
        if (!itemExists)
            throw new ArgumentException("Sản phẩm không tồn tại.");
        var exists = await _dbContext.Reviews.AnyAsync(r =>
        r.UserId == userId &&
        (r.ProductId == request.ProductId || r.MaterialId == request.MaterialId));

        if (exists)
            throw new ArgumentException("Bạn đã đánh giá mục này rồi.");
        var review = new Review
        {
            UserId = userId,
            MaterialId = request.MaterialId,
            ProductId = request.ProductId,
            Comment = request.Comment,
            RatingScore = request.RatingScore
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        // Return the newly created review with username
        return review.ReviewId;
    }
    public async Task<bool> UpdateReviewAsync(int reviewId, UpdateReviewRequest request, int userId)
    {
        var review = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (review == null)
            throw new KeyNotFoundException("Không tìm thấy đánh giá.");
        if (review.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa đánh giá này.");
        // Kiểm tra comment
        if (request.Comment != null)
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
                throw new ArgumentException("Bình luận không được để trống.");
            if (request.Comment.Length > 1000)
                throw new ArgumentException("Bình luận không được vượt quá 1000 ký tự.");
            review.Comment = request.Comment;
        }

        // Kiểm tra điểm đánh giá
        if (request.RatingScore.HasValue)
        {
            if (request.RatingScore.Value < 1 || request.RatingScore.Value > 5)
                throw new ArgumentException("Điểm đánh giá phải nằm trong khoảng từ 1 đến 5.");
            review.RatingScore = request.RatingScore.Value;
        }

        // Lưu thay đổi
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (review == null)
            throw new KeyNotFoundException("Không tìm thấy đánh giá.");
        if (review.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này.");
        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<decimal> GetAverageScoreAsync(int id, bool isProduct)
    {
        IQueryable<Review> query = _dbContext.Reviews.AsQueryable();

        if (isProduct)
            query = query.Where(r => r.ProductId == id);
        else
            query = query.Where(r => r.MaterialId == id);

        if (!await query.AnyAsync())
            return 0; // No reviews yet

        return await query.AverageAsync(r => r.RatingScore);
    }
    public async Task<IEnumerable<ReviewModel>> GetAllReviewsByIdAsync(int? productId, int? materialId)
    {
        if (!productId.HasValue && !materialId.HasValue)
            throw new ArgumentException("Cần cung cấp ProductId hoặc MaterialId.");

        var query = _dbContext.Reviews
            .Include(r => r.User)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(r => r.ProductId == productId.Value);
        else if (materialId.HasValue)
                query = query.Where(r => r.MaterialId == materialId.Value);
        return await query
            .Select(r => new ReviewModel
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                UserName = r.User.Username,
                ProductId = r.ProductId,
                MaterialId = r.MaterialId,
                Comment = r.Comment,
                RatingScore = r.RatingScore
            })
            .ToListAsync();
    }
}
