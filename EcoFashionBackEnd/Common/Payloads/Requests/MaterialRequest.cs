namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class MaterialRequest
    {
        public int TypeId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal materialSustainabilityCriteria1 { get; set; }
        public decimal materialSustainabilityCriteria2 { get; set; }
        public decimal materialSustainabilityCriteria3 { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? DocumentationUrl { get; set; }
    }
}
