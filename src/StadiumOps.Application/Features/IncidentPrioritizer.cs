namespace StadiumOps.Application.Features;

public static class IncidentPrioritizer
{
    public static string Prioritize(string category, string severity)
    {
        var normalized = $"{category} {severity}".ToLowerInvariant();
        if (normalized.Contains("fire") || normalized.Contains("critical") || normalized.Contains("lost child"))
        {
            return "Critical";
        }

        if (normalized.Contains("medical") || normalized.Contains("security") || normalized.Contains("high"))
        {
            return "High";
        }

        return "Normal";
    }

    public static string AssignTeam(string category)
    {
        var normalized = category.ToLowerInvariant();
        if (normalized.Contains("medical"))
        {
            return "Medical Team";
        }

        if (normalized.Contains("security") || normalized.Contains("lost child"))
        {
            return "Security Operations";
        }

        if (normalized.Contains("fire"))
        {
            return "Emergency Coordination";
        }

        return "Venue Operations";
    }
}
