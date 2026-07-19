using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace StadiumOps.ApiTests.Infrastructure;

public static class TestHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<Envelope<T>> ReadEnvelope<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<Envelope<T>>(stream, JsonOptions);
        Assert.NotNull(payload);
        Assert.True(payload.Success);
        Assert.NotNull(payload.Data);
        return payload;
    }

    public static async Task<AuthPayload> RegisterAsync(
        HttpClient client, 
        string emailPrefix, 
        string role = "RegisteredFan")
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Integration Fan",
            email = $"{emailPrefix}-{Guid.NewGuid():N}@example.com",
            password = "Testing1234!@#",
            preferredLanguage = "en",
            accessibilityPreference = "Wheelchair route",
            requestedRole = role
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await ReadEnvelope<AuthPayload>(response);
        return envelope.Data;
    }
}

public sealed record Envelope<T>(bool Success, T Data, string CorrelationId);
public sealed record AuthPayload(string AccessToken, string RefreshToken, string AccessTokenExpiresAt, UserPayload User);
public sealed record UserPayload(string Id, string Name, string Email, string PreferredLanguage, string[] Roles);
public sealed record PagedPayload<T>(T[] Items, int Page, int PageSize, int TotalCount, int TotalPages);
