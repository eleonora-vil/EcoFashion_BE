using EcoFashionBackEnd.Common.Payloads.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public class CreateDesignRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public float RecycledPercentage { get; set; }
    public string? CareInstructions { get; set; }
    public decimal SalePrice { get; set; }
    public int ProductScore { get; set; }
    public string? Status { get; set; }
    public int? DesignTypeId { get; set; }

    public CreateDesignFeatureRequest Feature { get; set; } = new();
    [FromForm]
    public string MaterialsJson { get; set; } = string.Empty;
    //[{"materialId":4,"persentageUsed":40,"meterUsed":1.8},{"materialId":2,"persentageUsed":60,"meterUsed":2.2}]

    [FromForm]
    public List<IFormFile> ImageFiles { get; set; } = new();
    
}
