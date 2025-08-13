using EcoFashionBackEnd.Dtos;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class GetSuppliersResponse
    {
        public IEnumerable<SupplierModel> Suppliers { get; set; }
    }
}
