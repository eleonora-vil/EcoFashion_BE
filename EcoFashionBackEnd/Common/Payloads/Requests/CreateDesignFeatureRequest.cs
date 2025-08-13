namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateDesignFeatureRequest
    {
        public bool ReduceWaste { get; set; }
        public bool LowImpactDyes { get; set; }
        public bool Durable { get; set; }
        public bool EthicallyManufactured { get; set; }
    }

}
