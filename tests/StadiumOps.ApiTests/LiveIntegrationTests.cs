using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StadiumOps.Application.Abstractions;
using StadiumOps.Infrastructure.Integrations;
using Xunit;

namespace StadiumOps.ApiTests;

public sealed class LiveIntegrationTests
{
    private static readonly bool RunLiveTests = 
        string.Equals(Environment.GetEnvironmentVariable("RUN_LIVE_INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    [Fact]
    public async Task LiveVertexAiGemini_ShouldGenerateContent_IfConfigured()
    {
        if (!RunLiveTests)
        {
            // Skip the test dynamically if live testing is disabled
            return;
        }

        // 1. Load actual configuration
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var projectId = config["VertexAI:ProjectId"];
        var location = config["VertexAI:Location"] ?? "us-central1";
        var model = config["VertexAI:Model"] ?? "gemini-2.0-flash";

        Assert.False(string.IsNullOrWhiteSpace(projectId), "VertexAI:ProjectId is required to run live integration tests.");

        var options = Options.Create(new VertexAiOptions
        {
            ProjectId = projectId,
            Location = location,
            Model = model
        });

        var gateway = new VertexGeminiAssistantGateway(options);

        // 2. Act
        var result = await gateway.GenerateAsync(
            new AiAssistantRequest(
                Guid.NewGuid(),
                "en",
                "Hello, answer with 'OK' if you can read this.",
                null),
            CancellationToken.None);

        // 3. Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Text));
        Assert.Equal(model, result.Model);
        Assert.True(result.TokensUsed > 0);
    }

    [Fact]
    public async Task LiveFirebaseFcm_ShouldFailWithCorrectException_IfConfigMissing()
    {
        // Even if we are not running live tests, we can verify that the Firebase Cloud Messaging gateway
        // throws a descriptive exception when unconfigured, preventing silent failures.
        var options = Options.Create(new FirebaseOptions
        {
            ServiceAccountPath = "",
            ProjectId = ""
        });

        var gateway = new FirebaseNotificationGateway(options, null);

        var exception = await Assert.ThrowsAsync<ExternalIntegrationNotConfiguredException>(() =>
            gateway.SendAsync(
                new NotificationDispatchRequest(
                    "Test Title",
                    "Test Body",
                    "Normal",
                    "dummy-device-token"),
                CancellationToken.None));

        Assert.Equal("FirebaseCloudMessaging", exception.IntegrationName);
    }
}
