using EcoFashionBackEnd.Dtos.Design;

namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class UpdateDesignVariantRequest
    {
        public int? Id { get; set; } // null nếu là variant mới
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int Quantity { get; set; }
        public float? CarbonFootprint { get; set; }
        public float? WaterUsage { get; set; }
        public float? WasteDiverted { get; set; }
    }
}
