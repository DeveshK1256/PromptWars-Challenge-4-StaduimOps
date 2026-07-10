namespace StadiumOps.Application.AI;

public static class AiAgentKeys
{
    public const string FanAssistant = "fan-assistant";
    public const string Navigation = "smart-navigation";
    public const string Transportation = "transportation-intelligence";
    public const string Accessibility = "accessibility-assistant";
    public const string Crowd = "crowd-intelligence";
    public const string Operations = "operations-intelligence";
    public const string Sustainability = "sustainability-advisor";
    public const string Emergency = "emergency-response";
    public const string Volunteer = "volunteer-assistant";
    public const string Translator = "multilingual-translator";
}

public sealed record AiAgentDefinition(
    string Key,
    string DisplayName,
    string Description,
    IReadOnlyCollection<string> Intents,
    IReadOnlyCollection<string> Responsibilities,
    bool RequiresOperationalRole,
    bool SafetyCritical);

public static class AiAgentCatalog
{
    public static readonly IReadOnlyCollection<AiAgentDefinition> All =
    [
        new(
            AiAgentKeys.FanAssistant,
            "Fan Assistant Agent",
            "Answers stadium, match-day, food, merchandise, and lost-and-found questions.",
            ["General", "FanExperience", "Food", "Merchandise", "LostAndFound"],
            ["Stadium FAQs", "Match information", "Food recommendations", "Merchandise locations", "Lost and found assistance"],
            false,
            false),
        new(
            AiAgentKeys.Navigation,
            "Smart Navigation Agent",
            "Provides indoor, outdoor, accessible, and crowd-aware routing guidance.",
            ["Navigation", "Route", "SeatFinding"],
            ["Accessible routing", "Crowd-aware routes", "Multi-floor navigation", "Route recalculation"],
            false,
            false),
        new(
            AiAgentKeys.Transportation,
            "Transportation Intelligence Agent",
            "Recommends public transport, parking, rideshare, and shuttle options.",
            ["Transportation", "Parking", "Transit"],
            ["ETA guidance", "Delay predictions", "Best transport option"],
            false,
            false),
        new(
            AiAgentKeys.Accessibility,
            "Accessibility Assistant",
            "Prioritizes wheelchair-safe routes, accessible entrances, elevator guidance, and assistive experiences.",
            ["Accessibility", "AccessibleRoute"],
            ["Wheelchair routing", "Accessible entrances", "Elevator guidance", "Screen reader support"],
            false,
            false),
        new(
            AiAgentKeys.Crowd,
            "Crowd Intelligence Agent",
            "Analyzes crowd density, congestion risk, and alternate flow recommendations.",
            ["Crowd", "Congestion", "Heatmap"],
            ["Crowd buildup detection", "Alternate entrances", "Evacuation route support"],
            true,
            true),
        new(
            AiAgentKeys.Operations,
            "Operations Intelligence Agent",
            "Summarizes operational conditions, incidents, resource needs, and staff allocation.",
            ["Operations", "IncidentSummary", "Staffing"],
            ["Operational summaries", "Incident prioritization", "Staff allocation", "Daily reports"],
            true,
            true),
        new(
            AiAgentKeys.Sustainability,
            "Sustainability Advisor",
            "Explains energy, water, waste, recycling, and low-carbon fan recommendations.",
            ["Sustainability", "Carbon", "Waste"],
            ["Carbon tracking", "Eco recommendations", "Waste reduction suggestions"],
            false,
            false),
        new(
            AiAgentKeys.Emergency,
            "Emergency Response Agent",
            "Supports emergency guidance, responder escalation, and location-sharing instructions without replacing protocols.",
            ["Emergency", "Medical", "Security", "Evacuation"],
            ["Medical guidance", "Lost child support", "Fire/security escalation", "Evacuation guidance"],
            true,
            true),
        new(
            AiAgentKeys.Volunteer,
            "Volunteer Assistant",
            "Supports task assignment, shift reminders, incident reporting, and operational updates for volunteers.",
            ["Volunteer", "TaskAssignment", "Shift"],
            ["Task assignment", "Shift reminders", "Incident reporting", "Operational updates"],
            false,
            false),
        new(
            AiAgentKeys.Translator,
            "Multilingual Translator",
            "Translates text and announcements across tournament-supported languages.",
            ["Translation", "Language"],
            ["Text translation", "Announcement translation", "Multilingual assistance"],
            false,
            false)
    ];

    public static AiAgentDefinition ForIntent(string intent)
    {
        return All.FirstOrDefault(agent => agent.Intents.Contains(intent, StringComparer.OrdinalIgnoreCase))
            ?? All.First(agent => agent.Key == AiAgentKeys.FanAssistant);
    }
}
