using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StadiumOps.Api.Responses;
using StadiumOps.Application.Features;
using StadiumOps.Infrastructure.Integrations;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class OperationsEndpoints
{
    public static RouteGroupBuilder MapOperationsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1")
            .WithTags("Operations Command Center")
            .RequireAuthorization("OperationsAccess");

        group.MapGet("/operations/overview", OverviewAsync);
        group.MapGet("/crowd/zones", CrowdZonesAsync);

        return group;
    }

    private static async Task<IResult> OverviewAsync(
        StadiumOpsDbContext dbContext,
        IOptions<VertexAiOptions> vertex,
        IOptions<FirebaseOptions> firebase,
        IOptions<MapsOptions> maps,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var stadiumCount = await dbContext.Stadiums.CountAsync(cancellationToken);
        var matchCount = await dbContext.Matches.CountAsync(cancellationToken);
        var openIncidentCount = await dbContext.IncidentReports.CountAsync(x => x.Status == "Open", cancellationToken);
        var criticalIncidentCount = await dbContext.IncidentReports.CountAsync(x => x.Priority == "Critical", cancellationToken);
        var congestedZoneCount = await dbContext.CrowdZones.CountAsync(x => x.Status == "Congested", cancellationToken);
        var activeVolunteerTasks = await dbContext.VolunteerTasks.CountAsync(x => x.Status != "Completed", cancellationToken);
        var sustainabilityScores = await dbContext.SustainabilityMetrics.Select(x => x.CarbonScore).ToArrayAsync(cancellationToken);

        var readiness = new List<string>
        {
            string.IsNullOrWhiteSpace(vertex.Value.ProjectId)
                ? "Vertex AI pending: set VertexAI:ProjectId and Google credentials."
                : "Vertex AI configured.",
            string.IsNullOrWhiteSpace(firebase.Value.ServiceAccountPath)
                ? "Firebase FCM pending: set Firebase:ServiceAccountPath."
                : "Firebase FCM configured.",
            string.IsNullOrWhiteSpace(maps.Value.BrowserApiKey)
                ? "Google Maps pending: set VITE_GOOGLE_MAPS_API_KEY for the web app."
                : "Google Maps key configured."
        };

        return ApiResults.Ok(context, new OperationsOverviewResponse(
            stadiumCount,
            matchCount,
            openIncidentCount,
            criticalIncidentCount,
            congestedZoneCount,
            activeVolunteerTasks,
            sustainabilityScores.Length == 0 ? 0 : sustainabilityScores.Average(),
            readiness));
    }

    private static async Task<IResult> CrowdZonesAsync(
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var zones = await dbContext.CrowdZones
            .Include(x => x.Stadium)
            .OrderByDescending(x => x.Status == "Congested")
            .ThenByDescending(x => x.CurrentDensity)
            .Select(x => new CrowdZoneResponse(
                x.Id,
                x.Stadium == null ? "Unknown stadium" : x.Stadium.Name,
                x.Name,
                x.CurrentDensity,
                x.MaximumCapacity,
                x.Status,
                x.LastUpdated))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, zones);
    }
}
