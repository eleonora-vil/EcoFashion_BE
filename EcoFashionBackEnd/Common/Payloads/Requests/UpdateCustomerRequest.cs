namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class UpdateCustomerRequest
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
    }
}
