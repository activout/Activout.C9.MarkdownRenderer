using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="Paragraph"/> node by recursively rendering its inline children.
/// </summary>
public sealed class ParagraphMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public ParagraphMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is Paragraph;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var paragraph = (Paragraph)content;
        var childContext = context.WithAncestor(MarkdownContainer.Paragraph);

        foreach (var child in paragraph.Content ?? [])
        {
            await _renderEngine.Render(child, childContext, writer);
        }
    }
}
