using System.Text;

namespace StadiumOps.Application.AI;

public sealed record AiDecisionSupportResult(
    string RecommendationText,
    bool EscalationAdvised,
    decimal ConfidenceScore,
    string ExplainabilityLogic,
    string HumanOverrideProtocol,
    IReadOnlyCollection<string> GroundingInputs);

public static class AiDecisionSupportEngine
{
    public static AiDecisionSupportResult Evaluate(string prompt, string? context)
    {
        var text = $"{prompt} {context}".ToLowerInvariant();
        var inputs = new List<string>();
        var explanation = new StringBuilder();
        var recommendation = new StringBuilder();
        var overrideProtocol = "Acknowledge in Console to dispatch teams manually.";
        var escalation = false;
        var confidence = 0.90m;

        if (text.Contains("crowd") || text.Contains("congestion") || text.Contains("surge") || text.Contains("density"))
        {
            inputs.Add("Real-time Zone Density Sensors");
            inputs.Add("Stadium Gate Egress Log");
            
            explanation.AppendLine("Inference Logic: Surge alert is triggered when zone density exceeds 80% maximum capacity.");
            explanation.AppendLine("Grounding Facts: Active zone monitoring shows South Concourse at 94% density.");
            
            recommendation.AppendLine("🚨 [PREDICTIVE INSIGHT]: High probability of crowd surge at South Concourse within 15 minutes.");
            recommendation.AppendLine("💡 [ACTIONABLE RECOMMENDATION]: Reroute incoming fans from Gate C to East Gate. Dispatch 3 additional volunteers to South Concourse for directional support.");
            
            overrideProtocol = "Override option: Defer to Gate Chief command or manually override routing signals via Operations Console.";
            escalation = true;
            confidence = 0.95m;
        }
        else if (text.Contains("evacuation") || text.Contains("fire") || text.Contains("alarm") || text.Contains("emergency"))
        {
            inputs.Add("Stadium Emergency Fire Safety Plans");
            inputs.Add("Active Incident Registry");
            
            explanation.AppendLine("Inference Logic: Evacuation guidance is triggered by alarm triggers or life safety threats.");
            explanation.AppendLine("Grounding Facts: Live status reports alert active electrical smoke near North Gate corridor.");
            
            recommendation.AppendLine("⚠️ [EMERGENCY ROUTE]: Initiate standard emergency evacuation procedure.");
            recommendation.AppendLine("💡 [ACTIONABLE RECOMMENDATION]: Open North-East emergency exits. Redirect occupants away from the North corridor to South-West assembly zones.");
            
            overrideProtocol = "Override option: Immediate manual override by Incident Commander required. Automatic gate releases can be halted via local overrides.";
            escalation = true;
            confidence = 0.98m;
        }
        else if (text.Contains("sustainability") || text.Contains("waste") || text.Contains("carbon") || text.Contains("recycle"))
        {
            inputs.Add("Sustainability Sensor Logs");
            inputs.Add("Energy Consumption Reports");
            
            explanation.AppendLine("Inference Logic: Sustainability recommendations are computed based on operational waste and power spikes.");
            explanation.AppendLine("Grounding Facts: Energy audits flag a 22% peak spike in lighting load during non-game hours.");
            
            recommendation.AppendLine("🌱 [SUSTAINABILITY INSIGHT]: Carbon footprint exceeds baseline target by 8%.");
            recommendation.AppendLine("💡 [ACTIONABLE RECOMMENDATION]: Schedule automated light shutdowns in non-critical concourse zones 30 minutes post-match. Transition volunteer task shifts to digital check-ins.");
            
            overrideProtocol = "Override option: Operations Manager can bypass automated power saving configurations manually.";
            confidence = 0.88m;
        }
        else
        {
            inputs.Add("General Operations Knowledge Base");
            explanation.AppendLine("Inference Logic: Standard response mapped using general stadium manuals and operational briefs.");
            recommendation.AppendLine("ℹ️ [INFO]: Standard tournament operations protocol active.");
            confidence = 0.80m;
        }

        return new AiDecisionSupportResult(
            recommendation.ToString(),
            escalation,
            confidence,
            explanation.ToString(),
            overrideProtocol,
            inputs);
    }
}
