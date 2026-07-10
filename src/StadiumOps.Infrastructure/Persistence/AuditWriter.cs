using StadiumOps.Application.Abstractions;
using StadiumOps.Domain.Operations;

namespace StadiumOps.Infrastructure.Persistence;

public sealed class AuditWriter(StadiumOpsDbContext dbContext) : IAuditWriter
{
    public void Add(
        Guid? userId,
        string action,
        string resource,
        string? details,
        string? ipAddress,
        string? correlationId)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            Details = details,
            IpAddress = ipAddress,
            CorrelationId = correlationId
        });
    }
}
