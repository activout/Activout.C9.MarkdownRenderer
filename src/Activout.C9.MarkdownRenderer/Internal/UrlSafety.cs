namespace Activout.C9.MarkdownRenderer.Internal;

/// <summary>
/// Neutralizes unsafe URI schemes before they are emitted into Markdown or HTML output.
/// </summary>
internal static class UrlSafety
{
    /// <summary>
    /// Returns a safe version of <paramref name="url"/>: relative URLs, protocol-relative URLs,
    /// and http/https URLs pass through unchanged; anything else (javascript:, data:, vbscript:, ...)
    /// is replaced with <c>#</c>.
    /// </summary>
    public static string? Sanitize(string? url)
    {
        if (string.IsNullOrEmpty(url)) return url;

        // Protocol-relative URLs (e.g. //images.ctfassets.net/...) and relative URLs are safe:
        // they cannot carry a script-executing scheme such as javascript: or data:.
        if (url.StartsWith("//", StringComparison.Ordinal)) return url;

        if (HasScheme(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Scheme is "http" or "https" ? url : "#";
        }

        // Relative URL. Note: Uri.TryCreate(..., UriKind.Absolute, ...) can succeed for an
        // OS-style absolute path such as "/about" (treating it as a file: URI), so schemeless
        // strings are intentionally never passed to Uri.TryCreate above.
        return url;
    }

    private static bool HasScheme(string url)
    {
        var colonIndex = url.IndexOf(':');
        if (colonIndex <= 0 || !char.IsAsciiLetter(url[0])) return false;

        for (var i = 1; i < colonIndex; i++)
        {
            var c = url[i];
            if (!char.IsAsciiLetterOrDigit(c) && c is not ('+' or '-' or '.')) return false;
        }

        return true;
    }

    /// <summary>
    /// Wraps a URL in angle brackets when it contains characters (spaces, parentheses) that
    /// would otherwise break Markdown link/image syntax.
    /// </summary>
    public static string FormatForMarkdown(string url) =>
        url.Any(c => c is ' ' or '(' or ')') ? $"<{url}>" : url;
}
