using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Dtos;

[Route("api/[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly SupplierService _supplierService;

    public SupplierController(SupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    // Landing Pages APIs - Public endpoints cho frontend showcase

    /// <summary>
    /// Get all suppliers for public listing/directory (chỉ thông tin công khai)
    /// </summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicSuppliers([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _supplierService.GetPublicSuppliers(page, pageSize);
        return Ok(ApiResult<List<SupplierSummaryModel>>.Succeed(result));
    }

    /// <summary>
    /// Get supplier public profile for landing page (không cần authentication)
    /// </summary>
    [HttpGet("public/{id}")]
    public async Task<IActionResult> GetSupplierPublicProfile(Guid id)
    {
        var supplier = await _supplierService.GetSupplierPublicProfile(id);
        if (supplier == null)
        {
            return NotFound(ApiResult<SupplierPublicModel>.Fail("Không tìm thấy nhà cung cấp."));
        }
        return Ok(ApiResult<SupplierPublicModel>.Succeed(supplier));
    }

    /// <summary>
    /// Search suppliers by keyword for public directory
    /// </summary>
    [HttpGet("public/search")]
    public async Task<IActionResult> SearchPublicSuppliers([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var searchResults = await _supplierService.SearchPublicSuppliers(keyword, page, pageSize);
        return Ok(ApiResult<List<SupplierSummaryModel>>.Succeed(searchResults));
    }

    /// <summary>
    /// Get featured/top suppliers for homepage
    /// </summary>
    [HttpGet("public/featured")]
    public async Task<IActionResult> GetFeaturedSuppliers([FromQuery] int count = 6)
    {
        var featuredSuppliers = await _supplierService.GetFeaturedSuppliers(count);
        return Ok(ApiResult<List<SupplierSummaryModel>>.Succeed(featuredSuppliers));
    }

    // Admin & Management APIs

    /// <summary>
    /// Admin: Get all suppliers (full details)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllSuppliers()
    {
        var result = await _supplierService.GetAllSuppliers();
        return Ok(ApiResult<GetSuppliersResponse>.Succeed(new GetSuppliersResponse
        {
            Suppliers = result
        }));
    }

    /// <summary>
    /// Admin: Create supplier
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplierId = await _supplierService.CreateSupplier(request);
        return CreatedAtAction(nameof(GetSupplierById), new { id = supplierId }, ApiResult<Guid>.Succeed(supplierId));
    }

    /// <summary>
    /// Admin: Get supplier full details
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetSupplierById(Guid id)
    {
        var supplier = await _supplierService.GetSupplierById(id);
        if (supplier == null)
        {
            return NotFound(ApiResult<SupplierDetailResponse>.Fail("Không tìm thấy nhà cung cấp."));
        }
        return Ok(ApiResult<SupplierDetailResponse>.Succeed(new SupplierDetailResponse
        {
            Supplier = supplier
        }));
    }

    /// <summary>
    /// Get supplier by user ID (for any authenticated user)
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetSupplierByUserId(int userId)
    {
        var supplier = await _supplierService.GetSupplierByUserId(userId);
        if (supplier == null)
        {
            return NotFound(ApiResult<SupplierModel>.Fail("Không tìm thấy thông tin nhà cung cấp cho người dùng này."));
        }
        return Ok(ApiResult<SupplierModel>.Succeed(supplier));
    }

    /// <summary>
    /// Admin: Update supplier
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequest request)
    {
        var isUpdated = await _supplierService.UpdateSupplier(id, request);
        if (!isUpdated)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy nhà cung cấp để cập nhật."));
        }
        return Ok(ApiResult<object>.Succeed("Cập nhật thông tin nhà cung cấp thành công."));
    }

    /// <summary>
    /// Admin: Delete supplier
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        var isDeleted = await _supplierService.DeleteSupplier(id);
        if (!isDeleted)
        {
            return NotFound(ApiResult<object>.Fail("Không tìm thấy nhà cung cấp để xóa."));
        }
        return Ok(ApiResult<object>.Succeed("Xóa nhà cung cấp thành công."));
    }

    /// <summary>
    /// Admin: Filter suppliers
    /// </summary>
    [HttpGet("Filter")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> FilterSuppliers(
        [FromQuery] string? supplierName,
        [FromQuery] string? email,
        [FromQuery] string? phoneNumber,
        [FromQuery] string? status)
    {
        var filteredSuppliers = await _supplierService.FilterSuppliers(supplierName, email, phoneNumber, status);
        return Ok(ApiResult<GetSuppliersResponse>.Succeed(new GetSuppliersResponse
        {
            Suppliers = filteredSuppliers
        }));
    }

    /// <summary>
    /// Admin: Search suppliers
    /// </summary>
    [HttpGet("Search")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SearchSuppliers([FromQuery] string? keyword)
    {
        var searchResults = await _supplierService.SearchSuppliers(keyword);
        return Ok(ApiResult<GetSuppliersResponse>.Succeed(new GetSuppliersResponse
        {
            Suppliers = searchResults
        }));
    }

    // Supplier User APIs

    /// <summary>
    /// Supplier: Get my profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetMySupplierProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<SupplierModel>.Fail("Không thể xác định người dùng."));
        }

        var supplier = await _supplierService.GetSupplierByUserId(userId);
        if (supplier == null)
        {
            return NotFound(ApiResult<SupplierModel>.Fail("Bạn chưa có profile Supplier. Vui lòng liên hệ admin để tạo profile."));
        }

        return Ok(ApiResult<SupplierModel>.Succeed(supplier));
    }

    /// <summary>
    /// Supplier: Update my profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMySupplierProfile([FromBody] UpdateSupplierRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
        }

        var supplier = await _supplierService.GetSupplierByUserId(userId);
        if (supplier == null)
        {
            return NotFound(ApiResult<object>.Fail("Bạn chưa có profile Supplier."));
        }

        var isUpdated = await _supplierService.UpdateSupplier(supplier.SupplierId, request);
        if (!isUpdated)
        {
            return BadRequest(ApiResult<object>.Fail("Không thể cập nhật profile."));
        }

        return Ok(ApiResult<object>.Succeed("Cập nhật profile thành công."));
    }
}
