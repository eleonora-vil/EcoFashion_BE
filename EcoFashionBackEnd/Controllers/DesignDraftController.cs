using AutoMapper;
using Azure;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Requests.DessignDraft;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.DesignDraft;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Services;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EcoFashionBackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DesignDraftController : ControllerBase
{
    private readonly DesignDraftService _designDraftService;
    private readonly DesignerService _designerService;

    public DesignDraftController(DesignDraftService designDraftService, DesignerService designerService)
    {
        _designDraftService = designDraftService;
        _designerService = designerService;
    }

    [HttpPost("create-draft")]
    public async Task<ActionResult<ApiResult<int>>> CreateDraft([FromForm] DraftDesignCreateRequest request)
    {
        // Lấy userId từ claim
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<int>.Fail("Không thể xác định người dùng."));
        }

        var designerId = await _designerService.GetDesignerIdByUserId(userId);
        if (designerId == Guid.Empty)
        {
            return BadRequest(ApiResult<int>.Fail("Không tìm thấy Designer tương ứng."));
        }

        try
        {
            var designId = await _designDraftService.CreateDraftDesignAsync(request, (Guid)designerId);
            return Ok(ApiResult<int>.Succeed(designId));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResult<object>.Fail(ex));
        }
    }

        [HttpGet("drafts")]
    public async Task<ActionResult<ApiResult<List<DesignDraftDto>>>> GetDrafts()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized(ApiResult<List<DesignDraftDto>>.Fail("Không xác định được user."));

        var designerId = await _designerService.GetDesignerIdByUserId(userId);
        if (designerId == Guid.Empty)
            return BadRequest(ApiResult<List<DesignDraftDto>>.Fail("Không tìm thấy designer."));

        var result = await _designDraftService.GetAllDraftsAsync((Guid)designerId);
        return Ok(ApiResult<List<DesignDraftDto>>.Succeed(result));
    }

    [HttpGet("drafts/{id}")]
    public async Task<ActionResult<ApiResult<DraftDesignDetailDto>>> GetDraftDetail(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return Unauthorized(ApiResult<DraftDesignDetailDto>.Fail("Không xác định được user."));

        var designerId = await _designerService.GetDesignerIdByUserId(userId);
        if (designerId == Guid.Empty)
            return BadRequest(ApiResult<DraftDesignDetailDto>.Fail("Không tìm thấy designer."));

        var result = await _designDraftService.GetDraftDetailAsync(id, (Guid)designerId);
        if (result == null)
            return NotFound(ApiResult<DraftDesignDetailDto>.Fail("Không tìm thấy bản thiết kế nháp."));

        return Ok(ApiResult<DraftDesignDetailDto>.Succeed(result));
    }

   
     
    [HttpPut("update-draft")]
    public async Task<ActionResult<ApiResult<bool>>> UpdateDraft([FromForm] DraftDesignUpdateRequest request, DesignDraftService _designDraftService)
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

        try
        {
            await _designDraftService.UpdateDraftDesignAsync(request, (Guid)designerId);
            return Ok(ApiResult<bool>.Succeed(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResult<object>.Fail(ex));
        }

    }

    [HttpGet("calculate-fabric-usage/{designId}")]
    public async Task<ActionResult<ApiResult<FabricUsageResponse>>> CalculateFabricUsage(int designId)
    {
        try
        {
            var result = await _designDraftService.CalculateFabricUsageByMaterialAsync(designId);

            return Ok(ApiResult<FabricUsageResponse>.Succeed(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResult<object>.Fail(ex.Message));
        }
    }


    
    [HttpGet("types")]
   
    public async Task<IActionResult> GetAllItemTypes()
    {
        var itemTypes = await _designDraftService.GetAllItemTypesAsync();
        return  Ok(ApiResult<object>.Succeed(itemTypes));
    }

    
    [HttpGet("ratios")]
    public async Task<IActionResult> GetAllItemTypeSizeRatios()
    {
        var ratios = await _designDraftService.GetAllItemTypeSizeRatiosAsync();
        return Ok(ApiResult<object>.Succeed(ratios));

    }
    [HttpDelete("{designId}")]
    public async Task<IActionResult> DeleteDesign(int designId)
    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResult<object>.Fail("Không xác định được user."));
        }
 
        var designerId = await _designerService.GetDesignerIdByUserId(userId);
       
        var success = await _designDraftService.DeleteDesignAsync(designId, (Guid)designerId);

        if (!success)
        {
           
            return BadRequest(ApiResult<object>.Fail(" Đã có sản phẩm liên kết."));
        }

        return Ok(ApiResult<object>.Succeed("Xóa thiết kế thành công."));
    }



}


   