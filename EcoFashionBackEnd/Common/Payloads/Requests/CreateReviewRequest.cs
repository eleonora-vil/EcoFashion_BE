namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateReviewRequest
    {
        public int? ProductId { get; set; }
        public int? MaterialId { get; set; }
        public string? Comment { get; set; }
        public required decimal RatingScore { get; set; }
    }
}
