using EcoFashionBackEnd.Dtos.DesignDraft;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class FabricUsageResponse
    {
        public int DesignId { get; set; }
        public string DesignName { get; set; }
        public List<MaterialFabricUsageDto> MaterialsUsage { get; set; } = new List<MaterialFabricUsageDto>();
    }
}
