using EcoFashionBackEnd.Dtos;

namespace EcoFashionBackEnd.Extensions.NewFolder
{
   
    public interface IVnPayService
    {
        Task<string> CreatePaymentUrlAsync(HttpContext context, VnPaymentRequestModel model);
        //Task<string> CreateVNPayUrlAsync(HttpContext context, VnPaymentRequestModel model);
        Task<string> CreateDepositPaymentUrlAsync(HttpContext context, VnPaymentRequestModel model);
        Task<string> CreateWithdrawalPaymentUrlAsync(HttpContext context, VnPaymentRequestModel model);

        VnPaymentResponseModel PaymentExecute(IQueryCollection collection);
    }
}
