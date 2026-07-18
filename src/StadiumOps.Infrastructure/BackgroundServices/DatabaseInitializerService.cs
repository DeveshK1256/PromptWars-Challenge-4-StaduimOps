using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StadiumOps.Infrastructure.Identity;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Infrastructure.BackgroundServices;

public sealed class DatabaseInitializerService(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseInitializerService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Database initializer service starting...");
        using var scope = scopeFactory.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StadiumOpsDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            if (dbContext.Database.IsRelational())
            {
                logger.LogInformation("Applying pending migrations...");
                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            logger.LogInformation("Seeding initial enterprise data...");
            await SeedData.InitializeAsync(dbContext, roleManager);
            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
