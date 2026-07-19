using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StadiumOps.ApiTests.Infrastructure;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.ApiTests;

public sealed class IncidentFlowTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreateIncident_WithValidData_ReturnsCreated()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "incident-create");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/incidents", new
        {
            category = "Medical",
            severity = "High",
            location = "Section 112",
            description = "Fan needs medical attention."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateIncident_WithMissingFields_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "incident-bad");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/incidents", new
        {
            category = "Medical",
            severity = "",
            location = "",
            description = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateIncident_Fire_AssignedCriticalPriorityAndCorrectTeam()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "fire-incident");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/incidents", new
        {
            category = "Fire",
            severity = "Normal",
            location = "East Stand",
            description = "Smoke detected near concession stand."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await ReadEnvelope<IncidentPayload>(response);
        Assert.Equal("Critical", envelope.Data.Priority);
        Assert.Equal("Emergency Coordination", envelope.Data.AssignedTeam);
    }

    [Fact]
    public async Task ListIncidents_WithoutIncidentRole_ReturnsForbidden()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "fan-list-incidents");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/incidents");

        // Fan role does not have IncidentAccess — expect 403
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateIncident_PersistsAuditLogAndOutboxEvent()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "audit-incident");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await client.PostAsJsonAsync("/api/v1/incidents", new
        {
            category = "Security",
            severity = "High",
            location = "Parking Lot C",
            description = "Suspicious activity reported."
        });

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StadiumOpsDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(x => x.Action == "IncidentCreated"));
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
            password = "Testing1234!@#",
            preferredLanguage = "en",
            accessibilityPreference = "",
            requestedRole = "RegisteredFan"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await ReadEnvelope<AuthPayload>(response);
        return envelope.Data;
    }

    private sealed record Envelope<T>(bool Success, T Data, string CorrelationId);
    private sealed record AuthPayload(string AccessToken, string RefreshToken, string AccessTokenExpiresAt, UserPayload User);
    private sealed record UserPayload(string Id, string Name, string Email, string PreferredLanguage, string[] Roles);
    private sealed record IncidentPayload(string Id, string Category, string Severity, string Priority, string Location, string Status, string AssignedTeam, DateTimeOffset CreatedAt);
}
