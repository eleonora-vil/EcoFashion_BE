using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.TransactionInventory;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionsController : ControllerBase
    {
        private readonly InventoryTransactionService _inventoryTransactionService;
        private readonly DesignerService _designerService;


        public InventoryTransactionsController(InventoryTransactionService inventoryTransactionService, DesignerService designerService)
        {
            _inventoryTransactionService = inventoryTransactionService;
            _designerService = designerService;
        }

        /// Lấy tất cả transaction của Product
        [HttpGet("products")]
        public async Task<IActionResult> GetProductTransactions()
        {
            var result = await _inventoryTransactionService.GetAllProductTransactionsAsync();
            return Ok(ApiResult<List<ProductInventoryTransactionDto>>.Succeed(result));
        }


        /// Lấy tất cả transaction của MaterialInventory(designer) 
        [HttpGet("materials")]
        public async Task<IActionResult> GetMaterialTransactions()
        {
            var result = await _inventoryTransactionService.GetAllMaterialTransactionsAsync();
            return Ok(ApiResult<List<MaterialInventoryTransactionDto>>.Succeed(result));
        }


        /// Lấy tất cả transaction (gom cả Product + Material)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var result = await _inventoryTransactionService.GetAllTransactionsAsync();
            return Ok(ApiResult<List<InventoryTransactionDto>>.Succeed(result));
        }


        [HttpGet("By-DesignerId")]
        public async Task<IActionResult> GetTransactionsByDesigner()
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

            var result = await _inventoryTransactionService.GetTransactionsByDesignerAsync((Guid)designerId);
            return Ok(ApiResult<List<InventoryTransactionDto>>.Succeed(result));
        }
    }
}
