namespace EcoFashionBackEnd.Dtos
{
    public class CreateDesignerMaterialInventoryRequest
    {
        public int MaterialId { get; set; }
        public int? Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}
