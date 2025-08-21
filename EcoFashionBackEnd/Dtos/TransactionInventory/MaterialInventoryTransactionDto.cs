namespace EcoFashionBackEnd.Dtos.TransactionInventory
{
    public class MaterialInventoryTransactionDto
    {
        public int TransactionId { get; set; }

        public int InventoryId { get; set; }
        public string MaterialName { get; set; } = string.Empty;  // lấy từ MaterialInventory

        public int? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }  // nếu muốn hiển thị người thao tác

        public decimal QuantityChanged { get; set; }
        public decimal? BeforeQty { get; set; }
        public decimal? AfterQty { get; set; }

        public string TransactionType { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; }
    }

}
