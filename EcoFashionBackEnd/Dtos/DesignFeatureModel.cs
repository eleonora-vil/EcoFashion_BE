namespace EcoFashionBackEnd.Dtos
{
    public class DesignFeatureModel
    {
        public int DesignId { get; set; }
        public bool ReduceWaste { get; set; }
        public int RecycledMaterials { get; set; }
        public bool LowImpactDyes { get; set; }
        public bool Durable { get; set; }
        public bool EthicallyManufactured { get; set; }
    }
}
