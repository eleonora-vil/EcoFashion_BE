using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Dtos.Wallet;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Extensions.NewFolder;
using EcoFashionBackEnd.Helpers;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace EcoFashionBackEnd.Services
{
    public class WalletService
    {
        private readonly IRepository<Wallet, int> _walletRepository;
        private readonly IRepository<WalletTransaction, int> _walletTransactionRepository;
        private readonly IVnPayService _vnPayService;
        private readonly IRepository<PaymentTransaction, int> _paymentTransactionRepository;


        public WalletService(
            IRepository<Wallet, int> walletRepository,
            IRepository<WalletTransaction, int> walletTransactionRepository,
            IVnPayService vnPayService            ,
            IRepository<PaymentTransaction, int> paymentTransactionRepository
            )
        {
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _vnPayService = vnPayService;
            _paymentTransactionRepository = paymentTransactionRepository;
        }
        public async Task<ApiResult<object>> CreateDepositAsync(int userId, double amount, HttpContext httpContext)
        {
            // 1. Tạo transaction pending trong DB
            var transaction = new WalletTransaction
            {
                WalletId = userId,
                Amount = amount,
                Type = TransactionType.Deposit,
                Status = Entities.TransactionStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _walletTransactionRepository.AddAsync(transaction);
            await _walletTransactionRepository.Commit();

            // 2. Build VNPay model
            var vnPayModel = new VnPaymentRequestModel
            {
                OrderId = transaction.Id, // dùng ID làm mã duy nhất
                Amount = amount,
                CreatedDate = DateTime.Now
            };

            var paymentUrl = await _vnPayService.CreateDepositPaymentUrlAsync(httpContext, vnPayModel);

            return ApiResult<object>.Succeed(new { PaymentUrl = paymentUrl });
        }


        public async Task<VnPaymentResponseModel> HandleVNPayDepositReturnAsync(IQueryCollection collection)
        {
            var response = _vnPayService.PaymentExecute(collection);

            if (!int.TryParse(response.OrderId, out int orderId))
                throw new Exception("Invalid OrderId from VNPay");


            var walletTransaction = await _walletTransactionRepository.GetByIdAsync(orderId) ;
            if (walletTransaction == null)
            {
                throw new Exception($"WalletTransaction not found for TxnRef: {response.TxnRef}");
            }

            var wallet = await _walletRepository.GetByIdAsync(walletTransaction.WalletId);
            if (wallet == null)
            {
                throw new Exception($"Wallet not found for user: {walletTransaction.WalletId}");
            }

            // Idempotent check: nếu WalletTransaction đã Success/Fail thì bỏ qua
            if (walletTransaction.Status == Entities.TransactionStatus.Success ||
                walletTransaction.Status == Entities.TransactionStatus.Fail)
            {
                return response;
            }

            if (response.VnPayResponseCode == "00")
            {
                // Thành công -> cộng tiền vào ví
                walletTransaction.Status = Entities.TransactionStatus.Success;
                walletTransaction.BalanceBefore = wallet.Balance;
                walletTransaction.BalanceAfter = wallet.Balance + walletTransaction.Amount;

                wallet.Balance += walletTransaction.Amount;
                await _walletRepository.UpdateAsync(wallet);
            }
            else
            {
                // Thất bại
                walletTransaction.Status = Entities.TransactionStatus.Fail;
            }

            await _walletTransactionRepository.UpdateAsync(walletTransaction);
            await _walletTransactionRepository.Commit();

            return response;
        }


        public async Task<WalletTransactionDto> RequestWithdrawalAsync(int userId, double amount, string? description = null)
        {
            var wallet = await _walletRepository.GetAll()
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                throw new Exception("Wallet not found");

            if (wallet.Balance < amount)
                throw new Exception("Insufficient balance");

            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                BalanceBefore = wallet.Balance,
                BalanceAfter = wallet.Balance, // chưa trừ tiền
                Type = TransactionType.Withdrawal,
                Status = Entities.TransactionStatus.Pending,
                Description = description ?? $"Request withdrawal {amount} VND",
                CreatedAt = DateTime.UtcNow
            };

            await _walletTransactionRepository.AddAsync(transaction);
            await _walletTransactionRepository.Commit();

            return new WalletTransactionDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                BalanceBefore = transaction.BalanceBefore,
                BalanceAfter = transaction.BalanceAfter,
                Type = transaction.Type,
                Status = transaction.Status,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            }; 
        }


        public async Task<ApiResult<object>> CreateWithdrawalPaymentAsync(int walletTransactionId, HttpContext httpContext)
        {
            var withdrawalRequest = await _walletTransactionRepository.GetByIdAsync(walletTransactionId);
            if (withdrawalRequest == null)
                return ApiResult<object>.Fail("Không tìm thấy yêu cầu rút tiền");

            if (withdrawalRequest.Type != withdrawalRequest.Type)
                return ApiResult<object>.Fail("TransactionId không phải yêu cầu rút tiền");

            if (withdrawalRequest.Status != Entities.TransactionStatus.Pending)
                return ApiResult<object>.Fail("Yêu cầu này đã được xử lý rồi");

            withdrawalRequest.Status = Entities.TransactionStatus.Success;
            await _walletTransactionRepository.UpdateAsync(withdrawalRequest);
            await _walletTransactionRepository.Commit();
            var wallet = await _walletRepository.GetByIdAsync(withdrawalRequest.WalletId);
            if (wallet == null)
                return ApiResult<object>.Fail("Không tìm thấy ví của người dùng");

            var withdrawalTxn = new WalletTransaction
            {
                WalletId = withdrawalRequest.WalletId,
                BalanceBefore = wallet.Balance,          // Trước khi trừ
                BalanceAfter = wallet.Balance - withdrawalRequest.Amount, // Sau khi trừ
                Amount = withdrawalRequest.Amount,
                Type = TransactionType.Withdrawal,
                Status = Entities.TransactionStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _walletTransactionRepository.AddAsync(withdrawalTxn);
            await _walletTransactionRepository.Commit();

            var vnPayModel = new VnPaymentRequestModel
            {
                OrderId = withdrawalTxn.Id,
                Amount = withdrawalTxn.Amount,
                CreatedDate = DateTime.Now
            };

            var paymentUrl = await _vnPayService.CreateWithdrawalPaymentUrlAsync(httpContext, vnPayModel);

            return ApiResult<object>.Succeed(new { PaymentUrl = paymentUrl });
        }

        public async Task<VnPaymentResponseModel> HandleVNPayWithdrawalReturnAsync(IQueryCollection collection)
        {
            var response = _vnPayService.PaymentExecute(collection);

            if (!int.TryParse(response.OrderId, out int orderId))
                throw new Exception("Invalid OrderId from VNPay");

            var withdrawalTxn = await _walletTransactionRepository.GetByIdAsync(orderId);
            if (withdrawalTxn == null)
                throw new Exception($"WalletTransaction not found for TxnRef: {response.TxnRef}");

            var wallet = await _walletRepository.GetByIdAsync(withdrawalTxn.WalletId);
            if (wallet == null)
                throw new Exception($"Wallet not found for WalletId: {withdrawalTxn.WalletId}");

            if (withdrawalTxn.Status == Entities.TransactionStatus.Success ||
                withdrawalTxn.Status == Entities.TransactionStatus.Fail)
            {
                return response;
            }

            if (response.VnPayResponseCode == "00")
            {
                withdrawalTxn.Status = Entities.TransactionStatus.Success;
                wallet.Balance -= withdrawalTxn.Amount;

            }
            else
            {
                withdrawalTxn.Status = Entities.TransactionStatus.Fail;
            }
            await _walletTransactionRepository.UpdateAsync(withdrawalTxn);
            await _walletTransactionRepository.Commit();

            return response;
        }



        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var wallet = await _walletRepository.GetAll().FirstOrDefaultAsync(w => w.UserId == userId);
            return (decimal)(wallet?.Balance ?? 0);
        }


        public async Task<List<WalletTransactionDto>> GetTransactionsAsync(int userId)
        {
            var wallet = await _walletRepository.GetAll().FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null) return new List<WalletTransactionDto>();

            var transactions = await _walletTransactionRepository.GetAll()
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return transactions.Select(t => new WalletTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                BalanceBefore = t.BalanceBefore,
                BalanceAfter = t.BalanceAfter,
                Status = t.Status,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList();
        }


        private string GetMessageFromResponseCode(string code)
        {
            return code switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Giao dịch bị nghi ngờ (liên hệ ngân hàng)",
                "09" => "Thẻ/Tài khoản chưa đăng ký InternetBanking",
                "10" => "Xác thực thông tin thẻ/tài khoản thất bại",
                "51" => "Tài khoản không đủ số dư",
                "65" => "Tài khoản bị khóa",
                _ => "Giao dịch thất bại"
            };
        }
    }
}

        //public async Task DepositAsync(int userId, double amount, string externalTxnId)
        //{
        //    // 1. Tìm ví user
        //    var wallet = await _walletRepository.GetAll().FirstOrDefaultAsync(w => w.UserId == userId);
        //    if (wallet == null)
        //    {
        //        wallet = new Wallet
        //        {
        //            UserId = userId,
        //            Balance = 0,
        //            CreatedAt = DateTime.UtcNow
        //        };
        //        await _walletRepository.AddAsync(wallet);
        //    }

        //    // 2. Update balance
        //    wallet.Balance += amount;
        //    await _walletRepository.UpdateAsync(wallet);

        //    // 3. Ghi transaction TransactionScope nếu được 
        //    var transaction = new WalletTransaction
        //    {
        //        WalletId = wallet.WalletId,
        //        Amount = amount,
        //        Type =TransactionType.Deposit,
        //        Description = $"Nạp tiền qua VNPay - Ref: {externalTxnId}",
        //        CreatedAt = DateTime.UtcNow,
        //        BalanceAfter = wallet.Balance
        //    };
        //    await _walletTransactionRepository.AddAsync(transaction);

        //    // 4. Update ví hệ thống (admin)
        //    var adminWallet = await _walletRepository.GetAll().FirstOrDefaultAsync(w => w.UserId == 1);
        //    if (adminWallet == null)
        //    {
        //        adminWallet = new Wallet
        //        {
        //            WalletId = 1,
        //            UserId = 1, // Admin
        //            Balance = 0,
        //            CreatedAt = DateTime.UtcNow
        //        };
        //        await _walletRepository.AddAsync(adminWallet);
        //    }

        //    adminWallet.Balance += amount;
        //    await _walletRepository.UpdateAsync(adminWallet);

        //    var adminTransaction = new WalletTransaction
        //    {
        //        WalletId = adminWallet.WalletId,
        //        Amount = amount,
        //        Type = TransactionType.Deposit,
        //        Description = $"User {userId} nạp tiền qua VNPay - Ref: {externalTxnId}",
        //        CreatedAt = DateTime.UtcNow,
        //        BalanceAfter = adminWallet.Balance
        //    };
        //    await _walletTransactionRepository.AddAsync(adminTransaction);
        //}