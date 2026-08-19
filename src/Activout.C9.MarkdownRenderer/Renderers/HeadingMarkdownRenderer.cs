using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders <see cref="Heading1"/> through <see cref="Heading6"/> as ATX headings.
/// </summary>
public sealed class HeadingMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public HeadingMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is IHeading;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var level = content switch
        {
            Heading1 => 1,
            Heading2 => 2,
            Heading3 => 3,
            Heading4 => 4,
            Heading5 => 5,
            Heading6 => 6,
            _ => 1
        };

        var heading = (IHeading)content;
        writer.Write(new string('#', level));
        writer.Write(" ");

        foreach (var child in heading.Content ?? [])
        {
            await _renderEngine.Render(child, context, writer);
        }
    }
}
