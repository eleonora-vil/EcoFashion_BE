using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EcoFashionBackEnd.Helpers;

public static class JwtHelper
{
    public static ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }
    
    public static string? GetClaimValue(string token, string claimType)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.FindFirst(claimType)?.Value;
    }
}
