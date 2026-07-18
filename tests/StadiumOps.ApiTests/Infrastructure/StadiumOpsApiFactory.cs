using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace StadiumOps.ApiTests.Infrastructure;

public sealed class StadiumOpsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "InMemory",
                ["Jwt:Issuer"] = "stadium-ops-tests",
                ["Jwt:Audience"] = "stadium-ops-web",
                ["Jwt:SigningKey"] = "tests-only-change-this-key-before-production-32chars",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
            });
        });
    }
}
