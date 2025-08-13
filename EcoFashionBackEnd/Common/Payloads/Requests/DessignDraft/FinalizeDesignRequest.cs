namespace EcoFashionBackEnd.Common.Payloads.Requests.DessignDraft
{
    public class FinalizeDesignRequest
    {
        public int DesignId { get; set; }
        public List<DesignMaterialRequest> Materials { get; set; }

        public bool ReduceWaste { get; set; }
        public bool LowImpactDyes { get; set; }
        public bool Durable { get; set; }
        public bool EthicallyManufactured { get; set; }

        public float TotalCarbon { get; set; }
        public float TotalWater { get; set; }
        public float TotalWaste { get; set; }
        public decimal? CustomSalePrice { get; set; }
    }
}
