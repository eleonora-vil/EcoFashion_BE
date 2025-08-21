namespace EcoFashionBackEnd.Controllers
{
    using EcoFashionBackEnd.Common.Payloads.Requests.Wallet;
    using EcoFashionBackEnd.Dtos.Wallet;
    using EcoFashionBackEnd.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _walletService;

        public WalletController(WalletService walletService)
        {
            _walletService = walletService;
        }

        //  Lấy số dư ví
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Không thể xác định người dùng.");

            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(new { UserId = userId, Balance = balance });
        }

        
        //  Lấy lịch sử giao dịch ví
       
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Không thể xác định người dùng.");

            var transactions = await _walletService.GetTransactionsAsync(userId);
            return Ok(transactions);
        }


        // taọ link deposit 
        [HttpPost("deposit")]
        public async Task<IActionResult> CreateDepositLink([FromBody] DepositRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Số tiền nạp phải > 0");

            // Xác định userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Không thể xác định người dùng.");

            var result = await _walletService.CreateDepositAsync(userId, request.Amount, HttpContext);

            return Ok(result);
        }

        [HttpGet("deposit/callback")]
        public async Task<IActionResult> DepositCallback()
        {
            var query = HttpContext.Request.Query;

            var result = await _walletService.HandleVNPayDepositReturnAsync(query);

            if (result == null)
                return BadRequest("Xử lý nạp tiền thất bại.");

            return Ok(new
            {
                Status = result.VnPayResponseCode == "00" ? "Success" : "Fail",
                TransactionId = result.TransactionId,
                WalletTransaction = result.OrderId
            });
        }
        // taọ RequestWithdrawal
        [HttpPost("withdrawal/request")]
        public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequestDto request)
        {
            if (request.Amount <= 0)
                return BadRequest("Số tiền rút phải > 0");

            // Lấy userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Không thể xác định người dùng.");

            try
            {
                var transaction = await _walletService.RequestWithdrawalAsync(userId, request.Amount, request.Description);
                return Ok(transaction); // hoặc map sang DTO trước
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("withdrawal")]
        public async Task<IActionResult> CreateWithdrawalLink([FromBody] ApproveWithdrawalDto request)
        {
            if (request.WalletTransactionId <= 0)
                return BadRequest("Thiếu WalletTransactionId hợp lệ.");

            // 👤 Xác định userId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Không thể xác định người dùng.");

            var result = await _walletService.CreateWithdrawalPaymentAsync(request.WalletTransactionId, HttpContext);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("withdrawal/callback")]
        public async Task<IActionResult> WithdrawalCallback()
        {
            // 1️⃣ VNPay gọi GET với query vnp_*
            var query = HttpContext.Request.Query;

            // 2️⃣ Gọi service để handle transaction
            var result = await _walletService.HandleVNPayWithdrawalReturnAsync(query);

            // 3️⃣ FE có thể đọc DB hoặc redirect
            if (result == null)
                return BadRequest("Xử lý rút tiền thất bại.");

            return Ok(new
            {
                Status = result.VnPayResponseCode == "00" ? "Success" : "Fail",
                TransactionId = result.TransactionId,
                WalletTransaction = result.OrderId
            });
        }
    }
}
