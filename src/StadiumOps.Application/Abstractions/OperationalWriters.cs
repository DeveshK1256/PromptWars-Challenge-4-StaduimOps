namespace StadiumOps.Application.Abstractions;

public interface IAuditWriter
{
    void Add(
        Guid? userId,
        string action,
        string resource,
        string? details,
        string? ipAddress,
        string? correlationId);
}

public interface IIntegrationEventOutboxWriter
{
    void Add(
        string eventType,
        string aggregateType,
        Guid? aggregateId,
        object payload,
        string? correlationId);
}
