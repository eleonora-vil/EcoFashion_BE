using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos.Cart;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
	public class CartService
	{
		private readonly AppDbContext _db;
		public CartService(AppDbContext db)
		{
			_db = db;
		}

		public async Task<CartDto> GetOrCreateActiveCartAsync(int userId)
		{
			var cart = await _db.Carts.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
			if (cart == null)
			{
				cart = new Cart { UserId = userId, IsActive = true };
				_db.Carts.Add(cart);
				await _db.SaveChangesAsync();
			}
			return await BuildCartDto(cart.CartId);
		}

		public async Task<CartDto> UpsertItemAsync(int userId, UpsertCartItemRequest request)
		{
			var cartId = await EnsureCartId(userId);
			var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.MaterialId == request.MaterialId);
			if (item == null)
			{
				var price = await _db.Materials.Where(m => m.MaterialId == request.MaterialId).Select(m => m.PricePerUnit).FirstOrDefaultAsync();
				item = new CartItem
				{
					CartId = cartId,
					MaterialId = request.MaterialId,
					Quantity = request.Quantity,
					UnitPriceSnapshot = price,
				};
				_db.CartItems.Add(item);
			}
			else
			{
				item.Quantity = request.Quantity;
				item.UpdatedAt = DateTime.UtcNow;
			}
			await _db.SaveChangesAsync();
			return await BuildCartDto(cartId);
		}

		public async Task<CartDto> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
		{
			var cartId = await EnsureCartId(userId);
			var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.CartId == cartId);
			if (item == null) throw new KeyNotFoundException("Cart item không tồn tại");
			item.Quantity = quantity;
			item.UpdatedAt = DateTime.UtcNow;
			await _db.SaveChangesAsync();
			return await BuildCartDto(cartId);
		}

		public async Task<CartDto> RemoveItemAsync(int userId, int cartItemId)
		{
			var cartId = await EnsureCartId(userId);
			var item = await _db.CartItems.FirstOrDefaultAsync(i => i.CartItemId == cartItemId && i.CartId == cartId);
			if (item != null)
			{
				_db.CartItems.Remove(item);
				await _db.SaveChangesAsync();
			}
			return await BuildCartDto(cartId);
		}

		public async Task<CartDto> ClearAsync(int userId)
		{
			var cartId = await EnsureCartId(userId);
			var items = _db.CartItems.Where(i => i.CartId == cartId);
			_db.CartItems.RemoveRange(items);
			await _db.SaveChangesAsync();
			return await BuildCartDto(cartId);
		}

		private async Task<int> EnsureCartId(int userId)
		{
			var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
			if (cart == null)
			{
				cart = new Cart { UserId = userId, IsActive = true };
				_db.Carts.Add(cart);
				await _db.SaveChangesAsync();
			}
			return cart.CartId;
		}

		private async Task<CartDto> BuildCartDto(int cartId)
		{
			var cart = await _db.Carts.AsNoTracking().FirstAsync(c => c.CartId == cartId);
			var items = await _db.CartItems.AsNoTracking()
				.Where(i => i.CartId == cartId)
				.ToListAsync();
			var materialIds = items.Select(i => i.MaterialId).Distinct().ToList();
			var materials = await _db.Materials
				.Where(m => materialIds.Contains(m.MaterialId))
				.Include(m => m.MaterialImages).ThenInclude(mi => mi.Image)
				.Include(m => m.Supplier)
				.ToListAsync();
			var dto = new CartDto
			{
				CartId = cart.CartId,
				IsActive = cart.IsActive,
				UpdatedAt = cart.UpdatedAt,
				Items = items.Select(i =>
				{
					var m = materials.First(x => x.MaterialId == i.MaterialId);
					return new CartItemDto
					{
						CartItemId = i.CartItemId,
						MaterialId = i.MaterialId,
						Quantity = i.Quantity,
						UnitPriceSnapshot = i.UnitPriceSnapshot,
						CurrentPrice = m.PricePerUnit,
						MaterialName = m.Name,
						ImageUrl = m.MaterialImages.FirstOrDefault()?.Image?.ImageUrl,
						UnitLabel = "mét",
						SupplierId = m.SupplierId,
						SupplierName = m.Supplier?.SupplierName
					};
				}).ToList()
			};
			return dto;
		}
	}
}


