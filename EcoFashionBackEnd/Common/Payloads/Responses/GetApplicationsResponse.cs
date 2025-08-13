using EcoFashionBackEnd.Dtos;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class GetApplicationsResponse
    {
        public IEnumerable<ApplicationModel> Applications { get; set; }
    }
}
