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

        [HttpPost]
        public async Task<IActionResult> CreateProducts([FromBody] ProductCreateRequest request)
        {
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
            if (request == null || request.Variants == null || !request.Variants.Any())
                return BadRequest("Request hoặc variants không được để trống.");

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

    }
}
