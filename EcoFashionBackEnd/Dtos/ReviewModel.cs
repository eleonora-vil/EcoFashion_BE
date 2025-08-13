namespace EcoFashionBackEnd.Dtos
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }
        public required int UserId { get; set; }
        public string? UserName { get; set; }
        public int? ProductId { get; set; }
        public int? MaterialId { get; set; }
        public string? Comment { get; set; }
        public required decimal RatingScore { get; set; }
    }
}
