using EcoFashionBackEnd.Dtos.DesignDraft;
using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Common.Payloads.Requests.DessignDraft
{
    public class DraftDesignCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float RecycledPercentage { get; set; }
        public int DesignTypeId { get; set; }

        public decimal? UnitPrice { get; set; }
        public decimal? SalePrice { get; set; }
        public float? LaborHours { get; set; }
        public decimal? LaborCostPerHour { get; set; }

        public string DraftPartsJson { get; set; }
        public string MaterialsJson { get; set; }

        public float TotalCarbon { get; set; }
        public float TotalWater { get; set; }
        public float TotalWaste { get; set; }

        public List<IFormFile> SketchImages { get; set; } = new();

    }
}

//    [{ "name": "Thân áo", "length": 20, "width": 10, "quantity": 2, "materialId": 1,    },
//    {"name": "Tay áo", "length": 20, "width": 20, "quantity": 1, "materialId": 2,  }]


  //[ { "MaterialId":1, "PercentageUsed":50, "MeterUsed":2.5 },{ "MaterialId":2, "PercentageUsed":50, "MeterUsed":2.5 } ]