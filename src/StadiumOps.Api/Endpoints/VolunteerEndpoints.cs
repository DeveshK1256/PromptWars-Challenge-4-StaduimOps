using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Extensions;
using StadiumOps.Api.Responses;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class VolunteerEndpoints
{
    public static RouteGroupBuilder MapVolunteerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1")
            .WithTags("Volunteer Management")
            .RequireAuthorization();

        group.MapGet("/volunteer/tasks", GetTasksAsync);
        group.MapPost("/volunteer/tasks", CreateTaskAsync).RequireAuthorization("OperationsAccess");
        group.MapPatch("/volunteer/tasks/{id:guid}", UpdateStatusAsync);

        return group;
    }

    private static async Task<IResult> GetTasksAsync(
        Guid? volunteerUserId,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var query = dbContext.VolunteerTasks.AsQueryable();
        if (volunteerUserId.HasValue)
        {
            query = query.Where(x => x.VolunteerUserId == volunteerUserId.Value);
        }

        var tasks = await query
            .OrderBy(x => x.StartsAt)
            .Select(x => new
            {
                x.Id,
                x.VolunteerUserId,
                x.Title,
                x.Location,
                x.Status,
                x.Priority,
                x.StartsAt,
                x.EndsAt
            })
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, tasks);
    }

    private static async Task<IResult> CreateTaskAsync(
        CreateVolunteerTaskRequest request,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Location))
        {
            return ApiResults.ValidationProblem(context, "Title and Location are required.");
        }

        var task = new VolunteerTask
        {
            VolunteerUserId = request.VolunteerUserId,
            Title = StadiumOps.Application.Security.InputSanitizer.Sanitize(request.Title),
            Location = StadiumOps.Application.Security.InputSanitizer.Sanitize(request.Location),
            Priority = StadiumOps.Application.Security.InputSanitizer.Sanitize(request.Priority ?? "Normal"),
            Status = "Assigned",
            StartsAt = request.StartsAt ?? DateTimeOffset.UtcNow,
            EndsAt = request.EndsAt ?? DateTimeOffset.UtcNow.AddHours(4)
        };

        dbContext.VolunteerTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Created(context, $"/api/v1/volunteers/tasks", new
        {
            task.Id,
            task.VolunteerUserId,
            task.Title,
            task.Location,
            task.Status,
            task.Priority,
            task.StartsAt,
            task.EndsAt
        });
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        UpdateVolunteerTaskStatusRequest request,
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ApiResults.ValidationProblem(context, "Status is required.");
        }

        var task = await dbContext.VolunteerTasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
        {
            return ApiResults.NotFound(context, "Volunteer task was not found.");
        }

        // Verify the user is either the assigned volunteer or has operations rights
        var currentUserId = context.User.GetUserId();
        if (task.VolunteerUserId != currentUserId && !context.User.IsInRole("OperationsCoordinator") && !context.User.IsInRole("Administrator"))
        {
            return ApiResults.Forbidden(context, "You are not authorized to update this task.");
        }

        task.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResults.Ok(context, new
        {
            task.Id,
            task.VolunteerUserId,
            task.Title,
            task.Location,
            task.Status,
            task.Priority,
            task.StartsAt,
            task.EndsAt
        });
    }
}

public sealed record CreateVolunteerTaskRequest(
    Guid VolunteerUserId,
    string Title,
    string Location,
    string? Priority,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt);

public sealed record UpdateVolunteerTaskStatusRequest(string Status);
