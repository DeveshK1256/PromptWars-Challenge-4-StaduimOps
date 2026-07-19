using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using StadiumOps.ApiTests.Infrastructure;
using Xunit;
using static StadiumOps.ApiTests.Infrastructure.TestHelpers;

namespace StadiumOps.ApiTests;

public sealed class SecurityIntegrationTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    [Fact]
    public async Task SqlInjection_AttemptOnIncidentCreation_IsMitigatedByParameterization()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "sqli");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 1. Create an incident with SQL injection payload in Category/Location
        var sqlPayload = "'; SELECT * FROM Users; --";
        var response = await client.PostAsJsonAsync("/api/v1/incidents", new
        {
            category = "Security",
            severity = "High",
            location = sqlPayload,
            description = "Attempting injection: " + sqlPayload
        });

        // 2. Assert EF Core parameterizes the query, executing successfully without query execution modifications
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var envelope = await ReadEnvelope<IncidentResponse>(response);
        Assert.NotNull(envelope.Data);
        Assert.Contains("Attempting injection", envelope.Data.Description);
    }

    [Fact]
    public async Task Cors_DisallowedOrigin_HeadersDoNotReflectEvilOrigin()
    {
        var client = factory.CreateClient();
        
        // Send a request pretending to be from an unauthorized origin
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/stadiums");
        request.Headers.Add("Origin", "http://evilattacker.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // Assert that CORS does not grant access to the unauthorized origin
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin");
            Assert.DoesNotContain("http://evilattacker.com", allowedOrigin);
        }
    }

    [Fact]
    public async Task RateLimiting_AiChatEndpoint_ExceedingLimitReturns429TooManyRequests()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "rate-limit");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Make 11 rapid requests (PermitLimit = 10, QueueLimit = 0)
        HttpResponseMessage? lastResponse = null;
        for (int i = 0; i < 11; i++)
        {
            lastResponse = await client.PostAsJsonAsync("/api/v1/ai/chat", new
            {
                prompt = "Hello AI"
            });

            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                break;
            }
        }

        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    private sealed record IncidentResponse(
        Guid Id,
        string Category,
        string Severity,
        string Priority,
        string Location,
        string Status,
        string? AssignedTeam,
        DateTimeOffset CreatedAt);
}
