using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using StadiumOps.ApiTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace StadiumOps.ApiTests;

public sealed class LoadAndPerformanceTests(StadiumOpsApiFactory factory, ITestOutputHelper output) 
    : IClassFixture<StadiumOpsApiFactory>
{
    [Fact]
    public async Task StadiumsEndpoint_LoadAndCacheVerification_Succeeds()
    {
        var client = factory.CreateClient();
        
        // 1. Register a test user and set header
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Load Tester",
            email = $"load-{Guid.NewGuid():N}@example.com",
            password = "Testing1234!@#",
            preferredLanguage = "en",
            accessibilityPreference = "",
            requestedRole = "RegisteredFan"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var authStream = await response.Content.ReadAsStreamAsync();
        var authDoc = await JsonDocument.ParseAsync(authStream);
        var token = authDoc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Cold Start Request (loads cache)
        var stopwatch = Stopwatch.StartNew();
        var coldResponse = await client.GetAsync("/api/v1/stadiums");
        stopwatch.Stop();
        var coldTimeMs = stopwatch.ElapsedMilliseconds;
        Assert.Equal(HttpStatusCode.OK, coldResponse.StatusCode);
        output.WriteLine($"Cold request duration: {coldTimeMs}ms");

        // 3. Concurrent Warm Requests (should hit memory cache)
        var taskCount = 20;
        var tasks = new Task<long>[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                var warmResponse = await client.GetAsync("/api/v1/stadiums");
                sw.Stop();
                Assert.Equal(HttpStatusCode.OK, warmResponse.StatusCode);
                return sw.ElapsedMilliseconds;
            });
        }

        var durations = await Task.WhenAll(tasks);
        long totalWarmTime = 0;
        foreach (var duration in durations)
        {
            totalWarmTime += duration;
        }
        var avgWarmTimeMs = totalWarmTime / taskCount;
        output.WriteLine($"Average warm request duration ({taskCount} concurrent requests): {avgWarmTimeMs}ms");

        // Cache hit validation: Warm cached queries must respond faster than DB cold queries.
        // We assert that the average warm response is fast (under 100ms on local test servers).
        Assert.True(avgWarmTimeMs < 100, $"Warm request averaged {avgWarmTimeMs}ms, which exceeds high-performance caching thresholds.");
    }
}
