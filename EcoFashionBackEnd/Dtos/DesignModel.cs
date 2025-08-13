namespace EcoFashionBackEnd.Dtos
{
    public class DesignModel
    {
        public int DesignId { get; set; }
        public Guid DesignerId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public float RecycledPercentage { get; set; }
        public string? CareInstructions { get; set; }
        public decimal SalePrice { get; set; }
        public int ProductScore { get; set; }
        public string? Status { get; set; }
        public int? DesignTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

