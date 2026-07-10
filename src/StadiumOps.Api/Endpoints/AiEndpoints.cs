using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StadiumOps.Api.Responses;
using StadiumOps.Application.AI;
using StadiumOps.Application.Abstractions;
using StadiumOps.Application.Events;
using StadiumOps.Application.Features;
using StadiumOps.Domain.Operations;
using StadiumOps.Infrastructure.Persistence;

namespace StadiumOps.Api.Endpoints;

public static class AiEndpoints
{
    public static RouteGroupBuilder MapAiEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/ai")
            .WithTags("AI Assistant")
            .RequireAuthorization();

        group.MapGet("/agents", AgentsAsync);
        group.MapGet("/knowledge", KnowledgeAsync);
        group.MapPost("/chat", ChatAsync);
        group.MapGet("/conversations", ConversationsAsync);

        return group;
    }

    private static async Task<IResult> ChatAsync(
        AiChatRequest request,
        IAiOrchestrator orchestrator,
        StadiumOpsDbContext dbContext,
        IAuditWriter auditWriter,
        IIntegrationEventOutboxWriter outboxWriter,
        ClaimsPrincipal principal,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return ApiResults.ValidationProblem(context, "Prompt is required.");
        }

        var userId = principal.GetUserId();
        if (userId is null)
        {
            return ApiResults.Unauthorized(context);
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
        var roles = principal.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct()
            .ToArray();
        var startedAt = System.Diagnostics.Stopwatch.GetTimestamp();

        try
        {
            var result = await orchestrator.GenerateAsync(
                new AiPromptContext(
                    userId.Value,
                    user?.PreferredLanguage ?? "en",
                    user?.AccessibilityPreference,
                    roles,
                    request.Prompt.Trim(),
                    request.Context),
                cancellationToken);
            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(startedAt);

            var conversation = new AiConversation
            {
                UserId = userId.Value,
                Prompt = request.Prompt.Trim(),
                Response = result.Text,
                Intent = result.Intent,
                AgentKey = result.AgentKey,
                AgentName = result.AgentName,
                ConfidenceScore = result.ConfidenceScore,
                EscalationRecommended = result.EscalationRecommended,
                Model = result.Model,
                TokensUsed = result.TokensUsed,
                PromptVersion = result.PromptVersion,
                GroundingSummary = result.GroundingSummary,
                SourcesJson = JsonSerializer.Serialize(result.KnowledgeSources),
                GuardrailsJson = JsonSerializer.Serialize(result.GuardrailNotes)
            };

            dbContext.AiConversations.Add(conversation);
            dbContext.AiAnalyticsEvents.Add(new AiAnalyticsEvent
            {
                UserId = userId,
                Intent = result.Intent,
                AgentKey = result.AgentKey,
                AgentName = result.AgentName,
                ConfidenceScore = result.ConfidenceScore,
                EscalationRecommended = result.EscalationRecommended,
                LatencyMs = (int)Math.Min(int.MaxValue, elapsed.TotalMilliseconds),
                TokensUsed = result.TokensUsed,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    result.PromptVersion,
                    Sources = result.KnowledgeSources,
                    Guardrails = result.GuardrailNotes
                })
            });
            auditWriter.Add(
                userId,
                "AiConversationCompleted",
                $"AiConversation:{conversation.Id}",
                result.Intent,
                context.Connection.RemoteIpAddress?.ToString(),
                context.GetCorrelationId());
            outboxWriter.Add(
                IntegrationEventNames.AiConversationCompleted,
                nameof(AiConversation),
                conversation.Id,
                new
                {
                    conversation.Id,
                    conversation.UserId,
                    conversation.Intent,
                    conversation.AgentKey,
                    conversation.ConfidenceScore,
                    conversation.EscalationRecommended,
                    conversation.Model,
                    conversation.TokensUsed
                },
                context.GetCorrelationId());
            await dbContext.SaveChangesAsync(cancellationToken);

            return ApiResults.Ok(context, new AiChatResponse(
                conversation.Id,
                conversation.Response,
                conversation.Intent,
                conversation.Model,
                conversation.TokensUsed,
                conversation.AgentName,
                conversation.ConfidenceScore,
                conversation.EscalationRecommended,
                result.KnowledgeSources));
        }
        catch (ExternalIntegrationNotConfiguredException ex)
        {
            return ApiResults.ServiceUnavailable(context, ex.Message);
        }
    }

    private static IResult AgentsAsync(HttpContext context)
    {
        var agents = AiAgentCatalog.All
            .Select(agent => new AiAgentResponse(
                agent.Key,
                agent.DisplayName,
                agent.Description,
                agent.Intents,
                agent.Responsibilities,
                agent.RequiresOperationalRole,
                agent.SafetyCritical))
            .ToArray();

        return ApiResults.Ok(context, agents);
    }

    private static async Task<IResult> KnowledgeAsync(
        StadiumOpsDbContext dbContext,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var documents = await dbContext.AiKnowledgeDocuments
            .Where(x => x.IsApproved)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .Select(x => new AiKnowledgeDocumentResponse(
                x.Id,
                x.Category,
                x.Title,
                x.SourceType,
                x.SourceUri,
                x.ContentSummary,
                x.Language,
                x.IsApproved))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, documents);
    }

    private static async Task<IResult> ConversationsAsync(
        StadiumOpsDbContext dbContext,
        ClaimsPrincipal principal,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return ApiResults.Unauthorized(context);
        }

        var conversations = await dbContext.AiConversations
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new AiChatResponse(
                x.Id,
                x.Response,
                x.Intent,
                x.Model,
                x.TokensUsed,
                x.AgentName,
                x.ConfidenceScore,
                x.EscalationRecommended,
                Array.Empty<string>()))
            .ToArrayAsync(cancellationToken);

        return ApiResults.Ok(context, conversations);
    }
}

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
