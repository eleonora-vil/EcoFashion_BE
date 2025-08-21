using EcoFashionBackEnd.Common.Payloads.Requests.Variant;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using EcoFashionBackEnd.Dtos.DesignDraft;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DesignsVariantController : ControllerBase
    {
        private readonly DesignsVariantService _variantService;

        public DesignsVariantController(DesignsVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpGet("{designId}/variants")]
        public async Task<ActionResult<ApiResult<List<VariantDetailsDto>>>> GetVariants(int designId)
        {
            var variants = await _variantService.GetVariantsByDesignIdAsync(designId);
            return Ok(ApiResult<List<VariantDetailsDto>>.Succeed(variants));
        }

        [HttpGet("variants/{variantId}")]
        public async Task<ActionResult<ApiResult<VariantDetailsDto>>> GetVariant(int variantId)
        {
            var variant = await _variantService.GetVariantByIdAsync(variantId);
            if (variant == null)
            {
                return NotFound(ApiResult<VariantDetailsDto>.Fail("Variant không tồn tại."));
            }
            return Ok(ApiResult<VariantDetailsDto>.Succeed(variant));
        }

        [HttpPost("{designId}/variants")]
        public async Task<ActionResult<ApiResult<bool>>> CreateVariant(int designId, [FromBody] DesignsVariantCreateRequest request)
        {
            try
            {
                // Gọi hàm với list chứa 1 phần tử nếu service yêu cầu list
                var created = await _variantService.CreateVariantAsync(designId, new List<DesignsVariantCreateRequest> { request });

                if (!created) return NotFound(ApiResult<bool>.Fail("Design không tồn tại."));
                return Ok(ApiResult<bool>.Succeed(true));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }



        [HttpPut("variants/{variantId}")]
        public async Task<ActionResult<ApiResult<bool>>> UpdateVariant(int variantId, [FromBody] DesignsVariantUpdateRequest request)
        {
            try
            {
                var updated = await _variantService.UpdateVariantAsync(variantId, request);
                if (!updated) return NotFound(ApiResult<bool>.Fail("Variant không tồn tại."));
                return Ok(ApiResult<bool>.Succeed(true));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("variants/{variantId}")]
        public async Task<ActionResult<ApiResult<bool>>> DeleteVariant(int variantId)
        {
            var deleted = await _variantService.DeleteVariantAsync(variantId);
            if (!deleted) return NotFound(ApiResult<bool>.Fail("Variant không tồn tại."));
            return Ok(ApiResult<bool>.Succeed(true));
        }
    }
}
