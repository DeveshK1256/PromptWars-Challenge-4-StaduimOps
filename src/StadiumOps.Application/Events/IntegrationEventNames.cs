namespace StadiumOps.Application.Events;

public static class IntegrationEventNames
{
    public const string IncidentReported = "stadiumops.incident.reported.v1";
    public const string IncidentStatusChanged = "stadiumops.incident.status-changed.v1";
    public const string CrowdZoneUpdated = "stadiumops.crowd.zone-updated.v1";
    public const string NotificationBroadcastRequested = "stadiumops.notification.broadcast-requested.v1";
    public const string AiConversationCompleted = "stadiumops.ai.conversation-completed.v1";
    public const string TransportStatusUpdated = "stadiumops.transport.status-updated.v1";
    public const string VolunteerTaskAssigned = "stadiumops.volunteer.task-assigned.v1";
}

public sealed record IntegrationEventEnvelope(
    string EventType,
    string Source,
    string CorrelationId,
    DateTimeOffset OccurredAt,
    string PayloadJson,
    Guid? AggregateId = null,
    string? AggregateType = null);
