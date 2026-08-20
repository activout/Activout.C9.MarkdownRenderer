using System.Text;

namespace Activout.C9.MarkdownRenderer.Internal;

/// <summary>
/// Centralized, context-aware Markdown character escaping for plain text content.
/// </summary>
internal static class MarkdownEscaping
{
    /// <summary>
    /// Escapes characters in <paramref name="value"/> that could unintentionally introduce
    /// Markdown syntax, without over-escaping ordinary text.
    /// </summary>
    public static string EscapeText(string value)
    {
        if (value.Length == 0) return value;

        StringBuilder? sb = null;
        var orderedMarkerDelimiterIndex = FindLeadingOrderedListMarkerDelimiterIndex(value);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            var escape = c switch
            {
                '\\' or '`' or '[' or ']' or '<' or '>' => true,
                '*' => IsFlanked(value, i) || (i == 0 && IsBulletMarker(value)),
                '_' => IsFlanked(value, i),
                '-' or '+' => i == 0 && IsBulletMarker(value),
                '#' => i == 0,
                _ => i == orderedMarkerDelimiterIndex
            };

            if (escape)
            {
                sb ??= new StringBuilder(value.Length + 4).Append(value, 0, i);
                sb.Append('\\');
            }

            sb?.Append(c);
        }

        return sb?.ToString() ?? value;
    }

    /// <summary>
    /// Chooses a backtick delimiter long enough to safely wrap <paramref name="code"/> as an
    /// inline code span, along with the padding needed if the content itself starts or ends
    /// with a backtick.
    /// </summary>
    public static string WrapInlineCode(string code)
    {
        var longestRun = 0;
        var currentRun = 0;
        foreach (var c in code)
        {
            if (c == '`')
            {
                currentRun++;
                longestRun = Math.Max(longestRun, currentRun);
            }
            else
            {
                currentRun = 0;
            }
        }

        var fence = new string('`', longestRun + 1);
        var needsPadding = code.Length > 0 && (code[0] == '`' || code[^1] == '`');
        var padding = needsPadding ? " " : "";
        return $"{fence}{padding}{code}{padding}{fence}";
    }

    /// <summary>
    /// Escapes backslashes and square brackets so a plain string is safe to place inside the
    /// link-text/alt-text portion of a Markdown link or image (e.g. asset titles).
    /// </summary>
    public static string EscapeLinkText(string value) =>
        value.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");

    private static bool IsFlanked(string value, int index)
    {
        var leftIsWord = index > 0 && !char.IsWhiteSpace(value[index - 1]);
        var rightIsWord = index < value.Length - 1 && !char.IsWhiteSpace(value[index + 1]);
        return leftIsWord || rightIsWord;
    }

    /// <summary>
    /// True when <paramref name="value"/> starts with a bullet character followed by a space or
    /// nothing else, i.e. it would be read as a CommonMark list marker at the start of a line.
    /// </summary>
    private static bool IsBulletMarker(string value) =>
        value.Length == 1 || char.IsWhiteSpace(value[1]);

    /// <summary>
    /// Finds the index of the <c>.</c> or <c>)</c> delimiter of a leading ordered-list marker
    /// (a digit run followed by that delimiter, then a space or nothing else), or -1 if
    /// <paramref name="value"/> does not start with one.
    /// </summary>
    private static int FindLeadingOrderedListMarkerDelimiterIndex(string value)
    {
        var i = 0;
        while (i < value.Length && i < 9 && char.IsAsciiDigit(value[i])) i++;
        if (i == 0 || i >= value.Length) return -1;
        if (value[i] is not ('.' or ')')) return -1;
        if (i + 1 < value.Length && !char.IsWhiteSpace(value[i + 1])) return -1;
        return i;
    }
}
