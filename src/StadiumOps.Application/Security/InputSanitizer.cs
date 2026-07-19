using System.Text.RegularExpressions;

namespace StadiumOps.Application.Security;

public static class InputSanitizer
{
    private static readonly Regex ScriptBlockRegex = new(@"<script[^>]*>[\s\S]*?<\/script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EventAttributesRegex = new(@"(?:on\w+\s*=)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex JavascriptProtocolRegex = new(@"javascript\s*:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Strip dangerous script blocks and general tags
        var clean = ScriptBlockRegex.Replace(input, string.Empty);
        clean = HtmlTagRegex.Replace(clean, string.Empty);
        clean = EventAttributesRegex.Replace(clean, string.Empty);
        clean = JavascriptProtocolRegex.Replace(clean, string.Empty);

        // HTML encode remaining characters to neutralize any leftover special symbols
        clean = System.Net.WebUtility.HtmlEncode(clean);

        return clean.Trim();
    }
}
