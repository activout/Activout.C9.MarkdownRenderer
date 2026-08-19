using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Internal;

/// <summary>
/// Determines whether a subtree of Contentful content can be represented safely as inline GFM,
/// with no block-level structure. Used by the table renderer to decide between a GFM pipe table
/// and an HTML fallback.
/// </summary>
internal static class InlineContentAnalyzer
{
    /// <summary>
    /// A table cell is simple when it has no content, or exactly one paragraph whose own content
    /// is entirely inline.
    /// </summary>
    public static bool IsSimpleCell(IReadOnlyList<IContent> cellContent)
    {
        if (cellContent.Count == 0) return true;
        if (cellContent.Count > 1) return false;
        return cellContent[0] is Paragraph paragraph && IsInlineOnly(paragraph.Content ?? []);
    }

    private static bool IsInlineOnly(IReadOnlyList<IContent> content)
    {
        foreach (var node in content)
        {
            switch (node)
            {
                case Text:
                case AssetHyperlink:
                case AssetStructure:
                    break;
                case Hyperlink link when IsInlineOnly(link.Content ?? []):
                    break;
                default:
                    return false;
            }
        }

        return true;
    }
}
