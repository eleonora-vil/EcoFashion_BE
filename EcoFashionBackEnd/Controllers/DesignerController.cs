using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Dtos;

[Route("api/[controller]")]
[ApiController]
public class DesignerController : ControllerBase
{
    private readonly DesignerService _designerService;

    public DesignerController(DesignerService designerService)
    {
        _designerService = designerService;
    }

    // Landing Pages APIs - Public endpoints cho frontend showcase

    /// <summary>
    /// Get all designers for public listing/directory (chỉ thông tin công khai)
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicDesigners([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _designerService.GetPublicDesigners(page, pageSize);
        return Ok(ApiResult<List<DesignerSummaryModel>>.Succeed(result));
    }

    /// <summary>
    /// Get designer public profile for landing page (không cần authentication)
    /// </summary>
    [HttpGet("public/{id}")]
    public async Task<IActionResult> GetDesignerPublicProfile(Guid id)
    {
        var designer = await _designerService.GetDesignerPublicProfile(id);
        if (designer == null)
        {
            return NotFound(ApiResult<DesignerPublicModel>.Fail("Không tìm thấy nhà thiết kế."));
        }
        return Ok(ApiResult<DesignerPublicModel>.Succeed(designer));
    }

    /// <summary>
    /// Search designers by keyword for public directory
    /// </summary>
    [HttpGet("public/search")]
    public async Task<IActionResult> SearchPublicDesigners([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var searchResults = await _designerService.SearchPublicDesigners(keyword, page, pageSize);
        return Ok(ApiResult<List<DesignerSummaryModel>>.Succeed(searchResults));
    }

    /// <summary>
    /// Get featured/top designers for homepage
    /// </summary>
    [HttpGet("public/featured")]
    public async Task<IActionResult> GetFeaturedDesigners([FromQuery] int count = 6)
    {
        var featuredDesigners = await _designerService.GetFeaturedDesigners(count);
        return Ok(ApiResult<List<DesignerSummaryModel>>.Succeed(featuredDesigners));
    }

    // Admin & Management APIs

    /// <summary>
    /// Admin: Get all designers (full details)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllDesigners()
    {
        var result = await _designerService.GetAllDesigners();
        return Ok(ApiResult<GetDesignersResponse>.Succeed(new GetDesignersResponse
        {
            Designers = result
        }));
    }

    /// <summary>
    /// Admin: Create designer
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateDesigner([FromBody] CreateDesignerRequest request)
    {
        var designerId = await _designerService.CreateDesigner(request);
        return CreatedAtAction(nameof(GetDesignerById), new { id = designerId }, ApiResult<Guid>.Succeed(designerId));
    }

    /// <summary>
    /// Admin: Get designer full details
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetDesignerById(Guid id)
    {
        var designer = await _designerService.GetDesignerById(id);
        if (designer == null)
        {
            return NotFound(ApiResult<DesignerDetailResponse>.Fail("Không tìm thấy nhà thiết kế."));
        }
        return Ok(ApiResult<DesignerDetailResponse>.Succeed(new DesignerDetailResponse
        {
            Designer = designer
        }));
    }

    /// <summary>
    /// Get designer by user ID (for any authenticated user)
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetDesignerByUserId(int userId)
    {
        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null)
        {
            return NotFound(ApiResult<DesignerModel>.Fail("Không tìm thấy thông tin nhà thiết kế cho người dùng này."));
        }
        return Ok(ApiResult<DesignerModel>.Succeed(designer));
    }

    /// <summary>
    /// Admin: Update designer
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateDesigner(Guid id, [FromBody] UpdateDesignerRequest request)
    {
        var isUpdated = await _designerService.UpdateDesigner(id, request);
        if (!isUpdated)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy nhà thiết kế để cập nhật."));
        }
        return Ok(ApiResult<object>.Succeed("Cập nhật thông tin nhà thiết kế thành công."));
    }

    /// <summary>
    /// Admin: Delete designer
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteDesigner(Guid id)
    {
        var isDeleted = await _designerService.DeleteDesigner(id);
        if (!isDeleted)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy nhà thiết kế để xóa."));
        }
        return Ok(ApiResult<object>.Succeed("Xóa nhà thiết kế thành công."));
    }

    /// <summary>
    /// Admin: Filter designers
    /// </summary>
    [HttpGet("Filter")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> FilterDesigners(
        [FromQuery] string? designerName,
        [FromQuery] string? email,
        [FromQuery] string? phoneNumber,
        [FromQuery] string? status)
    {
        var filteredDesigners = await _designerService.FilterDesigners(designerName, email, phoneNumber, status);
        return Ok(ApiResult<GetDesignersResponse>.Succeed(new GetDesignersResponse
        {
            Designers = filteredDesigners
        }));
    }

    /// <summary>
    /// Admin: Search designers
    /// </summary>
    [HttpGet("Search")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SearchDesigners([FromQuery] string? keyword)
    {
        var searchResults = await _designerService.SearchDesigners(keyword);
        return Ok(ApiResult<GetDesignersResponse>.Succeed(new GetDesignersResponse
        {
            Designers = searchResults
        }));
    }

    // Designer User APIs

    /// <summary>
    /// Designer: Get my profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyDesignerProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<DesignerModel>.Fail("Không thể xác định người dùng."));
        }

        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null)
        {
            return NotFound(ApiResult<DesignerModel>.Fail("Bạn chưa có profile Designer. Vui lòng liên hệ admin để tạo profile."));
        }

        return Ok(ApiResult<DesignerModel>.Succeed(designer));
    }

    /// <summary>
    /// Designer: Update my profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyDesignerProfile([FromBody] UpdateDesignerRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
        }

        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null)
        {
            return NotFound(ApiResult<object>.Fail("Bạn chưa có profile Designer."));
        }

        var isUpdated = await _designerService.UpdateDesigner(designer.DesignerId, request);
        if (!isUpdated)
        {
            return BadRequest(ApiResult<object>.Fail("Không thể cập nhật profile."));
        }

        return Ok(ApiResult<object>.Succeed("Cập nhật profile thành công."));
    }



    // Supplier Connection APIs

    /// <summary>
    /// Designer: Follow a supplier
    /// </summary>
    [Authorize]
    [HttpPost("follow/{supplierId}")]
    public async Task<IActionResult> ConnectWithSupplier(Guid supplierId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
        }

        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null) 
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy thông tin nhà thiết kế cho người dùng này."));
        }

        var result = await _designerService.ConnectWithSupplier(designer.DesignerId, supplierId);

        if (result == null)
        {
            return BadRequest(ApiResult<FollowedSupplierResponse>.Fail("Không thể kết nối hoặc nhà cung cấp đã được theo dõi."));
        }

        return Ok(ApiResult<FollowedSupplierResponse>.Succeed(result));
    }

    /// <summary>
    /// Designer: Get followed suppliers
    /// </summary>
    [Authorize]
    [HttpGet("follow")]
    public async Task<IActionResult> GetFollowedSuppliers()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
        }

        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy thông tin nhà thiết kế cho người dùng này."));
        }

        var followedSuppliers = await _designerService.GetFollowedSuppliers(designer.DesignerId);
        return Ok(ApiResult<GetSuppliersResponse>.Succeed(new GetSuppliersResponse
        {
            Suppliers = followedSuppliers
        }));
    }

    /// <summary>
    /// Designer: Unfollow a supplier
    /// </summary>
    [Authorize]
    [HttpDelete("follow/{supplierId}")]
    public async Task<IActionResult> RemoveFollowedSupplier(Guid supplierId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
        }

        var designer = await _designerService.GetDesignerByUserId(userId);
        if (designer == null)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy thông tin nhà thiết kế cho người dùng này."));
        }

        var isDeleted = await _designerService.RemoveFollowedSupplier(designer.DesignerId, supplierId);

        if (isDeleted)
        {
            return Ok(ApiResult<object>.Succeed("Xóa liên kết thành công.")); 
        }
        else
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy liên kết giữa nhà thiết kế và nhà cung cấp này."));
        }
    }
}