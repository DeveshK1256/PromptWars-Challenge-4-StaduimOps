using StadiumOps.Application.Features;

namespace StadiumOps.UnitTests.Application;

public sealed class IncidentPrioritizerTests
{
    [Theory]
    [InlineData("Fire", "Normal", "Critical")]
    [InlineData("Lost child", "Normal", "Critical")]
    [InlineData("Medical", "High", "High")]
    [InlineData("Broken facility", "Normal", "Normal")]
    public void Prioritize_ReturnsExpectedPriority(string category, string severity, string expected)
    {
        var priority = IncidentPrioritizer.Prioritize(category, severity);

        Assert.Equal(expected, priority);
    }

    [Theory]
    [InlineData("Medical", "Medical Team")]
    [InlineData("Security", "Security Operations")]
    [InlineData("Lost child", "Security Operations")]
    [InlineData("Fire", "Emergency Coordination")]
    [InlineData("Crowd issue", "Venue Operations")]
    public void AssignTeam_ReturnsExpectedTeam(string category, string expected)
    {
        var team = IncidentPrioritizer.AssignTeam(category);

        Assert.Equal(expected, team);
    }
}
