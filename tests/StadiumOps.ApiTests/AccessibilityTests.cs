using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace StadiumOps.ApiTests;

public sealed class AccessibilityTests
{
    private static readonly string FrontendPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../frontend"));

    [Fact]
    public void IndexHtml_ShouldHaveLangAndSkipLink()
    {
        var indexPath = Path.Combine(FrontendPath, "index.html");
        Assert.True(File.Exists(indexPath), $"index.html not found at {indexPath}");

        var html = File.ReadAllText(indexPath);

        // 1. Assert <html lang="en"> is present
        Assert.Contains("<html lang=\"en\"", html);

        // 2. Assert skip to main content link exists
        Assert.Contains("href=\"#main-content\"", html);
        Assert.Contains("Skip to main content", html);
    }

    [Fact]
    public void LoginPage_ShouldHaveAriaLabelsAndFormLabels()
    {
        var loginPagePath = Path.Combine(FrontendPath, "src/pages/LoginPage.tsx");
        Assert.True(File.Exists(loginPagePath), $"LoginPage.tsx not found at {loginPagePath}");

        var code = File.ReadAllText(loginPagePath);

        // 1. Assert label elements exist with htmlFor
        Assert.Contains("<label htmlFor=", code);

        // 2. Assert inputs have aria-required or required and matching ids
        Assert.Contains("id=\"email\"", code);
        Assert.Contains("id=\"password\"", code);
        Assert.Contains("aria-required=\"true\"", code);

        // 3. Assert live announcements exist for errors/success
        Assert.Contains("role=\"alert\"", code);
        Assert.Contains("aria-live=\"assertive\"", code);
        Assert.Contains("role=\"status\"", code);
        Assert.Contains("aria-live=\"polite\"", code);
    }

    [Fact]
    public void DashboardPage_ShouldHaveProgressbarAriaRoles()
    {
        var dashboardPath = Path.Combine(FrontendPath, "src/pages/DashboardPage.tsx");
        Assert.True(File.Exists(dashboardPath), $"DashboardPage.tsx not found at {dashboardPath}");

        var code = File.ReadAllText(dashboardPath);

        // 1. Assert progress bars have progressbar role and value limits
        Assert.Contains("role=\"progressbar\"", code);
        Assert.Contains("aria-valuenow=", code);
        Assert.Contains("aria-valuemin=", code);
        Assert.Contains("aria-valuemax=", code);
    }

    [Fact]
    public void AiChatPage_ShouldHaveLogAriaRoles()
    {
        var chatPath = Path.Combine(FrontendPath, "src/pages/AiChatPage.tsx");
        Assert.True(File.Exists(chatPath), $"AiChatPage.tsx not found at {chatPath}");

        var code = File.ReadAllText(chatPath);

        // 1. Assert log role is used for chat feed updates
        Assert.Contains("role=\"log\"", code);
        Assert.Contains("aria-live=\"polite\"", code);

        // 2. Assert textareas/inputs have aria-label or proper identifiers
        Assert.Contains("aria-label=\"Message input\"", code);
    }
}
