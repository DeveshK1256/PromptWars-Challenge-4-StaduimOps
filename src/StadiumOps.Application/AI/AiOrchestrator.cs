using StadiumOps.Application.Abstractions;

namespace StadiumOps.Application.AI;

public interface IAiOrchestrator
{
    Task<AiOrchestrationResult> GenerateAsync(AiPromptContext context, CancellationToken cancellationToken);
}

public sealed record AiOrchestrationResult(
    string Text,
    string Intent,
    string AgentKey,
    string AgentName,
    decimal ConfidenceScore,
    bool EscalationRecommended,
    string Model,
    int TokensUsed,
    IReadOnlyCollection<string> GuardrailNotes,
    IReadOnlyCollection<string> KnowledgeSources,
    string PromptVersion,
    string GroundingSummary);

public sealed class AiOrchestrator(IAiAssistantGateway gateway) : IAiOrchestrator
{
    public const string PromptVersion = "stadiumops-ai-v1";

    public async Task<AiOrchestrationResult> GenerateAsync(
        AiPromptContext context,
        CancellationToken cancellationToken)
    {
        var detection = AiIntentDetector.Detect(context.Prompt, context.UserContext);
        var agent = AiAgentCatalog.All.First(x => x.Key == detection.AgentKey);
        var safety = AiPromptPolicy.Assess(context, detection);
        var sources = AiKnowledgeSourceSelector.SelectSources(detection);
        var groundingSummary = $"Agent={agent.DisplayName}; Intent={detection.Intent}; Sources={string.Join(", ", sources)}";

        if (!safety.IsAllowed)
        {
            return new AiOrchestrationResult(
                safety.RefusalReason ?? "I can't help with that request.",
                detection.Intent,
                agent.Key,
                agent.DisplayName,
                0.98m,
                safety.EscalationRecommended,
                "policy-engine",
                0,
                safety.GuardrailNotes,
                sources,
                PromptVersion,
                groundingSummary);
        }

        var groundedPrompt = AiPromptPolicy.BuildGroundedPrompt(context, detection, agent, safety, sources);
        var result = await gateway.GenerateAsync(
            new AiAssistantRequest(
                context.UserId,
                context.PreferredLanguage,
                groundedPrompt,
                context.UserContext),
            cancellationToken);

        return new AiOrchestrationResult(
            result.Text,
            detection.Intent,
            agent.Key,
            agent.DisplayName,
            detection.ConfidenceScore,
            safety.EscalationRecommended || detection.EscalationRecommended || detection.ConfidenceScore < 0.70m,
            result.Model,
            result.TokensUsed,
            safety.GuardrailNotes,
            sources,
            PromptVersion,
            groundingSummary);
    }
}
