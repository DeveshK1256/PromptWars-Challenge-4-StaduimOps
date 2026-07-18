using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class TransportEndpoints
{
    public static RouteGroupBuilder MapTransportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1")
            .WithTags("Match-day Transportation")
            .RequireAuthorization();

        group.MapGet("/transport/status", GetTransportStatusAsync);

        return group;
    }

    private static async Task<IResult> GetTransportStatusAsync(
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var statusList = await dbContext.TransportStatuses
            .OrderBy(x => x.Mode)
            .Select(x => new
            {
                x.Id,
                x.Mode,
                x.Provider,
                x.Status,
                x.EstimatedDelayMinutes,
                x.LastUpdated
            })
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, statusList);
    }
}
