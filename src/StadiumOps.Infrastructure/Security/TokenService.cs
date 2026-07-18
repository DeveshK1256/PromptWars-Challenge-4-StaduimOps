using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Features;
using StadiumOps.Infrastructure.Identity;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Infrastructure.Security;

public sealed class TokenService(
    StadiumOpsDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> jwtOptions,
    IClock clock)
{
    public async Task<AuthResponse> CreateTokenPairAsync(
        ApplicationUser user,
        string? deviceName,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var now = clock.UtcNow;
        var options = jwtOptions.Value;
        var accessTokenExpiresAt = now.AddMinutes(options.AccessTokenMinutes);
        var signingKey = new SymmetricSecurityKey(options.ResolveSigningKey(false));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.Name)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessTokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        var refreshToken = GenerateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            DeviceName = deviceName,
            CreatedByIp = ipAddress,
            CreatedAt = now,
            ExpiresAt = now.AddDays(options.RefreshTokenDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            accessTokenExpiresAt,
            new UserProfileResponse(
                user.Id,
                user.Name,
                user.Email ?? "",
                user.PreferredLanguage,
                user.AccessibilityPreference,
                roles.ToArray()));
    }

    public async Task<AuthResponse?> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var existing = await dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (existing is null || existing.User is null || !existing.IsActive || existing.User.IsDisabled)
        {
            return null;
        }

        var replacement = await CreateTokenPairAsync(existing.User, existing.DeviceName, ipAddress, cancellationToken);
        existing.RevokedAt = clock.UtcNow;
        existing.ReplacedByTokenHash = HashToken(replacement.RefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return replacement;
    }

    public async Task<bool> RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (existing is null || existing.RevokedAt is not null)
        {
            return false;
        }

        existing.RevokedAt = clock.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateRefreshToken()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
