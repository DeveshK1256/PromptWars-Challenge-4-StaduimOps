namespace StadiumOps.Application.Features;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string? PreferredLanguage,
    string? AccessibilityPreference,
    string? RequestedRole);

public sealed record LoginRequest(string Email, string Password, string? DeviceName);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserProfileResponse User);

public sealed record UserProfileResponse(
    Guid Id,
    string Name,
    string Email,
    string PreferredLanguage,
    string? AccessibilityPreference,
    IReadOnlyCollection<string> Roles);

public sealed record StadiumSummaryResponse(
    Guid Id,
    string Name,
    string City,
    string Country,
    int Capacity,
    decimal Latitude,
    decimal Longitude);

public sealed record MatchSummaryResponse(
    Guid Id,
    string HomeTeam,
    string AwayTeam,
    DateTimeOffset StartsAt,
    string Stage,
    string Status,
    StadiumSummaryResponse Stadium);

public sealed record PointOfInterestResponse(
    Guid Id,
    string Name,
    string Category,
    string Level,
    string Zone,
    bool IsAccessible,
    int EstimatedWaitMinutes);

public sealed record NavigationRouteRequest(
    Guid StadiumId,
    string FromLocation,
    string ToLocation,
    bool AccessibilityRequired);

public sealed record NavigationRouteResponse(
    Guid Id,
    string FromLocation,
    string ToLocation,
    int DistanceMeters,
    int EstimatedMinutes,
    bool IsAccessible,
    int CrowdLoadPercent,
    string SafetyNote,
    string RecommendationNotice);

public sealed record AiChatRequest(string Prompt, string? Context);
public sealed record AiChatResponse(
    Guid ConversationId,
    string Response,
    string Intent,
    string Model,
    int TokensUsed,
    string AgentName,
    decimal ConfidenceScore,
    bool EscalationRecommended,
    IReadOnlyCollection<string> Sources);

public sealed record AiAgentResponse(
    string Key,
    string DisplayName,
    string Description,
    IReadOnlyCollection<string> Intents,
    IReadOnlyCollection<string> Responsibilities,
    bool RequiresOperationalRole,
    bool SafetyCritical);

public sealed record AiKnowledgeDocumentResponse(
    Guid Id,
    string Category,
    string Title,
    string SourceType,
    string? SourceUri,
    string ContentSummary,
    string Language,
    bool IsApproved);

public sealed record IncidentCreateRequest(
    string Category,
    string Severity,
    string Location,
    string Description);

public sealed record IncidentStatusRequest(string Status, string? AssignedTeam);

public sealed record IncidentResponse(
    Guid Id,
    string Category,
    string Severity,
    string Priority,
    string Location,
    string Status,
    string? AssignedTeam,
    DateTimeOffset CreatedAt);

public sealed record CrowdZoneResponse(
    Guid Id,
    string StadiumName,
    string Name,
    int CurrentDensity,
    int MaximumCapacity,
    string Status,
    DateTimeOffset LastUpdated);

public sealed record OperationsOverviewResponse(
    int StadiumCount,
    int MatchCount,
    int OpenIncidentCount,
    int CriticalIncidentCount,
    int CongestedZoneCount,
    int ActiveVolunteerTasks,
    decimal AverageSustainabilityScore,
    IReadOnlyCollection<string> AiInsightReadiness);

public sealed record BroadcastNotificationRequest(
    string Title,
    string Message,
    string Type,
    string Priority,
    string? DeviceToken);

public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    string Priority,
    bool IsRead,
    string ExternalDeliveryStatus);
