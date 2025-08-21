using EcoFashionBackEnd.Entities;
using System.Text.Json.Serialization;

namespace EcoFashionBackEnd.Dtos.Wallet
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public double BalanceBefore { get; set; }
        public double BalanceAfter { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionStatus? Status { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
