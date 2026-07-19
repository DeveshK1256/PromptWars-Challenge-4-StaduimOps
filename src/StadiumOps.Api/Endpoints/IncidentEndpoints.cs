using System.Security.Claims;
using StadiumOps.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StadiumOps.Api.Hubs;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Features;

namespace StadiumOps.Api.Endpoints;

public static class IncidentEndpoints
{
    public static RouteGroupBuilder MapIncidentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/incidents")
            .WithTags("Incident Response")
            .RequireAuthorization();

        group.MapPost("/", CreateAsync);
        group.MapGet("/", ListAsync).RequireAuthorization("IncidentAccess");
        group.MapPatch("/{id:guid}/status", UpdateStatusAsync).RequireAuthorization("IncidentAccess");

        return group;
    }

    private static async Task<IResult> CreateAsync(
        IncidentCreateRequest request,
        ClaimsPrincipal principal,
        IIncidentService service,
        IHubContext<OperationsHub> hubContext,
        ILogger<IncidentEndpoints> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Category)
            || string.IsNullOrWhiteSpace(request.Severity)
            || string.IsNullOrWhiteSpace(request.Location)
            || string.IsNullOrWhiteSpace(request.Description))
        {
            logger.LogWarning("Create incident failed: validation error - missing category/severity/location/description.");
            return ApiResults.ValidationProblem(context, "Category, severity, location, and description are required.");
        }

        var userId = principal.GetUserId();
        if (userId is null)
        {
            logger.LogWarning("Create incident unauthorized access attempt.");
            return ApiResults.Unauthorized(context);
        }

        try
        {
            logger.LogInformation("Creating incident for user {UserId} with category {Category}.", userId, request.Category);
            var response = await service.CreateIncidentAsync(
                request,
                userId.Value,
                context.Connection.RemoteIpAddress?.ToString(),
                context.GetCorrelationId(),
                cancellationToken);

            await hubContext.Clients.Group("operations").SendAsync("IncidentCreated", response, cancellationToken);
            logger.LogInformation("Incident {Id} created successfully.", response.Id);
            return ApiResults.Created(context, $"/api/v1/incidents/{response.Id}", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating incident.");
            return ApiResults.InternalServerError(context, "An unexpected error occurred while reporting the incident.");
        }
    }

    private static async Task<IResult> ListAsync(
        int? page,
        int? pageSize,
        string? status,
        IIncidentService service,
        ILogger<IncidentEndpoints> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page ?? 1, 1);
        if (safePage > 1000000)
        {
            logger.LogWarning("List incidents failed: requested page {Page} exceeds maximum limit.", safePage);
            return ApiResults.ValidationProblem(context, "Page number is too large.");
        }
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        try
        {
            logger.LogInformation("Listing incidents: Page={Page}, PageSize={PageSize}, Status={Status}.", safePage, safePageSize, status);
            var result = await service.ListIncidentsAsync(safePage, safePageSize, status, cancellationToken);
            return ApiResults.Ok(context, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while listing incidents.");
            return ApiResults.InternalServerError(context, "An unexpected error occurred while listing the incidents.");
        }
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        IncidentStatusRequest request,
        ClaimsPrincipal principal,
        IIncidentService service,
        IHubContext<OperationsHub> hubContext,
        ILogger<IncidentEndpoints> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            logger.LogWarning("Update incident status failed: status is required.");
            return ApiResults.ValidationProblem(context, "Status is required.");
        }

        var userId = principal.GetUserId();
        try
        {
            logger.LogInformation("Updating status of incident {Id} to {Status} by user {UserId}.", id, request.Status, userId);
            var response = await service.UpdateIncidentStatusAsync(
                id,
                request,
                userId,
                context.Connection.RemoteIpAddress?.ToString(),
                context.GetCorrelationId(),
                cancellationToken);

            if (response is null)
            {
                logger.LogWarning("Incident {Id} not found for status update.", id);
                return ApiResults.NotFound(context, "Incident was not found.");
            }

            await hubContext.Clients.Group("operations").SendAsync("IncidentUpdated", response, cancellationToken);
            logger.LogInformation("Incident {Id} status updated successfully to {Status}.", id, response.Status);
            return ApiResults.Ok(context, response);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict updating incident {Id}.", id);
            return ApiResults.Conflict(context, "This incident has been modified by another operator. Please reload and try again.");
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database constraints failed while updating incident {Id}.", id);
            return ApiResults.BadRequest(context, "Database integrity or constraint validation failed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while updating status of incident {Id}.", id);
            return ApiResults.InternalServerError(context, "An unexpected error occurred while updating the incident status.");
        }
    }
}
