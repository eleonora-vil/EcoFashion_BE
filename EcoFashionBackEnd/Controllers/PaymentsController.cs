using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Extensions.NewFolder;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _config;

        public PaymentsController(PaymentService paymentService, IConfiguration config)
        {
            _paymentService = paymentService;
            _config = config;
        }

        [HttpPost("create-vnpay")]
        public async Task<IActionResult> CreateVNPayPayment([FromBody] VnPaymentRequestModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(ApiResult<object>.Fail("Không xác định được người dùng."));

            var url = await _paymentService.CreateVNPayUrlAsync(HttpContext, model, userId);
            return Ok(new { redirectUrl = url });
        }

        [HttpGet("/Checkout/PaymentCallbackVnpay")]
        public async Task<IActionResult> VNPayReturn()
        {
            var result = await _paymentService.HandleVNPayReturnAsync(Request.Query);
            // Redirect về FE để wizard tiếp tục với order hiện tại
            var orderId = result.OrderId;
            var groupId = Request.Query["groupId"].ToString(); // optional nếu FE đính kèm khi gửi đi
            var status = result.VnPayResponseCode;
            var feUrl = _config["Frontend:CheckoutResultUrl"] ?? "/checkout/result";
            var join = feUrl.Contains("?") ? "&" : "?";
            var url = $"{feUrl}{join}orderId={orderId}&groupId={groupId}&code={status}";
            return Redirect(url);
        }

        // VNPAY IPN (server-to-server). VNPAY thường gửi dạng GET; hỗ trợ cả POST nếu cần.
        [HttpGet("vnpay/ipn")]
        public async Task<IActionResult> VNPayIpnGet()
        {
            var response = await _paymentService.HandleVNPayIpnAsync(Request.Query);
            // Trả đúng format VNPAY yêu cầu (thường là JSON với RspCode/Message)
            return Ok(response);
        }

        [HttpPost("vnpay/ipn")]
        public async Task<IActionResult> VNPayIpnPost()
        {
            IQueryCollection queryCollection = Request.HasFormContentType
                ? new QueryCollection(Request.Form.ToDictionary(kv => kv.Key, kv => kv.Value))
                : Request.Query;
            var response = await _paymentService.HandleVNPayIpnAsync(queryCollection);
            return Ok(response);
        }
    }
}
