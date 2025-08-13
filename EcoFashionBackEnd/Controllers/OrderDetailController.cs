using AutoMapper;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers;

[Route("api/OrderDetail")]
[ApiController]
public class OrderDetailController : ControllerBase
{
    private readonly OrderDetailService _orderDetailService;
    private readonly IMapper _mapper;
    public OrderDetailController(OrderDetailService service,  IMapper mapper)
    {
        _orderDetailService = service;
        _mapper = mapper;
    }
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllOrderDetail()
    {
        var orderDetails = await _orderDetailService.GetAllOrderDetailAsync();
        return Ok(ApiResult<IEnumerable<OrderDetailModel>>.Succeed(orderDetails));
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetailById(int id)
    {
        var order = await _orderDetailService.GetOrderDetailByIdAsync(id);
        if (order == null)
            return NotFound(ApiResult<object>.Fail("Không tìm thấy chi tiết đơn hàng"));
        return Ok(ApiResult<OrderDetailModel>.Succeed(order));
    }
    [HttpPost("Create")]
    public async Task<IActionResult> CreateOrderDetail(CreateOrderDetailRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var orderDetailId = await _orderDetailService.CreateOrderDetailAsync(userId,request);
        return CreatedAtAction(nameof(GetOrderDetailById), new {id = orderDetailId}, ApiResult<object>.Succeed(new { OrderDetailId = orderDetailId }));
    }
    [HttpPut("Update{orderDetailId}")]
    public async Task<IActionResult> UpdateOrderDetail(int orderDetailId, [FromForm] UpdateOrderDetailRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        try
        {
            var success = await _orderDetailService.UpdateOrderDetailAsync(orderDetailId, request);

            if (!success)
            {
                return NotFound(ApiResult<string>.Fail("Không tìm thấy chi tiết đơn hàng hoặc cập nhật thất bại."));
            }

            return Ok(ApiResult<string>.Succeed("Cập nhật chi tiết đơn hàng thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResult<string>.Fail($"Lỗi dữ liệu: {ex.Message}"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResult<string>.Fail("Đã xảy ra lỗi khi cập nhật chi tiết đơn hàng."));
        }
    }
    [HttpDelete("{orderDetailId}")]
    public async Task<IActionResult> DeleteOrderDetail(int orderDetailId)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            return Unauthorized(ApiResult<CreateDesignResponse>.Fail("Không thể xác định người dùng."));
        var success = await _orderDetailService.DeleteOrderDetailAsync(orderDetailId);
        if (success)
            return Ok(ApiResult<string>.Succeed("Xóa chi tiết đơn hàng thành công"));
        return NotFound(ApiResult<string>.Fail("Không tìm thấy chi tiết đơn hàng"));
    }
}
