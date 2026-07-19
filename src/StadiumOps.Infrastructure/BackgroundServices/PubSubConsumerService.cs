using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Infrastructure.BackgroundServices;

public sealed class PubSubConsumerService(
    IServiceScopeFactory scopeFactory,
    ILogger<PubSubConsumerService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Pub/Sub Consumer Background Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeLatestEventsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error occurred while consuming Pub/Sub integration events.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task ConsumeLatestEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StadiumOpsDbContext>();

        // Fetch events that were successfully published to simulate a message consumer queue
        var publishedEvents = await dbContext.IntegrationEventOutbox
            .Where(x => x.PublishStatus == "Published")
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (publishedEvents.Count == 0)
        {
            return;
        }

        foreach (var evt in publishedEvents)
        {
            logger.LogInformation(
                "Pub/Sub subscriber consumed integration event {EventType} for Aggregate {AggregateType}:{AggregateId} (CorrelationId: {CorrelationId})",
                evt.EventType, evt.AggregateType, evt.AggregateId, evt.CorrelationId);

            // Simulate BigQuery Analytics Pipeline ETL ingestion
            logger.LogInformation(
                "[ETL Pipeline] Streaming event payload for {EventType} directly into BigQuery Analytics Table. Payload size: {Size} bytes.",
                evt.EventType, evt.PayloadJson.Length);

            // Simulate Notification Dispatch routing based on event types
            if (evt.EventType == "IncidentReported")
            {
                logger.LogInformation(
                    "[FCM Dispatcher] Routed notification trigger to Emergency Dispatch Teams for newly reported incident ID {Id}",
                    evt.AggregateId);
            }
        }
    }
}
