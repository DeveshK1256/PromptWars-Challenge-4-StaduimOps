using StadiumOps.Application.Security;
using Xunit;

namespace StadiumOps.UnitTests.Security;

public sealed class InputSanitizerTests
{
    [Theory]
    [InlineData("Hello World", "Hello World")]
    [InlineData("Hello <script>alert('xss')</script> World", "Hello alert('xss') World")]
    [InlineData("<p>Para</p> Text", "Para Text")]
    [InlineData("Safe <b>Bold</b> String", "Safe Bold String")]
    [InlineData("   Spaced   ", "Spaced")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Sanitize_ShouldStripHtmlTagsAndTrim(string? input, string expected)
    {
        var result = InputSanitizer.Sanitize(input);
        Assert.Equal(expected, result);
    }
}
