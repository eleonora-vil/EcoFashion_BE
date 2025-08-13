using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcoFashionBackEnd.Services;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/analytics/inventory")]
    [Authorize(Roles = "admin")] // Chỉ admin dùng báo cáo tổng
    public class InventoryAnalyticsController : ControllerBase
    {
        private readonly InventoryAnalyticsService _svc;

        public InventoryAnalyticsController(InventoryAnalyticsService svc)
        {
            _svc = svc;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? supplierId, [FromQuery] int? warehouseId, [FromQuery] int? materialId)
        {
            var result = await _svc.GetSummaryAsync(from, to, supplierId, warehouseId, materialId);
            return Ok(result);
        }

        [HttpGet("movements")]
        public async Task<IActionResult> Movements([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? supplierId, [FromQuery] int? warehouseId, [FromQuery] int? materialId)
        {
            var result = await _svc.GetMovementsAsync(from, to, supplierId, warehouseId, materialId);
            return Ok(result);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> LowStock([FromQuery] Guid? supplierId, [FromQuery] int? warehouseId, [FromQuery] int? materialTypeId, [FromQuery] int limit = 50)
        {
            var result = await _svc.GetLowStockAsync(supplierId, warehouseId, materialTypeId, limit);
            return Ok(result);
        }

        [HttpGet("receipts-by-supplier")]
        public async Task<IActionResult> ReceiptsBySupplier([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? materialId)
        {
            var result = await _svc.GetReceiptsBySupplierAsync(from, to, materialId);
            return Ok(result);
        }
    }
}


