using System.Text.RegularExpressions;

namespace StadiumOps.Application.Security;

public static class InputSanitizer
{
    private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Strip HTML tags to avoid XSS injections
        var clean = HtmlTagRegex.Replace(input, string.Empty);
        return clean.Trim();
    }
}
