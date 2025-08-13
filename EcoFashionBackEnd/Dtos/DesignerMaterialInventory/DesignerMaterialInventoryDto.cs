using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Dtos.Material;

namespace EcoFashionBackEnd.Dtos.DesignerMaterialInventory
{
    public class DesignerMaterialInventoryDto
    {
        public int InventoryId { get; set; }
        public int MaterialId { get; set; }
        public int? Quantity { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; } = "out_of_stock";
        public DateTime LastBuyDate { get; set; }
        public DesignerPublicDto Designer { get; set; }
        public DesginerStoredMaterialsDto? Material { get; set; }
    }
}
