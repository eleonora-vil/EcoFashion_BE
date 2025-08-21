namespace EcoFashionBackEnd.Dtos.Design
{
    public class DesignerPublicDto
    {
        public Guid DesignerId { get; set; }
        public string? DesignerName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? SpecializationUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? BannerUrl { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public string? Certificates { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
