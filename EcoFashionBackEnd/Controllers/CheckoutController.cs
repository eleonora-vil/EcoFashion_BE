using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly CheckoutService _checkoutService;
        private readonly CartService _cartService;

        public CheckoutController(CheckoutService checkoutService, CartService cartService)
        {
            _checkoutService = checkoutService;
            _cartService = cartService;
        }

        // Tạo OrderGroup + nhiều Order theo Seller, kèm OrderDetail
        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var result = await _checkoutService.CreateSessionAsync(userId, request);
            return Ok(result);
        }

        // Tạo session trực tiếp từ cart hiện tại của user
        [HttpPost("create-session-from-cart")]
        public async Task<IActionResult> CreateSessionFromCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            // Lấy cart enrich rồi chuyển về CreateSessionRequest
            var cart = await _cartService.GetOrCreateActiveCartAsync(userId);
            if (!cart.Items.Any())
            {
                return BadRequest(new { message = "Giỏ hàng trống." });
            }

            var request = new CreateSessionRequest
            {
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ItemType = "material",
                    MaterialId = i.MaterialId,
                    Quantity = i.Quantity,
                    UnitPrice = i.CurrentPrice > 0 ? i.CurrentPrice : i.UnitPriceSnapshot,
                    SellerType = "Supplier",
                    SellerId = i.SupplierId
                }).ToList(),
                ShippingAddress = "Địa chỉ giao hàng"
            };

            var result = await _checkoutService.CreateSessionAsync(userId, request);
            return Ok(result);
        }
    }
}


