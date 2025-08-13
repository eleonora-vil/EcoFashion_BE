namespace EcoFashionBackEnd.Common.Payloads.Requests.DessignDraft
{
    public class DraftDesignUpdateRequest
    {
        public int DesignId { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public float RecycledPercentage { get; set; }

        public float TotalCarbon { get; set; }
        public float TotalWater { get; set; }
        public float TotalWaste { get; set; }

        public decimal LaborCostPerHour { get; set; }
        public float LaborHours { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int DesignTypeId { get; set; }

        public string DraftPartsJson { get; set; }
        public string MaterialsJson { get; set; }

        public List<IFormFile> SketchImages { get; set; }

    }

}
