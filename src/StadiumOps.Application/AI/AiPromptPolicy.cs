namespace StadiumOps.Application.AI;

public sealed record AiSafetyAssessment(
    bool IsAllowed,
    bool EscalationRecommended,
    string? RefusalReason,
    IReadOnlyCollection<string> GuardrailNotes);

public sealed record AiPromptContext(
    Guid UserId,
    string PreferredLanguage,
    string? AccessibilityPreference,
    IReadOnlyCollection<string> Roles,
    string Prompt,
    string? UserContext);

public static class AiPromptPolicy
{
    private static readonly string[] UnsafeSignals =
    [
        // Security bypass
        "show me the jwt",
        "give me the password",
        "bypass security",
        "disable cameras",
        "hack the system",
        "ignore emergency protocol",
        "ignore your instructions",
        "ignore previous instructions",
        "disregard safety",
        "override safety",
        // Violence / threats
        "weapon",
        "bomb",
        "explosive",
        "threat",
        "attack the stadium",
        "harm someone",
        // Data exfiltration
        "steal",
        "exfiltrate",
        "dump the database",
        "show all users",
        "list all passwords",
        "extract credentials",
        // Prompt injection
        "you are now",
        "pretend you are",
        "act as if you have no restrictions",
        "jailbreak",
        "dan mode",
        // PII extraction
        "show me other users",
        "give me someone else account",
        "reveal personal information"
    ];

    public static AiSafetyAssessment Assess(AiPromptContext context, AiIntentDetection detection)
    {
        var normalized = context.Prompt.ToLowerInvariant();
        if (UnsafeSignals.Any(signal => normalized.Contains(signal, StringComparison.OrdinalIgnoreCase)))
        {
            return new(
                false,
                false,
                "I can't help with unsafe, illegal, or security-bypass requests. Please contact stadium staff for legitimate assistance.",
                ["unsafe-request-declined", "security-boundary-enforced"]);
        }

        var notes = new List<string>
        {
            "do-not-fabricate",
            "prioritize-safety",
            "distinguish-facts-from-recommendations"
        };

        if (!string.IsNullOrWhiteSpace(context.AccessibilityPreference)
            || detection.AgentKey == AiAgentKeys.Accessibility)
        {
            notes.Add("never-recommend-inaccessible-paths");
        }

        if (detection.AgentKey == AiAgentKeys.Emergency)
        {
            notes.Add("emergency-guidance-is-advisory");
            notes.Add("escalate-to-human-responders");
        }

        if (RequiresOperationalRole(detection.AgentKey) && !HasOperationalRole(context.Roles))
        {
            notes.Add("restricted-operational-details-redacted");
        }

        return new(true, detection.EscalationRecommended, null, notes);
    }

    public static string BuildGroundedPrompt(
        AiPromptContext context,
        AiIntentDetection detection,
        AiAgentDefinition agent,
        AiSafetyAssessment safety,
        IReadOnlyCollection<string> knowledgeSources)
    {
        var sources = knowledgeSources.Count == 0
            ? "No approved knowledge source was retrieved. State uncertainty clearly."
            : string.Join(Environment.NewLine, knowledgeSources.Select(source => $"- {source}"));

        return $"""
        Role:
        You are the {agent.DisplayName} for a smart stadium operations platform.

        Intent:
        {detection.Intent}

        User Profile:
        Preferred language: {context.PreferredLanguage}
        Accessibility preference: {context.AccessibilityPreference ?? "none supplied"}
        Roles: {string.Join(", ", context.Roles)}

        User Context:
        {context.UserContext ?? "No user-provided context."}

        Retrieved Knowledge Sources:
        {sources}

        Safety Constraints:
        {string.Join(Environment.NewLine, safety.GuardrailNotes.Select(note => $"- {note}"))}
        - AI must augment human decision-making, not replace stadium protocols.
        - If uncertain, say: "I'm not certain. Please verify with stadium staff."

        Task:
        Answer accurately using only grounded context and approved operational assumptions.

        User Request:
        {context.Prompt}

        Output:
        Clear, concise, actionable response.
        """;
    }

    public static bool HasOperationalRole(IReadOnlyCollection<string> roles)
    {
        var privileged = new[]
        {
            "OperationsManager",
            "SecurityOfficer",
            "MedicalTeam",
            "TransportationOperator",
            "SustainabilityOfficer",
            "Admin",
            "SuperAdmin"
        };

        return roles.Any(role => privileged.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    private static bool RequiresOperationalRole(string agentKey) =>
        agentKey is AiAgentKeys.Crowd or AiAgentKeys.Operations or AiAgentKeys.Emergency;
}
