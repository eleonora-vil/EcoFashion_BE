using System.Security.Cryptography;
using System.Text;

public static class GravatarHelper
{
    public static string GetGravatarUrl(string email, int size = 200)
    {
        var emailHash = MD5Hash(email.Trim().ToLower());
        return $"https://www.gravatar.com/avatar/{emailHash}?s={size}";
    }

    private static string MD5Hash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
