using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using StadiumOps.ApiTests.Infrastructure;
using Xunit;

namespace StadiumOps.ApiTests;

public sealed class LogoutTokenInvalidationTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Logout_ShouldBlacklistJwtAccessToken()
    {
        var client = factory.CreateClient();
        
        // 1. Register a user and acquire the access token
        var auth = await RegisterAsync(client, "blacklist-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 2. Call /api/v1/auth/me (should succeed)
        var meResponseBefore = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponseBefore.StatusCode);

        // 3. Logout (which blacklists the access token)
        var logoutResponse = await client.PostAsJsonAsync("/api/v1/auth/logout", new
        {
            refreshToken = auth.RefreshToken
        });
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // 4. Call /api/v1/auth/me again with the same access token (should now return 401 Unauthorized)
        var meResponseAfter = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponseAfter.StatusCode);
    }

    private static async Task<Envelope<T>> ReadEnvelope<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<Envelope<T>>(stream, JsonOptions);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
        return payload;
    }

    private static async Task<AuthPayload> RegisterAsync(HttpClient client, string emailPrefix)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Blacklist Fan",
            email = $"{emailPrefix}-{Guid.NewGuid():N}@example.com",
            password = "Testing1234!@#", // Policy-compliant password
            preferredLanguage = "en",
            accessibilityPreference = "Wheelchair route",
            requestedRole = "RegisteredFan"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await ReadEnvelope<AuthPayload>(response);
        return envelope.Data;
    }

    private sealed record Envelope<T>(bool Success, T Data, string CorrelationId);
    private sealed record AuthPayload(string AccessToken, string RefreshToken, string AccessTokenExpiresAt, UserPayload User);
    private sealed record UserPayload(string Id, string Name, string Email, string PreferredLanguage, string[] Roles);
}
