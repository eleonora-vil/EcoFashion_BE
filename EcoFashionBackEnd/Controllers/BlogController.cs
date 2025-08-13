using AutoMapper;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos.Blog;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers;

[Route("api/Blog")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly BlogService _blogService;
    public BlogController(IMapper mapper, BlogService blogService)
    {
        _mapper = mapper;
        _blogService = blogService;
    }
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllBlogs()
    {
        var blogs = await _blogService.GetAllBlogsAsync();
        return Ok(ApiResult<IEnumerable<BlogDetailDto?>>.Succeed(blogs));
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlogById(int id)
    {
        var blog = await _blogService.GetByIdAsync(id);
        if (blog == null)
            return NotFound(ApiResult<BlogDetailDto>.Fail("Không tìm thấy bài viết"));
        return Ok(ApiResult<BlogDetailDto>.Succeed(blog));
    }
    [HttpPost("Create")]
    public async Task<IActionResult> CreateBlog([FromForm] CreateBlogRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var blogId = await _blogService.CreateBlogAsync(request, userId, request.ImageFiles);
        return CreatedAtAction(nameof(GetBlogById), new { id = blogId }, ApiResult<object>.Succeed(new { BlogId = blogId }));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlog(int id, [FromBody] UpdateBlogRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var result = await _blogService.UpdateBlogAsync(id, userId, request);
        if (result)
            return Ok(ApiResult<object>.Succeed("Blog đã được cập nhật."));
        return BadRequest("Cập nhật blog thất bại.");
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlog(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var result = await _blogService.DeleteBlogAsync(id, userId);
        if (result)
            return Ok(ApiResult<object>.Succeed("Blog đã được xóa."));
        return BadRequest("Xóa blog thất bại.");
    }
}
