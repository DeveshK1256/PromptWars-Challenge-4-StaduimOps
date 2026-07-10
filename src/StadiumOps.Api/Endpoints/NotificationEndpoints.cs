using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Events;
using StadiumOps.Application.Features;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", ListAsync);
        group.MapPost("/broadcast", BroadcastAsync).RequireAuthorization("OperationsAccess");

        return group;
    }

    private static async Task<IResult> ListAsync(
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();
        if (userId is null)
        {
            return ApiResults.Unauthorized(context);
        }

        var notifications = await dbContext.Notifications
            .Where(x => x.UserId == userId || x.UserId == null)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .Select(x => new NotificationResponse(
                x.Id,
                x.Title,
                x.Message,
                x.Type,
                x.Priority,
                x.IsRead,
                "in_app"))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, notifications);
    }

    private static async Task<IResult> BroadcastAsync(
        BroadcastNotificationRequest request,
        INotificationGateway gateway,
        StadiumOpsDbContext dbContext,
        IAuditWriter auditWriter,
        IIntegrationEventOutboxWriter outboxWriter,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            return ApiResults.ValidationProblem(context, "Title and message are required.");
        }

        var notification = new StadiumNotification
        {
            Type = string.IsNullOrWhiteSpace(request.Type) ? "Operations" : request.Type.Trim(),
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim()
        };
        dbContext.Notifications.Add(notification);

        NotificationDispatchResult dispatch;
        try
        {
            dispatch = await gateway.SendAsync(
                new NotificationDispatchRequest(
                    notification.Title,
                    notification.Message,
                    notification.Priority,
                    request.DeviceToken),
                cancellationToken);
        }
        catch (ExternalIntegrationNotConfiguredException ex)
        {
            dispatch = new NotificationDispatchResult(
                true,
                false,
                ex.IntegrationName,
                ex.Message);
        }

        auditWriter.Add(
            context.User.GetUserId(),
            "NotificationBroadcastRequested",
            $"Notification:{notification.Id}",
            $"{notification.Type}:{notification.Priority}",
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        outboxWriter.Add(
            IntegrationEventNames.NotificationBroadcastRequested,
            nameof(StadiumNotification),
            notification.Id,
            new
            {
                notification.Id,
                notification.Type,
                notification.Title,
                notification.Priority,
                dispatch.ExternalDeliveryAttempted,
                dispatch.ExternalDeliverySucceeded,
                dispatch.Provider,
                dispatch.StatusMessage
            },
            context.GetCorrelationId());

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Ok(context, new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.Priority,
            notification.IsRead,
            dispatch.StatusMessage));
    }
}
