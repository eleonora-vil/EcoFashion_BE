using EcoFashionBackEnd.Dtos;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class GetDesignersResponse
    {
        public IEnumerable<DesignerModel> Designers { get; set; }
    }
}
