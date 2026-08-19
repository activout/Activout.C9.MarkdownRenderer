using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="Quote"/> node using a <c>&gt;</c>-prefixed writer, so nested content of
/// any kind is quoted correctly without needing to know it is inside a block quote.
/// </summary>
public sealed class BlockQuoteMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public BlockQuoteMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is Quote;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var quote = (Quote)content;
        var quoteWriter = new PrefixMarkdownWriter(writer, "> ");
        var childContext = context.WithAncestor(MarkdownContainer.BlockQuote);

        await BlockRendering.RenderBlocks(_renderEngine, quote.Content ?? [], childContext, quoteWriter);
    }
}
