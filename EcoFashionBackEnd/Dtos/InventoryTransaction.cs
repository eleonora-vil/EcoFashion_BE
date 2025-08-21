namespace EcoFashionBackEnd.Dtos
{
    public class InventoryTransactionDto
    {
        public int TransactionId { get; set; }

        // "Product" hoặc "Material"
        public string InventoryType { get; set; } = string.Empty;

        public int InventoryId { get; set; }

        // Chỉ cần 1 trường tên, map dựa vào InventoryType
        public string? ItemName { get; set; } = string.Empty;

        public int? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }

        public decimal QuantityChanged { get; set; }
        public decimal? BeforeQty { get; set; }
        public decimal? AfterQty { get; set; }

        public string TransactionType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

}
