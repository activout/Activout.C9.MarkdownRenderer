using Activout.C9.MarkdownRenderer.Renderers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// Converts Contentful Rich Text documents into GitHub-Flavoured Markdown. This is the simple
/// entry point for consumers; advanced consumers can reach the underlying <see cref="MarkdownRenderEngine"/>
/// via <see cref="RenderEngine"/> to register custom renderers.
/// </summary>
public sealed class MarkdownRenderer
{
    private readonly DocumentMarkdownRenderer _documentRenderer;

    /// <summary>
    /// Creates a renderer with default options.
    /// </summary>
    public MarkdownRenderer() : this(new MarkdownRendererOptions())
    {
    }

    /// <summary>
    /// Creates a renderer with the given options.
    /// </summary>
    public MarkdownRenderer(MarkdownRendererOptions options)
    {
        RenderEngine = new MarkdownRenderEngine(options.RenderingIssue);
        RenderEngine.AddRenderers(
        [
            new ParagraphMarkdownRenderer(RenderEngine),
            new HeadingMarkdownRenderer(RenderEngine),
            new TextMarkdownRenderer(RenderEngine),
            new HorizontalRulerMarkdownRenderer(),
            new HyperlinkMarkdownRenderer(RenderEngine),
            new BlockQuoteMarkdownRenderer(RenderEngine),
            new ListMarkdownRenderer(RenderEngine),
            new ListItemMarkdownRenderer(RenderEngine),
            new AssetMarkdownRenderer(RenderEngine),
            new AssetHyperlinkMarkdownRenderer(RenderEngine),
            new TableMarkdownRenderer(RenderEngine),
            new NullMarkdownContentRenderer(RenderEngine)
        ]);

        _documentRenderer = new DocumentMarkdownRenderer(RenderEngine);
    }

    /// <summary>
    /// The underlying render engine, for advanced consumers who need to register custom renderers
    /// or dispatch rendering themselves.
    /// </summary>
    public MarkdownRenderEngine RenderEngine { get; }

    /// <summary>
    /// Converts <paramref name="document"/> to Markdown. Returns an empty string for <c>null</c>.
    /// </summary>
    public async Task<string> ToMarkdown(Document? document)
    {
        if (document is null) return "";
        return await _documentRenderer.Render(document);
    }

    /// <summary>
    /// Registers a custom renderer with the underlying <see cref="RenderEngine"/>.
    /// </summary>
    public void AddRenderer(IMarkdownContentRenderer renderer) => RenderEngine.AddRenderer(renderer);
}
