using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Dtos.Warehouse;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/Inventory/materials")]
    public class MaterialInventoryController : ControllerBase
    {
        private readonly MaterialInventoryService _inventoryService;

        public MaterialInventoryController(MaterialInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("stocks")]
        [Authorize(Roles = "admin,supplier")]
        public async Task<IActionResult> GetStocks([FromQuery] int? materialId, [FromQuery] int? warehouseId)
        {
            Guid? supplierId = null;
            if (User.IsInRole("supplier"))
            {
                var sid = User.FindFirst("SupplierId")?.Value;
                if (Guid.TryParse(sid, out var g)) supplierId = g;
            }
            var result = await _inventoryService.GetStocksAsync(supplierId, materialId, warehouseId);
            return Ok(result);
        }

        [HttpGet("transactions")]
        [Authorize(Roles = "admin,supplier")]
        public async Task<IActionResult> GetTransactions([FromQuery] int? materialId, [FromQuery] int? warehouseId, [FromQuery] string? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            Guid? supplierId = null;
            if (User.IsInRole("supplier"))
            {
                var sid = User.FindFirst("SupplierId")?.Value;
                if (Guid.TryParse(sid, out var g)) supplierId = g;
            }
            var result = await _inventoryService.GetTransactionsAsync(supplierId, materialId, warehouseId, type, from, to);
            return Ok(result);
        }

        [HttpPost("receive")]
        [Authorize(Roles = "admin,supplier")]
        public async Task<IActionResult> Receive([FromBody] ReceiveMaterialRequest request)
        {
            int? userId = null;
            var uid = User.FindFirst("UserId")?.Value;
            if (int.TryParse(uid, out var parsed)) userId = parsed;

            var ok = await _inventoryService.ReceiveAsync(request.MaterialId, request.WarehouseId, request.Quantity, request.Unit, request.Note, request.ReferenceType, request.ReferenceId, userId);
            return Ok(ok);
        }
    }
}


