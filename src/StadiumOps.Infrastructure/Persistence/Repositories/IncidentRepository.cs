using Microsoft.EntityFrameworkCore;
using StadiumOps.Application.Abstractions;
using StadiumOps.Domain.Operations;

namespace StadiumOps.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository(StadiumOpsDbContext dbContext) : IIncidentRepository
{
    public async Task AddAsync(IncidentReport incident, CancellationToken cancellationToken)
    {
        await dbContext.IncidentReports.AddAsync(incident, cancellationToken);
    }

    public async Task<IncidentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.IncidentReports.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.IncidentReports.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }
        return await query.CountAsync(cancellationToken);
    }

    public async Task<IncidentReport[]> ListAsync(int page, int pageSize, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.IncidentReports.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.Priority == "Critical")
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
