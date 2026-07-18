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

        var client = new Client(project: config.ProjectId, location: config.Location, vertexAI: true);
        var content = new Content
        {
            Role = "user",
            Parts = [new Part { Text = request.Prompt }]
        };

        var intentTask = DetectIntentSemanticallyAsync(client, config.Model, request.Prompt, cancellationToken);
        var responseTask = client.Models.GenerateContentAsync(
            model: config.Model,
            contents: content,
            cancellationToken: cancellationToken);

        await Task.WhenAll(intentTask, responseTask);

        var response = await responseTask;
        var intent = await intentTask;

        return new AiAssistantResult(
            response.Text ?? "No AI response text was returned.",
            intent,
            config.Model,
            response.UsageMetadata?.TotalTokenCount ?? 0);
    }

    private async Task<string> DetectIntentSemanticallyAsync(
        Client client,
        string model,
        string prompt,
        CancellationToken cancellationToken)
    {
        try
        {
            var classificationPrompt = $"""
            Classify the user prompt below into exactly one of the following intent categories:
            - Emergency
            - Accessibility
            - Navigation
            - Crowd
            - Transportation
            - Sustainability
            - Volunteer
            - Translation
            - Operations
            - General

            User prompt: "{prompt}"

            Respond with ONLY the category name. Do not include formatting, punctuation, or extra words.
            """;

            var content = new Content
            {
                Role = "user",
                Parts = [new Part { Text = classificationPrompt }]
            };

            var response = await client.Models.GenerateContentAsync(
                model: model,
                contents: content,
                cancellationToken: cancellationToken);

            var intent = response.Text?.Trim() ?? "General";
            var validIntents = new[] { "Emergency", "Accessibility", "Navigation", "Crowd", "Transportation", "Sustainability", "Volunteer", "Translation", "Operations", "General" };
            return validIntents.FirstOrDefault(x => x.Equals(intent, StringComparison.OrdinalIgnoreCase)) ?? "General";
        }
        catch
        {
            return InferIntent(prompt);
        }
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
