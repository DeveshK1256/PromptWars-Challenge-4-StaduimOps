using StadiumOps.Application.Security;
using Xunit;

namespace StadiumOps.UnitTests.Security;

public sealed class InputSanitizerTests
{
    [Theory]
    [InlineData("Hello World", "Hello World")]
    [InlineData("Hello <script>alert('xss')</script> World", "Hello  World")]
    [InlineData("<p>Para</p> Text", "Para Text")]
    [InlineData("Safe <b>Bold</b> String", "Safe Bold String")]
    [InlineData("   Spaced   ", "Spaced")]
    [InlineData("<img src=x onerror=alert(1)>", "img src=x")]
    [InlineData("javascript:alert(1)", "alert(1)")]
    [InlineData("<svg onload=alert(1)>", "svg")]
    [InlineData("Hello & Welcome", "Hello &amp; Welcome")] // Neutralized via HTML encoding
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Sanitize_ShouldStripHtmlTagsAndTrim(string? input, string expected)
    {
        var result = InputSanitizer.Sanitize(input);
        Assert.Equal(expected, result);
    }
}
