using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Internal;

/// <summary>
/// Joins sibling block-level nodes (paragraphs, lists, quotes, tables, ...) with exactly one
/// blank line between them, skipping blocks that render as empty so they cannot introduce
/// stray blank lines.
/// </summary>
internal static class BlockRendering
{
    public static Task RenderBlocks(
        MarkdownRenderEngine renderEngine,
        IReadOnlyList<IContent> children,
        MarkdownRenderContext context,
        IMarkdownWriter writer) =>
        RenderBlocks(renderEngine, children, context, writer, static (_, _) => true);

    /// <summary>
    /// Joins sibling blocks with a single newline, adding a further blank line before a child
    /// only when <paramref name="needsBlankLineBefore"/> says the previous/current pair requires
    /// one (e.g. two consecutive paragraphs, but not a nested list directly following text).
    /// </summary>
    public static async Task RenderBlocks(
        MarkdownRenderEngine renderEngine,
        IReadOnlyList<IContent> children,
        MarkdownRenderContext context,
        IMarkdownWriter writer,
        Func<IContent, IContent, bool> needsBlankLineBefore)
    {
        IContent? previous = null;
        var isFirst = true;
        foreach (var child in children)
        {
            var buffer = new StringBuilderMarkdownWriter();
            await renderEngine.Render(child, context, buffer);
            var rendered = buffer.ToString();
            if (rendered.Length == 0) continue;

            if (!isFirst)
            {
                writer.WriteLine();
                if (needsBlankLineBefore(previous!, child))
                {
                    writer.WriteLine();
                }
            }

            isFirst = false;
            previous = child;
            writer.Write(rendered);
        }
    }
}
