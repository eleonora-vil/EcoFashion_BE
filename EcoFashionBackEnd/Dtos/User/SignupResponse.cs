namespace EcoFashionBackEnd.Dtos.User
{
    public class SignupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
