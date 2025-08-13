using EcoFashionBackEnd.Dtos;

namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class GetCustomerResponse
    {
        public IEnumerable<UserModel> Customers { get; set; }
    }
}
