namespace StadiumOps.Application.AI;

public sealed record AiIntentDetection(
    string Intent,
    string AgentKey,
    decimal ConfidenceScore,
    bool EscalationRecommended,
    IReadOnlyCollection<string> MatchedSignals);

public static class AiIntentDetector
{
    public static AiIntentDetection Detect(string prompt, string? context)
    {
        var text = $"{prompt} {context}".ToLowerInvariant();
        var signals = new List<string>();

        if (Matches(text, signals, "emergency", "medical", "injured", "fire", "evacuation", "security", "lost child"))
        {
            return new("Emergency", AiAgentKeys.Emergency, 0.94m, true, signals);
        }

        if (Matches(text, signals, "wheelchair", "accessible", "accessibility", "elevator", "step-free", "screen reader"))
        {
            return new("Accessibility", AiAgentKeys.Accessibility, 0.91m, false, signals);
        }

        if (Matches(text, signals, "route", "gate", "seat", "section", "directions", "map", "entrance"))
        {
            return new("Navigation", AiAgentKeys.Navigation, 0.88m, false, signals);
        }

        if (Matches(text, signals, "crowd", "congestion", "density", "queue", "heatmap", "bottleneck"))
        {
            return new("Crowd", AiAgentKeys.Crowd, 0.86m, true, signals);
        }

        if (Matches(text, signals, "parking", "metro", "bus", "train", "shuttle", "traffic", "rideshare"))
        {
            return new("Transportation", AiAgentKeys.Transportation, 0.87m, false, signals);
        }

        if (Matches(text, signals, "waste", "recycling", "carbon", "energy", "water", "sustainable", "sustainability"))
        {
            return new("Sustainability", AiAgentKeys.Sustainability, 0.85m, false, signals);
        }

        if (Matches(text, signals, "volunteer", "shift", "task", "assignment", "check in"))
        {
            return new("Volunteer", AiAgentKeys.Volunteer, 0.84m, false, signals);
        }

        if (Matches(text, signals, "translate", "language", "spanish", "french", "arabic", "hindi", "mandarin"))
        {
            return new("Translation", AiAgentKeys.Translator, 0.83m, false, signals);
        }

        if (Matches(text, signals, "incident", "staff", "operations", "summary", "report", "deploy"))
        {
            return new("Operations", AiAgentKeys.Operations, 0.82m, true, signals);
        }

        return new("General", AiAgentKeys.FanAssistant, 0.72m, false, signals);
    }

    private static bool Matches(string text, List<string> signals, params string[] keywords)
    {
        var matched = false;
        foreach (var keyword in keywords)
        {
            if (!text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            signals.Add(keyword);
            matched = true;
        }

        return matched;
    }
}
