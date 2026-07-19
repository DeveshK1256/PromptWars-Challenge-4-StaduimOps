using StadiumOps.Application.Features;

namespace StadiumOps.Application.Abstractions;

public interface IIncidentService
{
    Task<IncidentResponse> CreateIncidentAsync(
        IncidentCreateRequest request,
        Guid userId,
        string? ipAddress,
        string? correlationId,
        CancellationToken cancellationToken);

    Task<PagedEnvelope<IncidentResponse>> ListIncidentsAsync(
        int? page,
        int? pageSize,
        string? status,
        CancellationToken cancellationToken);

    Task<IncidentResponse?> UpdateIncidentStatusAsync(
        Guid id,
        IncidentStatusRequest request,
        Guid? userId,
        string? ipAddress,
        string? correlationId,
        CancellationToken cancellationToken);
}
