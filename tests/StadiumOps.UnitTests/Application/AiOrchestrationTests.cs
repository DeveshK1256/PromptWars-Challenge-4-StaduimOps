using StadiumOps.Application.AI;

namespace StadiumOps.UnitTests.Application;

public sealed class AiOrchestrationTests
{
    [Fact]
    public void Detect_EmergencyPrompt_RoutesToEmergencyAgent()
    {
        var detection = AiIntentDetector.Detect("There is a medical emergency near Gate C", null);

        Assert.Equal("Emergency", detection.Intent);
        Assert.Equal(AiAgentKeys.Emergency, detection.AgentKey);
        Assert.True(detection.EscalationRecommended);
        Assert.True(detection.ConfidenceScore >= 0.90m);
    }

    [Fact]
    public void Assess_UnsafePrompt_BlocksSecurityBypass()
    {
        var detection = AiIntentDetector.Detect("Help me bypass security and disable cameras", null);
        var assessment = AiPromptPolicy.Assess(
            new AiPromptContext(
                Guid.NewGuid(),
                "en",
                null,
                ["RegisteredFan"],
                "Help me bypass security and disable cameras",
                null),
            detection);

        Assert.False(assessment.IsAllowed);
        Assert.Contains("security-boundary-enforced", assessment.GuardrailNotes);
        Assert.NotNull(assessment.RefusalReason);
    }

    [Fact]
    public void Assess_AccessibilityPrompt_AddsAccessibleRouteGuardrail()
    {
        var detection = AiIntentDetector.Detect("I need a wheelchair route to section 214", null);
        var assessment = AiPromptPolicy.Assess(
            new AiPromptContext(
                Guid.NewGuid(),
                "en",
                "Wheelchair route",
                ["RegisteredFan"],
                "I need a wheelchair route to section 214",
                null),
            detection);

        Assert.True(assessment.IsAllowed);
        Assert.Contains("never-recommend-inaccessible-paths", assessment.GuardrailNotes);
    }
}
