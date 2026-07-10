namespace StadiumOps.Application.AI;

public static class AiKnowledgeSourceSelector
{
    public static IReadOnlyCollection<string> SelectSources(AiIntentDetection detection)
    {
        return detection.AgentKey switch
        {
            AiAgentKeys.Navigation => ["Stadium maps", "Venue information", "Crowd zone snapshots", "Accessibility guides"],
            AiAgentKeys.Accessibility => ["Accessibility guides", "Stadium maps", "Medical services directory"],
            AiAgentKeys.Transportation => ["Transportation schedules", "Parking guidance", "Traffic partner feeds"],
            AiAgentKeys.Crowd => ["Crowd sensors", "Historical crowd patterns", "Event schedule"],
            AiAgentKeys.Operations => ["Incident reports", "Staffing plans", "Operations playbooks"],
            AiAgentKeys.Sustainability => ["Sustainability guidelines", "Energy and waste metrics", "Public transport guidance"],
            AiAgentKeys.Emergency => ["Emergency procedures", "Medical services directory", "Security policies"],
            AiAgentKeys.Volunteer => ["Volunteer handbook", "Task assignments", "Operational updates"],
            AiAgentKeys.Translator => ["Approved announcement text", "Language support policy"],
            _ => ["Stadium information", "FAQ content", "Match schedules"]
        };
    }
}
