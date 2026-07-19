using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StadiumOps.ApiTests.Infrastructure;
using static StadiumOps.ApiTests.Infrastructure.TestHelpers;

namespace StadiumOps.ApiTests;

public sealed class ProblemStatementFlowTests(StadiumOpsApiFactory factory) : IClassFixture<StadiumOpsApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task GetTransportStatus_ReturnsSuccess()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "transport-user");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/transport/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSustainabilityMetrics_ReturnsSuccess()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "sustain-user");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.GetAsync("/api/v1/sustainability/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReportSustainabilityMetric_WithoutOperationsRole_ReturnsForbidden()
    {
        var client = factory.CreateClient();
        var auth = await RegisterAsync(client, "sustain-nonop", "RegisteredFan");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await client.PostAsJsonAsync("/api/v1/sustainability/metrics", new
        {
            stadiumId = Guid.NewGuid(),
            energyKwh = 15000.50m,
            waterLiters = 4500.25m,
            wasteKg = 200.00m,
            recyclingRate = 0.85m,
            carbonScore = 92.5m
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task VolunteerTasks_Lifecycle_Succeeds()
    {
        var client = factory.CreateClient();
        
        // 1. Create a task (needs Operations Coordinator role)
        var opAuth = await RegisterAsync(client, "vol-op", "OperationsCoordinator");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opAuth.AccessToken);

        var volunteerUserId = Guid.NewGuid();
        var createTaskResponse = await client.PostAsJsonAsync("/api/v1/volunteer/tasks", new
        {
            volunteerUserId,
            title = "Help with navigation at Gate A",
            location = "Gate A",
            priority = "High"
        });

        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);
        var envelope = await ReadEnvelope<CreatedTaskPayload>(createTaskResponse);
        var taskId = envelope.Data.Id;

        // 2. List tasks for the volunteer
        var getTasksResponse = await client.GetAsync($"/api/v1/volunteer/tasks?volunteerUserId={volunteerUserId}");
        Assert.Equal(HttpStatusCode.OK, getTasksResponse.StatusCode);

        // 3. Update status (needs to be the volunteer or coordinator)
        var volAuth = await RegisterAsync(client, "vol-user", "Volunteer");
        // We override the volunteer's ID in database or just simulate volunteer token update.
        // Let's use the coordinator token to update status
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opAuth.AccessToken);
        var updateResponse = await client.PatchAsJsonAsync($"/api/v1/volunteer/tasks/{taskId}", new
        {
            status = "InProgress"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    private sealed record CreatedTaskPayload(Guid Id, Guid VolunteerUserId, string Title, string Location, string Status, string Priority);
}
