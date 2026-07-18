using StadiumOps.Domain.Common;

namespace StadiumOps.Domain.Operations;

public sealed class Stadium : Entity
{
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public int Capacity { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string TimeZone { get; set; } = "America/New_York";
    public ICollection<Match> Matches { get; set; } = [];
    public ICollection<StadiumPointOfInterest> PointsOfInterest { get; set; } = [];
    public ICollection<CrowdZone> CrowdZones { get; set; } = [];
}

public sealed class Match : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public string Stage { get; set; } = "Group Stage";
    public string Status { get; set; } = "Scheduled";
}

public sealed class Seat : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public required string Section { get; set; }
    public required string Row { get; set; }
    public required string Number { get; set; }
    public bool IsAccessible { get; set; }
}

public sealed class StadiumPointOfInterest : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Level { get; set; }
    public required string Zone { get; set; }
    public bool IsAccessible { get; set; }
    public int EstimatedWaitMinutes { get; set; }
}

public sealed class NavigationRoute : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public required string FromLocation { get; set; }
    public required string ToLocation { get; set; }
    public int DistanceMeters { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsAccessible { get; set; }
    public int CrowdLoadPercent { get; set; }
    public string SafetyNote { get; set; } = "Follow venue staff instructions and posted emergency guidance.";
}

public sealed class CrowdZone : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public required string Name { get; set; }
    public int CurrentDensity { get; set; }
    public int MaximumCapacity { get; set; }
    public string Status { get; set; } = "Normal";
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class IncidentReport : Entity
{
    public Guid ReporterId { get; set; }
    public required string Category { get; set; }
    public required string Severity { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Normal";
    public string? AssignedTeam { get; set; }
}

public sealed class StadiumNotification : Entity
{
    public Guid? UserId { get; set; }
    public required string Type { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; }
    public string Priority { get; set; } = "Normal";
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class AiConversation : Entity
{
    public Guid UserId { get; set; }
    public required string Prompt { get; set; }
    public required string Response { get; set; }
    public string Intent { get; set; } = "General";
    public string AgentKey { get; set; } = "fan-assistant";
    public string AgentName { get; set; } = "Fan Assistant Agent";
    public decimal ConfidenceScore { get; set; }
    public bool EscalationRecommended { get; set; }
    public int TokensUsed { get; set; }
    public string Model { get; set; } = "unconfigured";
    public string PromptVersion { get; set; } = "stadiumops-ai-v1";
    public string GroundingSummary { get; set; } = "";
    public string SourcesJson { get; set; } = "[]";
    public string GuardrailsJson { get; set; } = "[]";
}

public sealed class AiKnowledgeDocument : Entity
{
    public required string Category { get; set; }
    public required string Title { get; set; }
    public required string SourceType { get; set; }
    public string? SourceUri { get; set; }
    public required string ContentSummary { get; set; }
    public string Language { get; set; } = "en";
    public bool IsApproved { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public sealed class AiAnalyticsEvent : Entity
{
    public Guid? UserId { get; set; }
    public string EventName { get; set; } = "AiConversationCompleted";
    public required string Intent { get; set; }
    public required string AgentKey { get; set; }
    public required string AgentName { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool EscalationRecommended { get; set; }
    public int LatencyMs { get; set; }
    public int TokensUsed { get; set; }
    public string MetadataJson { get; set; } = "{}";
}

public sealed class TransportStatus : Entity
{
    public required string Mode { get; set; }
    public required string Provider { get; set; }
    public required string Status { get; set; }
    public int EstimatedDelayMinutes { get; set; }
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SustainabilityMetric : Entity
{
    public Guid StadiumId { get; set; }
    public Stadium? Stadium { get; set; }
    public decimal EnergyKwh { get; set; }
    public decimal WaterLiters { get; set; }
    public decimal WasteKg { get; set; }
    public decimal RecyclingRate { get; set; }
    public decimal CarbonScore { get; set; }
    public DateOnly MetricDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

public sealed class VolunteerTask : Entity
{
    public Guid VolunteerUserId { get; set; }
    public required string Title { get; set; }
    public required string Location { get; set; }
    public string Status { get; set; } = "Assigned";
    public string Priority { get; set; } = "Normal";
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
}

public sealed class AuditLog : Entity
{
    public Guid? UserId { get; set; }
    public required string Action { get; set; }
    public required string Resource { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}

public sealed class IntegrationEventOutbox : Entity
{
    public required string EventType { get; set; }
    public required string AggregateType { get; set; }
    public Guid? AggregateId { get; set; }
    public required string PayloadJson { get; set; }
    public string PublishStatus { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
}
