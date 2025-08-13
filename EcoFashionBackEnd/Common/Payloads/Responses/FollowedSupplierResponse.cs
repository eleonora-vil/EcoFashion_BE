namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class FollowedSupplierResponse
    {
        public Guid SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? SpecializationUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }
    }
}
