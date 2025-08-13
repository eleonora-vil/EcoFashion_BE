using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Helpers;
using EcoFashionBackEnd.Services;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace EcoFashionBackEnd.Extensions.NewFolder
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentService> _logger;
        public VnPayService(IConfiguration config, ILogger<PaymentService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> CreatePaymentUrlAsync(HttpContext context, VnPaymentRequestModel model)
        {
            Console.WriteLine($"Creating payment URL for OrderId: {model.OrderId}, Amount: {model.Amount}");

            var txnRef = string.IsNullOrWhiteSpace(model.TxnRef)
                ? $"{model.OrderId}_{DateTime.Now:yyyyMMddHHmmss}"
                : model.TxnRef; // ưu tiên TxnRef từ service để đồng bộ DB
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());

            //vnpay.AddRequestData("vnp_Amount", ((int)(model.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));

            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang: {model.OrderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:PaymentBackReturnUrl"]);

            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);

            Console.WriteLine($"Generated payment URL: {paymentUrl}");

            return paymentUrl;
        }
       








        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var txnRef = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));

            // Try parse OrderId from TxnRef formats: ORD-{orderId}-... or {orderId}_timestamp
            int vnp_orderId = 0;
            var dashParts = (txnRef ?? string.Empty).Split('-');
            if (dashParts.Length >= 3 && string.Equals(dashParts[0], "ORD", StringComparison.OrdinalIgnoreCase))
            {
                _ = int.TryParse(dashParts[1], out vnp_orderId);
            }
            if (vnp_orderId == 0)
            {
                var underscoreParts = (txnRef ?? string.Empty).Split('_');
                _ = int.TryParse(underscoreParts.FirstOrDefault() ?? "0", out vnp_orderId);
            }
            if (vnp_orderId == 0 && !string.IsNullOrWhiteSpace(vnp_OrderInfo))
            {
                var digits = new string(vnp_OrderInfo.Where(char.IsDigit).ToArray());
                _ = int.TryParse(digits, out vnp_orderId);
            }
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false,
                    TxnRef = txnRef,
                    OrderId = vnp_orderId.ToString()
                };
            }

            return new VnPaymentResponseModel
            {
                Success = vnp_ResponseCode == "00",
                PaymentMethod = vnpay.GetResponseData("vnp_CardType") ?? "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId.ToString(),
                TxnRef = txnRef,
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                BankCode = vnp_BankCode,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }


 }


