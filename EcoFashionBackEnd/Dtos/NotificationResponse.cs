namespace EcoFashionBackEnd.Dtos
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // success, error, info, warning
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RelatedId { get; set; }
        public string? RelatedType { get; set; }
    }
}
