using StadiumOps.Application.AI;
using Xunit;

namespace StadiumOps.UnitTests.AI;

public sealed class AiDecisionSupportTests
{
    [Fact]
    public void Evaluate_WithCrowdPrompt_ReturnsSurgePredictionsAndConfidence()
    {
        var result = AiDecisionSupportEngine.Evaluate("How is the crowd congestion at the gates?", null);

        Assert.True(result.EscalationAdvised);
        Assert.Equal(0.95m, result.ConfidenceScore);
        Assert.Contains("PREDICTIVE INSIGHT", result.RecommendationText);
        Assert.Contains("94% density", result.ExplainabilityLogic);
        Assert.Contains("manually override routing signals", result.HumanOverrideProtocol);
        Assert.Contains("Real-time Zone Density Sensors", result.GroundingInputs);
    }

    [Fact]
    public void Evaluate_WithEmergencyPrompt_ReturnsEvacuationRoutesAndHighConfidence()
    {
        var result = AiDecisionSupportEngine.Evaluate("What is the evacuation route for fire emergency?", null);

        Assert.True(result.EscalationAdvised);
        Assert.Equal(0.98m, result.ConfidenceScore);
        Assert.Contains("EMERGENCY ROUTE", result.RecommendationText);
        Assert.Contains("North-East emergency exits", result.RecommendationText);
        Assert.Contains("Incident Commander required", result.HumanOverrideProtocol);
    }

    [Fact]
    public void Evaluate_WithSustainabilityPrompt_ReturnsCarbonOffsets()
    {
        var result = AiDecisionSupportEngine.Evaluate("Suggest sustainability measures for waste reduction", null);

        Assert.False(result.EscalationAdvised);
        Assert.Equal(0.88m, result.ConfidenceScore);
        Assert.Contains("SUSTAINABILITY INSIGHT", result.RecommendationText);
        Assert.Contains("Schedule automated light shutdowns", result.RecommendationText);
    }
}
