using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

public interface IOrderPayoutService
{
    Task ProcessPayoutsAsync();
}

public class OrderPayoutService : IOrderPayoutService
{
    private readonly AppDbContext _context;

    public OrderPayoutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessPayoutsAsync()
    {
        var orders = await _context.Orders
            .Where(o => o.FulfillmentStatus == FulfillmentStatus.Delivered && !o.IsPaidOut)
            .ToListAsync();

        if (!orders.Any()) return;

        var systemWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletId == 1); 

        foreach (var order in orders)
        {
            var sellerUser = await GetSellerUserAsync(order.SellerType, order.SellerId);
            if (sellerUser == null) continue;

            var sellerWallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == sellerUser.UserId);
            if (sellerWallet == null) continue;

            var orderAmount = order.TotalPrice;
            var systemFee = orderAmount * 0.1m; 
            var sellerAmount = orderAmount - systemFee;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = systemWallet.WalletId,
                BalanceBefore = systemWallet.Balance,
                BalanceAfter = systemWallet.Balance - (double)sellerAmount,
                Type =TransactionType.Transfer,
                Status =TransactionStatus.Success,
                Amount = (double)-sellerAmount,
                Description = $"Payout order {order.OrderId} to {order.SellerType} {order.SellerId}",
                CreatedAt = DateTime.UtcNow
            });

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = sellerWallet.WalletId,
                Type = TransactionType.Transfer,
                Amount = (double)sellerAmount,
                BalanceBefore = sellerWallet.Balance,
                Status = TransactionStatus.Success,
                BalanceAfter = sellerWallet.Balance + (double)sellerAmount,
                Description = $"Received payout for order {order.OrderId}, fee {systemFee}",
                CreatedAt = DateTime.UtcNow
            });

            systemWallet.Balance -= (double)sellerAmount;
            sellerWallet.Balance += (double)sellerAmount;

            
            order.IsPaidOut = true;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<User?> GetSellerUserAsync(string? sellerType, Guid? sellerId)
    {
        if (sellerType == "Supplier")
        {
            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SupplierId == sellerId);
            return supplier?.User;
        }
        else if (sellerType == "Designer")
        {
            var designer = await _context.Designers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DesignerId == sellerId);
            return designer?.User;
        }
        return null;
    }
}
