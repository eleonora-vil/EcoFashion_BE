namespace EcoFashionBackEnd.Dtos.User
{
    public class VerifyOTPRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OTPCode { get; set; } = string.Empty;
    }
}
