using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders the root <see cref="Document"/>. Not an <see cref="IMarkdownContentRenderer"/> since
/// <see cref="Document"/> does not implement <see cref="IContent"/> in the Contentful model;
/// used directly by <see cref="MarkdownRenderer"/>.
/// </summary>
internal sealed class DocumentMarkdownRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    public DocumentMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public async Task<string> Render(Document document)
    {
        var writer = new StringBuilderMarkdownWriter();
        var context = new MarkdownRenderContext().WithAncestor(MarkdownContainer.Document);

        await BlockRendering.RenderBlocks(_renderEngine, document.Content ?? [], context, writer);

        var raw = writer.ToString();
        return raw.Length == 0 ? "" : raw + "\n";
    }
}
