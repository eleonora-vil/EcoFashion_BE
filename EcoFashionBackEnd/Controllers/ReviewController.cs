using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers;

[Route("api/Review")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly ReviewService _reviewService;
    public ReviewController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        return Ok(ApiResult<IEnumerable<ReviewModel>>.Succeed(reviews));
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReviewById(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
            return NotFound("Không tìm thấy đánh giá");
        return Ok(ApiResult<ReviewModel>.Succeed(review));
    }
    [HttpPost("Create")]
    public async Task<IActionResult> CreateReview(CreateReviewRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var reviewId = await _reviewService.CreateReviewAsync(request, userId);
        return CreatedAtAction(nameof(CreateReview), new { id = reviewId }, ApiResult<object>.Succeed(new { ReviewId = reviewId }));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id,[FromForm] UpdateReviewRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var result = await _reviewService.UpdateReviewAsync(id, request, userId);
        if (result)
            return Ok(ApiResult<object>.Succeed("Đánh giá đã được cập nhật"));
        return BadRequest("Cập nhật đánh giá thất bại");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var result = await _reviewService.DeleteReviewAsync(id, userId);
        if (result)
            return Ok(ApiResult<object>.Succeed("Đánh giá đã được xoá"));
        return BadRequest("Xóa đánh giá thất bại");
    }
    [HttpGet("average-score/{id}")]
    public async Task<IActionResult> GetAverageScore(int id, [FromQuery] bool isProduct)
    {
        var score = await _reviewService.GetAverageScoreAsync(id, isProduct);
        return Ok(ApiResult<decimal>.Succeed(score));
    }
    [HttpGet]
    public async Task<IActionResult> GetAllReviewsById([FromQuery] int? productId, [FromQuery] int? materialId)
    {
        if (!productId.HasValue && !materialId.HasValue)
            return BadRequest(ApiResult<object>.Fail("Cần cung cấp ProductId hoặc MaterialId."));

        var reviews = await _reviewService.GetAllReviewsByIdAsync(productId, materialId);

        return Ok(ApiResult<IEnumerable<ReviewModel>>.Succeed(reviews));
    }

}
