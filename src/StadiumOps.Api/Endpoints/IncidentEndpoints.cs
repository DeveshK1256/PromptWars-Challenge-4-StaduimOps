using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Events;
using StadiumOps.Application.Features;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Persistence;

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
        StadiumOpsDbContext dbContext,
        IAuditWriter auditWriter,
        IIntegrationEventOutboxWriter outboxWriter,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Category)
            || string.IsNullOrWhiteSpace(request.Severity)
            || string.IsNullOrWhiteSpace(request.Location)
            || string.IsNullOrWhiteSpace(request.Description))
        {
            return ApiResults.ValidationProblem(context, "Category, severity, location, and description are required.");
        }

        var userId = principal.GetUserId();
        if (userId is null)
        {
            return ApiResults.Unauthorized(context);
        }

        var incident = new IncidentReport
        {
            ReporterId = userId.Value,
            Category = request.Category.Trim(),
            Severity = request.Severity.Trim(),
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            Priority = IncidentPrioritizer.Prioritize(request.Category, request.Severity),
            Status = "Open",
            AssignedTeam = IncidentPrioritizer.AssignTeam(request.Category)
        };

        dbContext.IncidentReports.Add(incident);
        auditWriter.Add(
            userId,
            "IncidentCreated",
            $"Incident:{incident.Id}",
            $"{incident.Category} at {incident.Location}",
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        outboxWriter.Add(
            IntegrationEventNames.IncidentReported,
            nameof(IncidentReport),
            incident.Id,
            new
            {
                incident.Id,
                incident.Category,
                incident.Severity,
                incident.Priority,
                incident.Location,
                incident.Status,
                incident.AssignedTeam,
                incident.ReporterId
            },
            context.GetCorrelationId());

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResults.Created(context, $"/api/v1/incidents/{incident.Id}", ToResponse(incident));
    }

    private static async Task<IResult> ListAsync(
        int? page,
        int? pageSize,
        string? status,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page ?? 1, 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);
        var query = dbContext.IncidentReports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var incidents = await query
            .OrderByDescending(x => x.Priority == "Critical")
            .ThenByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => ToResponse(x))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, new PagedEnvelope<IncidentResponse>(
            incidents,
            safePage,
            safePageSize,
            total,
            (int)Math.Ceiling(total / (double)safePageSize)));
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        IncidentStatusRequest request,
        ClaimsPrincipal principal,
        StadiumOpsDbContext dbContext,
        IAuditWriter auditWriter,
        IIntegrationEventOutboxWriter outboxWriter,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResults.ValidationProblem(context, "Status is required.");
        }

        var incident = await dbContext.IncidentReports.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (incident is null)
        {
            return ApiResults.NotFound(context, "Incident was not found.");
        }

        incident.Status = request.Status.Trim();
        incident.AssignedTeam = string.IsNullOrWhiteSpace(request.AssignedTeam)
            ? incident.AssignedTeam
            : request.AssignedTeam.Trim();

        auditWriter.Add(
            principal.GetUserId(),
            "IncidentStatusUpdated",
            $"Incident:{incident.Id}",
            incident.Status,
            context.Connection.RemoteIpAddress?.ToString(),
            context.GetCorrelationId());
        outboxWriter.Add(
            IntegrationEventNames.IncidentStatusChanged,
            nameof(IncidentReport),
            incident.Id,
            new
            {
                incident.Id,
                incident.Status,
                incident.AssignedTeam
            },
            context.GetCorrelationId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Ok(context, ToResponse(incident));
    }

    private static IncidentResponse ToResponse(IncidentReport incident) => new(
        incident.Id,
        incident.Category,
        incident.Severity,
        incident.Priority,
        incident.Location,
        incident.Status,
        incident.AssignedTeam,
        incident.CreatedAt);
}
