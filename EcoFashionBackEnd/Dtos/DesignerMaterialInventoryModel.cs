namespace EcoFashionBackEnd.Dtos
{
    public class DesignerMaterialInventoryModel
    {
        public int InventoryId { get; set; }
        public Guid DesignerId { get; set; }
        public int MaterialId { get; set; }
        public int? Quantity { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = "out_of_stock";
        public DateTime LastBuyDate { get; set; }
        public DesignerModel? Designer { get; set; }
        public MaterialModel? Material { get; set; }
    }
}
