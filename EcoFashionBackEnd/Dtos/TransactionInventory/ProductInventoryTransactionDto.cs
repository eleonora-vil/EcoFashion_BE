namespace EcoFashionBackEnd.Dtos.TransactionInventory
{
    public class ProductInventoryTransactionDto
    {
        public int TransactionId { get; set; }

        public int InventoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;  // lấy từ ProductInventory

        public int? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }  // lấy từ User nếu có

        public decimal QuantityChanged { get; set; }
        public decimal? BeforeQty { get; set; }
        public decimal? AfterQty { get; set; }

        public DateTime TransactionDate { get; set; }

        public string TransactionType { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

}
