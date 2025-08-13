using EcoFashionBackEnd.Dtos;
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

        public CheckoutController(CheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
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
    }
}


