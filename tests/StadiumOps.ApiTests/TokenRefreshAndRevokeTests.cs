using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using StadiumOps.ApiTests.Infrastructure;

namespace StadiumOps.ApiTests;

public sealed class TokenRefreshAndRevokeTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "refresh-valid");

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = auth.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await ReadEnvelope<AuthPayload>(response);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data.AccessToken));
        Assert.NotEqual(auth.AccessToken, envelope.Data.AccessToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = "this-is-not-a-valid-refresh-token"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithEmptyToken_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_RevokesSession()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "logout-valid");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/auth/logout", new
        {
            refreshToken = auth.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Trying to refresh with the revoked token should now fail
        client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsUserProfile()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "me-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await ReadEnvelope<UserProfilePayload>(response);
        Assert.Contains("RegisteredFan", envelope.Data.Roles);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var email = $"wrong-pw-{Guid.NewGuid():N}@example.com";

        // Register first
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Test Fan",
            email,
            password = "Testing1234",
            preferredLanguage = "en",
            accessibilityPreference = "",
            requestedRole = "RegisteredFan"
        });

        // Try logging in with wrong password
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "WrongPassword999"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<Envelope<T>> ReadEnvelope<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<Envelope<T>>(stream, JsonOptions);
        Assert.NotNull(payload);
        return payload;
    }

    private static async Task<AuthPayload> RegisterAsync(HttpClient client, string prefix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Test Fan",
            email = $"{prefix}-{Guid.NewGuid():N}@example.com",
            password = "Testing1234",
            preferredLanguage = "en",
            accessibilityPreference = "",
            requestedRole = "RegisteredFan"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await ReadEnvelope<AuthPayload>(response);
        return envelope.Data;
    }

    [Fact]
    public async Task Me_WithExpiredToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("tests-only-change-this-key-before-production-32chars"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "expired@example.com")
        };

        var expiredToken = new JwtSecurityToken(
            issuer: "stadium-ops-tests",
            audience: "stadium-ops-web",
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-20),
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(expiredToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record Envelope<T>(bool Success, T Data, string CorrelationId);
    private sealed record AuthPayload(string AccessToken, string RefreshToken, string AccessTokenExpiresAt, UserPayload User);
    private sealed record UserPayload(string Id, string Name, string Email, string PreferredLanguage, string[] Roles);
    private sealed record UserProfilePayload(string Id, string Name, string Email, string PreferredLanguage, string? AccessibilityPreference, string[] Roles);
}
