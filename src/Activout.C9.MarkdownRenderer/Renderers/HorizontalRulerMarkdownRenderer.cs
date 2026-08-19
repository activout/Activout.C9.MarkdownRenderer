using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="HorizontalRuler"/> node.
/// </summary>
public sealed class HorizontalRulerMarkdownRenderer : IMarkdownContentRenderer
{
    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is HorizontalRuler;

    /// <inheritdoc />
    public Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        writer.Write("---");
        return Task.CompletedTask;
    }
}
