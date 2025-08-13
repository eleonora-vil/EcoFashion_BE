namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateDesignerRequest
    {
        public int UserId { get; set; }
        public string? DesignerName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? PortfolioFiles { get; set; }
        public string? BannerUrl { get; set; }
        public string? SpecializationUrl { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Certificates { get; set; }
        public string? IdentificationPictureFront { get; set; }
        public string? IdentificationPictureBack { get; set; }
    }
}
