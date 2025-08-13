namespace EcoFashionBackEnd.Dtos.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class UserInfo
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Username { get; set; }
    public string Role { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
