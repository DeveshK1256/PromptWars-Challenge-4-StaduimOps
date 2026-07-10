using System.Text.Json;
using StadiumOps.Application.Abstractions;
using StadiumOps.Domain.Operations;

namespace StadiumOps.Infrastructure.Persistence;

public sealed class IntegrationEventOutboxWriter(StadiumOpsDbContext dbContext) : IIntegrationEventOutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Add(
        string eventType,
        string aggregateType,
        Guid? aggregateId,
        object payload,
        string? correlationId)
    {
        dbContext.IntegrationEventOutbox.Add(new IntegrationEventOutbox
        {
            EventType = eventType,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
            PublishStatus = "Pending",
            CorrelationId = correlationId
        });
    }
}
