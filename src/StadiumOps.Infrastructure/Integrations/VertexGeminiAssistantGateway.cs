using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using StadiumOps.Application.Abstractions;

namespace StadiumOps.Infrastructure.Integrations;

public sealed class VertexGeminiAssistantGateway(IOptions<VertexAiOptions> options)
    : IAiAssistantGateway
{
    public async Task<AiAssistantResult> GenerateAsync(
        AiAssistantRequest request,
        CancellationToken cancellationToken)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.ProjectId))
        {
            throw new ExternalIntegrationNotConfiguredException(
                "VertexAI",
                "Vertex AI is not configured. Set VertexAI:ProjectId and Google Application Default Credentials before using AI chat.");
        }

        var prompt = $"""
        You are the FIFA World Cup 2026 Smart Stadium assistant.
        Answer in language: {request.PreferredLanguage}.
        Treat all emergency, security, and medical topics as guidance only and tell users to follow official venue staff and emergency responders.
        Do not invent live facts. If real-time data is unavailable, say so.

        Context:
        {request.Context ?? "No additional context supplied."}

        Fan or operator request:
        {request.Prompt}
        """;

        var client = new Client(project: config.ProjectId, location: config.Location, vertexAI: true);
        var content = new Content
        {
            Role = "user",
            Parts = [new Part { Text = prompt }]
        };

        var response = await client.Models.GenerateContentAsync(
            model: config.Model,
            contents: content,
            cancellationToken: cancellationToken);

        return new AiAssistantResult(
            response.Text ?? "No AI response text was returned.",
            InferIntent(request.Prompt),
            config.Model,
            response.UsageMetadata?.TotalTokenCount ?? 0);
    }

    private static string InferIntent(string prompt)
    {
        var normalized = prompt.ToLowerInvariant();
        if (normalized.Contains("route") || normalized.Contains("gate") || normalized.Contains("seat"))
        {
            return "Navigation";
        }

        if (normalized.Contains("emergency") || normalized.Contains("medical") || normalized.Contains("security"))
        {
            return "Emergency";
        }

        if (normalized.Contains("parking") || normalized.Contains("metro") || normalized.Contains("bus"))
        {
            return "Transportation";
        }

        return "General";
    }
}
