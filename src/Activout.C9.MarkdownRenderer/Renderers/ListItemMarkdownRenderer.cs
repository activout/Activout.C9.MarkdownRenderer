using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="ListItem"/> node's children into the marker-prefixed writer supplied by
/// the owning <see cref="ListMarkdownRenderer"/>, joining multiple children with blank lines.
/// </summary>
public sealed class ListItemMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public ListItemMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is ListItem;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var listItem = (ListItem)content;
        var childContext = context.WithAncestor(MarkdownContainer.ListItem);

        await BlockRendering.RenderBlocks(
            _renderEngine, listItem.Content ?? [], childContext, writer,
            static (previous, current) => previous is Paragraph && current is Paragraph);
    }
}
