using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="List"/> node (ordered or unordered), owning list markers and the
/// indentation applied to each item's content.
/// </summary>
public sealed class ListMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public ListMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is List;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var list = (List)content;
        var ordered = list.NodeType == "ordered-list";
        var items = list.Content ?? [];
        var childContext = context.WithAncestor(MarkdownContainer.List);

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                writer.WriteLine();
            }

            var marker = ordered ? $"{i + 1}. " : "- ";
            var continuation = new string(' ', marker.Length);
            var itemWriter = new PrefixMarkdownWriter(writer, marker, continuation);

            await _renderEngine.Render(items[i], childContext, itemWriter);
        }
    }
}
