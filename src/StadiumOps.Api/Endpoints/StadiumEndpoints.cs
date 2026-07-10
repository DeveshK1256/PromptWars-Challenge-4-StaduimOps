using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Features;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class StadiumEndpoints
{
    public static RouteGroupBuilder MapStadiumEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1")
            .WithTags("Fan and Navigation")
            .RequireAuthorization();

        group.MapGet("/matches/today", TodayMatchAsync);
        group.MapGet("/stadiums", StadiumsAsync);
        group.MapGet("/stadiums/{id:guid}/pois", PointsOfInterestAsync);
        group.MapPost("/navigation/routes", RouteAsync);

        return group;
    }

    private static async Task<IResult> TodayMatchAsync(
        StadiumOpsDbContext dbContext,
        IClock clock,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var today = clock.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var match = await dbContext.Matches
            .Include(x => x.Stadium)
            .Where(x => x.StartsAt >= today && x.StartsAt < tomorrow)
            .OrderBy(x => x.StartsAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await dbContext.Matches
                .Include(x => x.Stadium)
                .OrderBy(x => x.StartsAt)
                .FirstOrDefaultAsync(cancellationToken);

        if (match?.Stadium is null)
        {
            return ApiResults.NotFound(context, "No match data is available.");
        }

        return ApiResults.Ok(context, new MatchSummaryResponse(
            match.Id,
            match.HomeTeam,
            match.AwayTeam,
            match.StartsAt,
            match.Stage,
            match.Status,
            new StadiumSummaryResponse(
                match.Stadium.Id,
                match.Stadium.Name,
                match.Stadium.City,
                match.Stadium.Country,
                match.Stadium.Capacity,
                match.Stadium.Latitude,
                match.Stadium.Longitude)));
    }

    private static async Task<IResult> StadiumsAsync(
        int? page,
        int? pageSize,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(page ?? 1, 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);
        var total = await dbContext.Stadiums.CountAsync(cancellationToken);
        var items = await dbContext.Stadiums
            .OrderBy(x => x.Name)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => new StadiumSummaryResponse(x.Id, x.Name, x.City, x.Country, x.Capacity, x.Latitude, x.Longitude))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, new PagedEnvelope<StadiumSummaryResponse>(
            items,
            safePage,
            safePageSize,
            total,
            (int)Math.Ceiling(total / (double)safePageSize)));
    }

    private static async Task<IResult> PointsOfInterestAsync(
        Guid id,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.Stadiums.AnyAsync(x => x.Id == id, cancellationToken);
        if (!exists)
        {
            return ApiResults.NotFound(context, "Stadium was not found.");
        }

        var pois = await dbContext.PointsOfInterest
            .Where(x => x.StadiumId == id)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .Select(x => new PointOfInterestResponse(
                x.Id,
                x.Name,
                x.Category,
                x.Level,
                x.Zone,
                x.IsAccessible,
                x.EstimatedWaitMinutes))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, pois);
    }

    private static async Task<IResult> RouteAsync(
        NavigationRouteRequest request,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FromLocation) || string.IsNullOrWhiteSpace(request.ToLocation))
        {
            return ApiResults.ValidationProblem(context, "FromLocation and ToLocation are required.");
        }

        var query = dbContext.NavigationRoutes
            .Where(x => x.StadiumId == request.StadiumId
                && x.FromLocation == request.FromLocation
                && x.ToLocation == request.ToLocation);

        if (request.AccessibilityRequired)
        {
            query = query.Where(x => x.IsAccessible);
        }

        var route = await query
            .OrderBy(x => x.CrowdLoadPercent)
            .ThenBy(x => x.EstimatedMinutes)
            .FirstOrDefaultAsync(cancellationToken);

        if (route is null)
        {
            return ApiResults.NotFound(context, "No safe route matched the requested locations and accessibility requirements.");
        }

        return ApiResults.Ok(context, new NavigationRouteResponse(
            route.Id,
            route.FromLocation,
            route.ToLocation,
            route.DistanceMeters,
            route.EstimatedMinutes,
            route.IsAccessible,
            route.CrowdLoadPercent,
            route.SafetyNote,
            "Recommendation only. Always follow official stadium staff, signage, and emergency responders."));
    }
}
