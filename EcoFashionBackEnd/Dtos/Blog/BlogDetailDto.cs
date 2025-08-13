namespace EcoFashionBackEnd.Dtos.Blog
{
    public class BlogDetailDto
    {
        public int BlogId { get; set; }
        public int UserID { get; set; }
        public required string UserName { get; set; }
        public required string AvatarUrl { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; }
    }
}
