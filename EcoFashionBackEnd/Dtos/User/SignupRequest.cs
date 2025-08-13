namespace EcoFashionBackEnd.Dtos.User
{
    public class SignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string fullname { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }
}
