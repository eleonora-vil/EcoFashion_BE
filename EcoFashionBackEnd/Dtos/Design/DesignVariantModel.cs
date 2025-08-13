namespace EcoFashionBackEnd.Dtos.Design
{
    public class DesignVariantModel
    {
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }

        public float CarbonFootprint { get; set; }
        public float WaterUsage { get; set; }
        public float WasteDiverted { get; set; }
    }
}
