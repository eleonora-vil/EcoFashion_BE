using AutoMapper;
using Azure;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.DesignShow;
using EcoFashionBackEnd.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EcoFashionBackEnd.Controllers;



//[Authorize]
[Route("api/Design")]
[ApiController]
public class DesignController : ControllerBase
{
    private readonly DesignService _designService;
    private readonly DesignerService _designerService;

    private readonly IMapper _mapper;

    public DesignController(DesignService designService, DesignerService designerService,IMapper mapper)
    {
        _designService = designService;
        _designerService = designerService;
        _mapper = mapper;
    }

    [HttpGet("designs-with-products")]
    public async Task<IActionResult> GetDesignsWithProducts()
    {
        var result = await _designService.GetDesignsWithProductsAsync();
        return Ok(ApiResult<List<DesignWithProductInfoDto>>.Succeed(result));
    }

    [HttpGet("{designId}/designer/{designerId}")]
    public async Task<IActionResult> GetDesignDetailWithProducts(int designId, Guid designerId)
    {
        try
        {
            var result = await _designService.GetDesignDetailWithProductsAsync(designId, designerId);
            return Ok(ApiResult<DesignDetailDto>.Succeed(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<DesignDetailDto>.Fail(ex.Message));
        }
    }

    [HttpGet("designer/{designerId}")]
    public async Task<IActionResult> GetDesignsByDesigner(Guid designerId)
    {
        try
        {
            // Kiểm tra tham số đầu vào
            if (designerId == Guid.Empty)
            {
                return BadRequest(ApiResult<List<DesignSummaryDto>>.Fail("DesignerId không được để trống."));
            }

            // Gọi service lấy danh sách design có product cho designer đó
            var designs = await _designService.GetDesignsWithProductsByDesignerAsync(designerId);

            // Nếu không tìm thấy design nào
            if (designs == null || designs.Count == 0)
            {
                return NotFound(ApiResult<List<DesignSummaryDto>>.Fail("Không tìm thấy thiết kế nào cho designer này."));
            }

            // Trả về kết quả thành công với data
            return Ok(ApiResult<List<DesignSummaryDto>>.Succeed(designs));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<DesignSummaryDto>>.Fail("Có lỗi xảy ra trong quá trình xử lý."));
        }
    }

    [HttpGet("GetDesignsWithProductsPagination")]
    public async Task<IActionResult> GetAllDesignPagination([FromQuery] int page, [FromQuery] int pageSize)
    {
        var result = await _designService.GetDesignsWithProductsPaginationAsync(page, pageSize);
        return Ok(ApiResult<List<DesignWithProductInfoDto>>.Succeed(result));
    }

    [HttpGet("GetAllPagination-by-designer/{designerId}")]
    public async Task<IActionResult> GetAllDesignsByDesignerIdPagination(Guid designerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _designService.GetDesignsWithDesignerPaginationAsync(designerId, page, pageSize);

        if (result == null || !result.Any())
            return NotFound(ApiResult<List<DesignWithProductInfoDto>>.Fail("Không tìm thấy thiết kế nào cho nhà thiết kế này."));

        return Ok(ApiResult<List<DesignWithProductInfoDto>>.Succeed(result));
    }

    [HttpGet("design-variant/{designerId}")]
    public async Task<IActionResult> GetDesignsWithoutProductsByDesignerId(Guid designerId)
    {
        try
        {
            // Kiểm tra tham số đầu vào
            if (designerId == Guid.Empty)
            {
                return BadRequest(ApiResult<List<DesignSummaryDto>>.Fail("DesignerId không được để trống."));
            }

            // Gọi service lấy danh sách design có product cho designer đó
            var designs = await _designService.GetDesignsWithoutProductsByDesignerIdAsync(designerId);

            // Nếu không tìm thấy design nào
            if (designs == null || designs.Count == 0)
            {
                return NotFound(ApiResult<List<DesignSummaryDto>>.Fail("Không tìm thấy thiết kế nào cho designer này."));
            }

            // Trả về kết quả thành công với data
            return Ok(ApiResult<List<DesignSummaryDto>>.Succeed(designs));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<DesignSummaryDto>>.Fail("Có lỗi xảy ra trong quá trình xử lý."));
        }
    }

    [HttpGet("designs-with-products/{designerId}")]
    public async Task<IActionResult> GetDesignsWithProductsAnDesignerId(Guid designerId)
    {
        try
        {
            // Kiểm tra tham số đầu vào
            if (designerId == Guid.Empty)
            {
                return BadRequest(ApiResult<List<DesignSummaryDto>>.Fail("DesignerId không được để trống."));
            }

            // Gọi service lấy danh sách design có product cho designer đó
            var designs = await _designService.GetDesignsWithProductsAndDesignerIdAsync(designerId);

            // Nếu không tìm thấy design nào
            if (designs == null || designs.Count == 0)
            {
                return NotFound(ApiResult<List<DesignWithProductInfoDto>>.Fail("Không tìm thấy thiết kế nào cho designer này."));
            }

            // Trả về kết quả thành công với data
            return Ok(ApiResult<List<DesignWithProductInfoDto>>.Succeed(designs));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<DesignSummaryDto>>.Fail("Có lỗi xảy ra trong quá trình xử lý."));
        }
    }

    [HttpGet("designProductDetails/{designId}/{designerId}")]
    public async Task<IActionResult> GetDesignWithProductsDetail(int designId, Guid designerId)
    {
        try
        {
            var result = await _designService.GetProductsByDesignAsync(designId, designerId);
            return Ok(ApiResult<List<ProductDto>>.Succeed(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<List<ProductDto>>.Fail(ex.Message));
        }
    }
    [HttpPut("update-basic-info")]
    public async Task<IActionResult> UpdateProductBasicInfo([FromForm] UpdateProductDto input)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<bool>.Fail("Không thể xác định người dùng."));
        }

        var designerId = await _designerService.GetDesignerIdByUserId(userId);
        if (designerId == Guid.Empty)
        {
            return BadRequest(ApiResult<bool>.Fail("Không tìm thấy Designer tương ứng."));
        }

        var success = await _designService.UpdateProductBasicInfoAsync(input, (Guid)designerId);

        if (!success)
        {
            return BadRequest(ApiResult<object>.Fail("Cập nhật thất bại. Thiết kế không tồn tại hoặc bạn không có quyền."));
        }

        return Ok(ApiResult<object>.Succeed("Cập nhật thành công."));
    }




    //[HttpGet("GetAll")]
    //public async Task<IActionResult> GetAllDesigns()
    //{
    //    var designs = await _designService.GetAllDesigns();
    //    //return Ok(ApiResult<IEnumerable<DesignModel>>.Succeed(designs));
    //    var response = _mapper.Map<IEnumerable<DesignDetailResponse>>(designs);
    //    return Ok(ApiResult<IEnumerable<DesignDetailResponse>>.Succeed(response));
    //}

    //[HttpGet("GetAllPagination")]
    //public async Task<IActionResult> GetAllDesignPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    //{
    //    var designs = await _designService.GetAllDesignsPagination(page, pageSize);
    //    var response = _mapper.Map<IEnumerable<DesignDetailResponse>>(designs);
    //    return Ok(ApiResult<IEnumerable<DesignDetailResponse>>.Succeed(response));
    //}


    //[HttpGet("{id}")]
    //public async Task<IActionResult> GetDesignById(int id)
    //{
    //    var design = await _designService.GetDesignById(id);
    //    if (design == null) return NotFound(ApiResult<DesignModel>.Fail("Không tìm thấy thiết kế."));
    //    return Ok(ApiResult<DesignModel>.Succeed(design)); 

    //}

    //[HttpGet("Detail/{id}")]
    //public async Task<IActionResult> GetDesignDetail(int id)
    //{
    //    var dto = await _designService.GetDesignDetailById(id);
    //    if (dto == null)
    //        return NotFound(ApiResult<DesignDetailResponse>.Fail("Không tìm thấy thiết kế."));

    //    var response = _mapper.Map<DesignDetailResponse>(dto);
    //    return Ok(ApiResult<DesignDetailResponse>.Succeed(response));
    //}

    //[HttpGet("Designs-by-designer/{designerId}")]
    //public async Task<IActionResult> GetAllDesignsByDesignerId(Guid designerId)
    //{
    //    var designs = await _designService.GetAllDesignsByDesignerIdAsync(designerId);

    //    if (designs == null || !designs.Any())
    //        return NotFound(ApiResult<List<DesignDetailDto>>.Fail("Không tìm thấy thiết kế nào cho nhà thiết kế này."));

    //    return Ok(ApiResult<List<DesignDetailDto>>.Succeed(designs));
    //}

    //[HttpGet("GetAllPagination-by-designer/{designerId}")]
    //public async Task<IActionResult> GetAllDesignsByDesignerIdPagination(Guid designerId,[FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    //{
    //    var designs = await _designService.GetAllDesignsByDesingerIdPagination(designerId, page, pageSize);

    //    if (designs == null || !designs.Any())
    //        return NotFound(ApiResult<List<DesignDetailDto>>.Fail("Không tìm thấy thiết kế nào cho nhà thiết kế này."));

    //    var response = _mapper.Map<IEnumerable<DesignDetailResponse>>(designs);
    //    return Ok(ApiResult<IEnumerable<DesignDetailResponse>>.Succeed(response));
    //}



    //[HttpPost("Create")]
    //public async Task<IActionResult> CreateDesign([FromForm] CreateDesignRequest request)
    //{
    //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    //    {
    //        return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
    //    }

    //    var designerId = await _designerService.GetDesignerIdByUserId(userId);
    //    if (!designerId.HasValue)
    //    {
    //        return BadRequest(ApiResult<CreateDesignResponse>.Fail("Người dùng này không phải là nhà thiết kế."));
    //    }

    //    var designId = await _designService.CreateDesign(request, designerId.Value, request.ImageFiles);
    //    var response = new CreateDesignResponse { DesignId = designId };
    //    return CreatedAtAction(nameof(GetDesignById), new { id = response.DesignId }, ApiResult<CreateDesignResponse>.Succeed(response));
    //}

    //[HttpPut("update-dessignVariant-By{id}")]
    //public async Task<IActionResult> UpdateDesign(int id, [FromBody] UpdateDesignRequest request)
    //{
    //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    //    {
    //        return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
    //    }

    //    var designerId = await _designerService.GetDesignerIdByUserId(userId);
    //    if (!designerId.HasValue)
    //    {
    //        return BadRequest(ApiResult<object>.Fail("Người dùng này không phải là nhà thiết kế."));
    //    }

    //    var existingDesign = await _designService.GetDesignById(id);
    //    if (existingDesign == null || existingDesign.DesignerId != designerId.Value)
    //    {
    //        return NotFound(ApiResult<object>.Fail("Không tìm thấy thiết kế hoặc bạn không có quyền cập nhật."));
    //    }

    //    var success = await _designService.UpdateDesignVariants(id, request);
    //    if (success)
    //        return Ok(ApiResult<object>.Succeed("Thiết kế đã được cập nhật."));

    //    return BadRequest(ApiResult<object>.Fail("Cập nhật thiết kế thất bại."));
    //}

    //[HttpDelete("{id}")]
    //public async Task<IActionResult> DeleteDesign(int id)
    //{
    //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    //    {
    //        return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
    //    }

    //    var designerId = await _designerService.GetDesignerIdByUserId(userId);
    //    if (!designerId.HasValue)
    //    {
    //        return BadRequest(ApiResult<object>.Fail("Người dùng này không phải là nhà thiết kế."));
    //    }

    //    var existingDesign = await _designService.GetDesignById(id);
    //    if (existingDesign == null || existingDesign.DesignerId != designerId.Value)
    //    {
    //        return NotFound(ApiResult<object>.Fail("Không tìm thấy thiết kế hoặc bạn không có quyền xóa."));
    //    }

    //    var deleted = await _designService.DeleteDesign(id);
    //    if (deleted)
    //        return Ok(ApiResult<object>.Succeed("Thiết kế đã được xóa."));
    //    return BadRequest(ApiResult<object>.Fail("Xóa thiết kế thất bại."));
    //}

    //[HttpPost("variants")]
    //public async Task<IActionResult> AddVariant([FromBody] CreateDesignVariantRequest request)
    //{
    //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    //    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    //        return Unauthorized(ApiResult<object>.Fail("Không xác định được người dùng."));

    //    try
    //    {
    //        var result = await _designService.AddVariantAndUpdateMaterials(request, userId);
    //        if (result)
    //            return Ok(ApiResult<object>.Succeed("Thêm biến thể thành công."));
    //        return BadRequest(ApiResult<object>.Fail("Thêm biến thể thất bại."));
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ApiResult<object>.Fail($"Lỗi: {ex.Message}"));
    //    }
    //}
}