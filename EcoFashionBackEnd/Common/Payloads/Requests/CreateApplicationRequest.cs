namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateApplicationRequest
    {
        public int TargetRoleId { get; set; }
        public string? PorfolioUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? SpecializationUrl { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Note { get; set; }
    }
}
