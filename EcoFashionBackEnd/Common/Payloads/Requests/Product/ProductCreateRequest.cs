using EcoFashionBackEnd.Common.Payloads.Requests.Variant;

namespace EcoFashionBackEnd.Common.Payloads.Requests.Product
{
    public class ProductCreateRequest
    {
        public int DesignId { get; set; }
        public string CareInstruction { get; set; }
        public List<DesignsVariantCreateRequest> Variants { get; set; }
    }
}
