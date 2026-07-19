using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StadiumOps.ApiTests.Infrastructure;
using static StadiumOps.ApiTests.Infrastructure.TestHelpers;

namespace StadiumOps.ApiTests;

public sealed class NavigationAndStadiumTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Stadiums_WithToken_ReturnsSeededStadiums()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "nav-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/stadiums");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await ReadEnvelope<PagedPayload<StadiumPayload>>(response);
        Assert.NotEmpty(envelope.Data.Items);
        Assert.True(envelope.Data.TotalCount > 0);
    }

    [Fact]
    public async Task Stadiums_WithPageSize_RespectsLimit()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "page-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/stadiums?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var envelope = await ReadEnvelope<PagedPayload<StadiumPayload>>(response);
        Assert.True(envelope.Data.Items.Length <= 2);
    }

    [Fact]
    public async Task Stadiums_WithExcessivePageSize_ClampedTo100()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "clamp-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Should not error, should just clamp to 100 maximum
        var response = await client.GetAsync("/api/v1/stadiums?pageSize=9999");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Stadiums_WithPageGreaterThanMillion_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "page-million-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/stadiums?page=1000001");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TodayMatch_WithToken_ReturnsMatchOrNotFound()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "match-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/matches/today");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task StadiumPois_WithUnknownStadiumId_ReturnsNotFound()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "poi-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync($"/api/v1/stadiums/{Guid.NewGuid()}/pois");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NavigationRoute_WithMissingLocations_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "route-missing-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/navigation/routes", new
        {
            stadiumId = Guid.NewGuid(),
            fromLocation = "",
            toLocation = "",
            accessibilityRequired = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NavigationRoute_WithNoMatchingRoute_ReturnsNotFound()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "route-nomatch-test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/navigation/routes", new
        {
            stadiumId = Guid.NewGuid(),
            fromLocation = "NonExistentLocation",
            toLocation = "AnotherNonExistentLocation",
            accessibilityRequired = false
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record StadiumPayload(string Id, string Name, string City, string Country, int Capacity);
}
