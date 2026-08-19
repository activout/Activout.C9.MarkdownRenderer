using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Final fallback renderer for any <see cref="IContent"/> not claimed by another renderer.
/// Reports <see cref="MarkdownRenderingIssueKind.UnsupportedContent"/> and preserves child
/// content where the node exposes any.
/// </summary>
public sealed class NullMarkdownContentRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public NullMarkdownContentRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 500;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => true;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        _renderEngine.ReportIssue(new MarkdownRenderingIssue
        {
            Kind = MarkdownRenderingIssueKind.UnsupportedContent,
            Content = content,
            Message = $"Unsupported content node '{content.GetType().Name}'."
        });

        // Embedded entries/resources are the one standard node type with no dedicated renderer
        // that still carries meaningful child content worth preserving.
        if (content is EntryStructure entry)
        {
            foreach (var child in entry.Content ?? [])
            {
                await _renderEngine.Render(child, context, writer);
            }
        }
    }
}
