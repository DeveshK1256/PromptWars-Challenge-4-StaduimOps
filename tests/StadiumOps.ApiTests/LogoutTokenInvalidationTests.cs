using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using StadiumOps.ApiTests.Infrastructure;
using static StadiumOps.ApiTests.Infrastructure.TestHelpers;
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

}
