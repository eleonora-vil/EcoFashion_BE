using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests.Product;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;  // service chứa CreateProductsAsync
        private readonly ILogger<ProductsController> _logger;
        private readonly DesignerService _designerService;

        public ProductsController(ProductService productService, ILogger<ProductsController> logger, DesignerService designerService)
        {
            _productService = productService;
            _logger = logger;
            _designerService = designerService;
        }

        [HttpPost("CreateByNewVariant")]
        public async Task<IActionResult> CreateProducts([FromForm] ProductCreateRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(ApiResult<int>.Fail("Không thể xác định người dùng."));

            var designerId = await _designerService.GetDesignerIdByUserId(userId);
            if (designerId == Guid.Empty)
                return BadRequest(ApiResult<int>.Fail("Không tìm thấy Designer tương ứng."));

            try
            {
                var productIds = await _productService.CreateProductsAsync(request, (Guid)designerId);
                return Ok(ApiResult<List<int>>.Succeed(productIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tạo sản phẩm");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("CreateByExistVariant")]
        public async Task<IActionResult> CreateFromDesign([FromForm] CreateProductsFromDesignRequest request)
        {
            // xác thực user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(ApiResult<List<int>>.Fail("Không thể xác định người dùng."));

            var designerId = await _designerService.GetDesignerIdByUserId(userId);
            if (designerId == Guid.Empty)
                return BadRequest(ApiResult<List<int>>.Fail("Không tìm thấy Designer tương ứng."));

            if (request == null || request.DesignId <= 0)
                return BadRequest(ApiResult<List<int>>.Fail("DesignId không hợp lệ."));

            try
            {
                // Gọi service: service sẽ lấy variants từ DB theo DesignId
                var createdIds = await _productService.CreateProductsWithExistVariantAsync(request, (Guid)designerId);

                return Ok(ApiResult<List<int>>.Succeed(createdIds));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResult<List<int>>.Fail(ex.Message));
            }
        }
    }
}
