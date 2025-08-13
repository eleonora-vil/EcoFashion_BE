namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class UpdateReviewRequest
    {
        public string? Comment { get; set; }
        public decimal? RatingScore { get; set; }
    }
}
