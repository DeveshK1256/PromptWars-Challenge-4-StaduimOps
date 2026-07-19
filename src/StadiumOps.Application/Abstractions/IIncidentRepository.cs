using StadiumOps.Domain.Operations;

namespace StadiumOps.Application.Abstractions;

public interface IIncidentRepository
{
    Task AddAsync(IncidentReport incident, CancellationToken cancellationToken);
    Task<IncidentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<int> CountAsync(string? status, CancellationToken cancellationToken);
    Task<IncidentReport[]> ListAsync(int page, int pageSize, string? status, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
