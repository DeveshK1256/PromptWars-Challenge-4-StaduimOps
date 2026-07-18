namespace StadiumOps.Application.Abstractions;

public interface IAiAssistantGateway
{
    Task<AiAssistantResult> GenerateAsync(AiAssistantRequest request, CancellationToken cancellationToken);
}

public interface INotificationGateway
{
    Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, CancellationToken cancellationToken);
}

public sealed record AiAssistantRequest(
    Guid UserId,
    string PreferredLanguage,
    string Prompt,
    string? Context);

public sealed record AiAssistantResult(
    string Text,
    string Intent,
    string Model,
    int TokensUsed);

public sealed record NotificationDispatchRequest(
    string Title,
    string Body,
    string Priority,
    string? DeviceToken);

public sealed record NotificationDispatchResult(
    bool ExternalDeliveryAttempted,
    bool ExternalDeliverySucceeded,
    string Provider,
    string StatusMessage);

public sealed class ExternalIntegrationNotConfiguredException(string integrationName, string message)
    : InvalidOperationException(message)
{
    public string IntegrationName { get; } = integrationName;
}
