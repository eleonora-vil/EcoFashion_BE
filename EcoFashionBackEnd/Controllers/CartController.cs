using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos.Cart;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers;

[Route("api/Cart")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
	private readonly CartService _cartService;
	public CartController(CartService cartService)
	{
		_cartService = cartService;
	}

	[HttpGet]
	public async Task<IActionResult> GetMyCart()
	{
		if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
		var cart = await _cartService.GetOrCreateActiveCartAsync(userId);
		return Ok(ApiResult<CartDto>.Succeed(cart));
	}

	[HttpPut("items")]
	public async Task<IActionResult> UpsertItem([FromBody] UpsertCartItemRequest request)
	{
		if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
		var cart = await _cartService.UpsertItemAsync(userId, request);
		return Ok(ApiResult<CartDto>.Succeed(cart));
	}

	[HttpPatch("items/{cartItemId}")]
	public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemQuantityRequest request)
	{
		if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
		var cart = await _cartService.UpdateQuantityAsync(userId, cartItemId, request.Quantity);
		return Ok(ApiResult<CartDto>.Succeed(cart));
	}

	[HttpDelete("items/{cartItemId}")]
	public async Task<IActionResult> RemoveItem(int cartItemId)
	{
		if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
		var cart = await _cartService.RemoveItemAsync(userId, cartItemId);
		return Ok(ApiResult<CartDto>.Succeed(cart));
	}

	[HttpPost("clear")]
	public async Task<IActionResult> Clear()
	{
		if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
			return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
		var cart = await _cartService.ClearAsync(userId);
		return Ok(ApiResult<CartDto>.Succeed(cart));
	}
}


