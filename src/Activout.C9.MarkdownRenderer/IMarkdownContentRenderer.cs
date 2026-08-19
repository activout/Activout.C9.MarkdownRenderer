using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// Renders a single kind of Contentful Rich Text <see cref="IContent"/> node to Markdown.
/// </summary>
public interface IMarkdownContentRenderer
{
    /// <summary>
    /// Determines selection order among renderers that both support the same content.
    /// Lower values are tried first. Built-in renderers use 100; the fallback renderer uses 500.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns whether this renderer can render <paramref name="content"/>.
    /// </summary>
    bool SupportsContent(IContent content);

    /// <summary>
    /// Renders <paramref name="content"/> to <paramref name="writer"/>.
    /// </summary>
    Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer);
}
