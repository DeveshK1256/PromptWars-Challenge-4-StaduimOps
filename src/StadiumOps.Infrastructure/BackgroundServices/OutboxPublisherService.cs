using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Infrastructure.BackgroundServices;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher service started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing outbox events.");
            }
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StadiumOpsDbContext>();

        var events = await dbContext.IntegrationEventOutbox
            .Where(x => x.PublishStatus == "Pending" && (x.NextAttemptAt == null || x.NextAttemptAt <= DateTimeOffset.UtcNow))
            .OrderBy(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} outbox events.", events.Count);

        foreach (var evt in events)
        {
            try
            {
                // In a real implementation, this would publish to Cloud Pub/Sub or a message broker.
                // For now, we mark events as published and log them.
                logger.LogInformation(
                    "Publishing outbox event {EventType} for aggregate {AggregateType}:{AggregateId}.",
                    evt.EventType, evt.AggregateType, evt.AggregateId);

                evt.PublishStatus = "Published";
                evt.PublishedAt = DateTimeOffset.UtcNow;
                evt.RetryCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish outbox event {EventType}.", evt.EventType);
                evt.RetryCount++;
                evt.LastError = ex.Message;
                evt.PublishStatus = evt.RetryCount >= 5 ? "DeadLetter" : "Pending";
                evt.NextAttemptAt = DateTimeOffset.UtcNow.AddSeconds(Math.Pow(2, evt.RetryCount) * 10);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
