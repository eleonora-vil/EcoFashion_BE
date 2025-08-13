namespace EcoFashionBackEnd.Dtos
{
    public class OrderDetailModel
    {
        // From OrderDetail
        public int OrderDetailId { get; set; }
        public required int OrderId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
        public required string ItemName { get; set; }
        public required string ProviderName { get; set; }
        public string? ImageUrl { get; set; }
        // From Design (when Type == design)
        public int? DesignId { get; set; }
        public Guid? DesignerId { get; set; }
        // From Material (when Type == material)
        public int? MaterialId { get; set; }
        public Guid? SupplierId { get; set; }
    }
}
