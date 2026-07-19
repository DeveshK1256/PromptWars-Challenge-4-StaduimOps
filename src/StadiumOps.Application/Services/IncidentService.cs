using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Events;
using StadiumOps.Application.Features;
using StadiumOps.Domain.Operations;

namespace StadiumOps.Application.Services;

public sealed class IncidentService(
    IIncidentRepository repository,
    IAuditWriter auditWriter,
    IIntegrationEventOutboxWriter outboxWriter)
    : IIncidentService
{
    public async Task<IncidentResponse> CreateIncidentAsync(
        IncidentCreateRequest request,
        Guid userId,
        string? ipAddress,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var incident = new IncidentReport
        {
            ReporterId = userId,
            Category = Security.InputSanitizer.Sanitize(request.Category),
            Severity = Security.InputSanitizer.Sanitize(request.Severity),
            Location = Security.InputSanitizer.Sanitize(request.Location),
            Description = Security.InputSanitizer.Sanitize(request.Description),
            Priority = IncidentPrioritizer.Prioritize(request.Category, request.Severity),
            Status = "Open",
            AssignedTeam = IncidentPrioritizer.AssignTeam(request.Category)
        };

        await repository.AddAsync(incident, cancellationToken);
        
        auditWriter.Add(
            userId,
            "IncidentCreated",
            $"Incident:{incident.Id}",
            incident.Category,
            ipAddress,
            correlationId);

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
            correlationId);

        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(incident);
    }

    public async Task<PagedEnvelope<IncidentResponse>> ListIncidentsAsync(
        int? page,
        int? pageSize,
        string? status,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page ?? 1, 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var total = await repository.CountAsync(status, cancellationToken);
        var items = await repository.ListAsync(safePage, safePageSize, status, cancellationToken);

        var responses = items.Select(ToResponse).ToArray();

        return new PagedEnvelope<IncidentResponse>(
            responses,
            safePage,
            safePageSize,
            total,
            (int)Math.Ceiling(total / (double)safePageSize));
    }

    public async Task<IncidentResponse?> UpdateIncidentStatusAsync(
        Guid id,
        IncidentStatusRequest request,
        Guid? userId,
        string? ipAddress,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var incident = await repository.GetByIdAsync(id, cancellationToken);
        if (incident is null)
        {
            return null;
        }

        incident.Status = request.Status.Trim();
        incident.AssignedTeam = string.IsNullOrWhiteSpace(request.AssignedTeam)
            ? incident.AssignedTeam
            : request.AssignedTeam.Trim();

        auditWriter.Add(
            userId,
            "IncidentStatusUpdated",
            $"Incident:{incident.Id}",
            incident.Status,
            ipAddress,
            correlationId);

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
            correlationId);

        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(incident);
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
