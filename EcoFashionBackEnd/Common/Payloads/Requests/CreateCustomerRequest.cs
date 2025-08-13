namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateCustomerRequest
    {
        public string? Email { get; set; }
        public string Password { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
    }
}