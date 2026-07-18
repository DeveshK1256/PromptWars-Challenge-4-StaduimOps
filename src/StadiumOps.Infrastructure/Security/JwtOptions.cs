using System.Text;

namespace StadiumOps.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "stadium-ops";
    public string Audience { get; set; } = "stadium-ops-web";
    public string? SigningKey { get; set; }
    public int AccessTokenMinutes { get; set; } = 20;
    public int RefreshTokenDays { get; set; } = 7;

    public byte[] ResolveSigningKey(bool isProduction)
    {
        if (!string.IsNullOrWhiteSpace(SigningKey))
        {
            if (SigningKey.Length < 32)
            {
                throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters (256 bits) long to prevent signature forgery.");
            }
            return Encoding.UTF8.GetBytes(SigningKey);
        }

        if (isProduction)
        {
            throw new InvalidOperationException("Jwt:SigningKey is required in Production.");
        }

        return Encoding.UTF8.GetBytes("development-only-change-this-key-before-production-32chars-minimum");
    }
}
