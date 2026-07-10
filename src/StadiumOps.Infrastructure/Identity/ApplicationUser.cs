using Microsoft.AspNetCore.Identity;

namespace StadiumOps.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = "";
    public string PreferredLanguage { get; set; } = "en";
    public string? AccessibilityPreference { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDisabled { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = "";
}

public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public required string TokenHash { get; set; }
    public string? DeviceName { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
