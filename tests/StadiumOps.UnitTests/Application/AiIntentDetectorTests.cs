using StadiumOps.Application.AI;

namespace StadiumOps.UnitTests.Application;

public sealed class AiIntentDetectorTests
{
    [Theory]
    [InlineData("There is a medical emergency near Gate C", "Emergency", AiAgentKeys.Emergency)]
    [InlineData("Someone is injured in Section 4", "Emergency", AiAgentKeys.Emergency)]
    [InlineData("Fire alarm is ringing in the south stand", "Emergency", AiAgentKeys.Emergency)]
    [InlineData("I need to evacuate, where do I go?", "Emergency", AiAgentKeys.Emergency)]
    [InlineData("I lost my child near Gate B", "Emergency", AiAgentKeys.Emergency)]
    public void Detect_EmergencyPrompts_RoutesToEmergencyAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
        Assert.True(result.EscalationRecommended);
        Assert.True(result.ConfidenceScore >= 0.90m);
    }

    [Theory]
    [InlineData("I need a wheelchair route to section 214", "Accessibility", AiAgentKeys.Accessibility)]
    [InlineData("Where is the accessible entrance?", "Accessibility", AiAgentKeys.Accessibility)]
    [InlineData("Is there an elevator near Gate A?", "Accessibility", AiAgentKeys.Accessibility)]
    public void Detect_AccessibilityPrompts_RoutesToAccessibilityAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
        Assert.False(result.EscalationRecommended);
    }

    [Theory]
    [InlineData("How do I get to Gate 3?", "Navigation", AiAgentKeys.Navigation)]
    [InlineData("Where is my seat in section 101?", "Navigation", AiAgentKeys.Navigation)]
    [InlineData("Show me the route to the main entrance", "Navigation", AiAgentKeys.Navigation)]
    public void Detect_NavigationPrompts_RoutesToNavigationAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Theory]
    [InlineData("How crowded is the south concourse?", "Crowd", AiAgentKeys.Crowd)]
    [InlineData("Is there congestion near the food area?", "Crowd", AiAgentKeys.Crowd)]
    [InlineData("Show me the density heatmap", "Crowd", AiAgentKeys.Crowd)]
    public void Detect_CrowdPrompts_RoutesToCrowdAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
        Assert.True(result.EscalationRecommended);
    }

    [Theory]
    [InlineData("Where can I park my car?", "Transportation", AiAgentKeys.Transportation)]
    [InlineData("Which metro line goes to the stadium?", "Transportation", AiAgentKeys.Transportation)]
    [InlineData("Is there a shuttle bus available?", "Transportation", AiAgentKeys.Transportation)]
    public void Detect_TransportationPrompts_RoutesToTransportationAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Theory]
    [InlineData("How is the stadium managing waste recycling?", "Sustainability", AiAgentKeys.Sustainability)]
    [InlineData("What is the carbon footprint of this event?", "Sustainability", AiAgentKeys.Sustainability)]
    public void Detect_SustainabilityPrompts_RoutesToSustainabilityAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Theory]
    [InlineData("Translate this to Spanish: Welcome to the stadium", "Translation", AiAgentKeys.Translator)]
    [InlineData("How do I say hello in Arabic?", "Translation", AiAgentKeys.Translator)]
    public void Detect_TranslationPrompts_RoutesToTranslatorAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Theory]
    [InlineData("I need help with my volunteer shift today", "Volunteer", AiAgentKeys.Volunteer)]
    [InlineData("What is my assigned task for this event?", "Volunteer", AiAgentKeys.Volunteer)]
    public void Detect_VolunteerPrompts_RoutesToVolunteerAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Theory]
    [InlineData("What is the operational status of all incidents?", "Operations", AiAgentKeys.Operations)]
    [InlineData("Summarize the current staff deployment report", "Operations", AiAgentKeys.Operations)]
    public void Detect_OperationsPrompts_RoutesToOperationsAgent(string prompt, string expectedIntent, string expectedAgent)
    {
        var result = AiIntentDetector.Detect(prompt, null);
        Assert.Equal(expectedIntent, result.Intent);
        Assert.Equal(expectedAgent, result.AgentKey);
    }

    [Fact]
    public void Detect_GenericQuestion_RoutesToFanAssistant()
    {
        var result = AiIntentDetector.Detect("What time does the fan zone open?", null);
        Assert.Equal("General", result.Intent);
        Assert.Equal(AiAgentKeys.FanAssistant, result.AgentKey);
        Assert.True(result.ConfidenceScore < 0.90m);
    }

    [Fact]
    public void Detect_EmptyPrompt_ReturnsFanAssistantWithLowConfidence()
    {
        var result = AiIntentDetector.Detect("", null);
        Assert.Equal(AiAgentKeys.FanAssistant, result.AgentKey);
        Assert.True(result.ConfidenceScore <= 0.80m);
    }

    [Fact]
    public void Detect_ContextBoostsEmergencyIntent()
    {
        // Emergency keyword in context should still trigger emergency
        var result = AiIntentDetector.Detect("Need help", "emergency at gate B");
        Assert.Equal("Emergency", result.Intent);
    }

    [Fact]
    public void Detect_MatchedSignals_AreReturned()
    {
        var result = AiIntentDetector.Detect("There is a medical emergency at the stadium", null);
        Assert.NotEmpty(result.MatchedSignals);
        Assert.Contains(result.MatchedSignals, s => s == "medical" || s == "emergency");
    }
}
