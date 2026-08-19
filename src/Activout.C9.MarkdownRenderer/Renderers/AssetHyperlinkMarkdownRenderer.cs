using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders an <see cref="AssetHyperlink"/> node as a Markdown link, preserving recursively
/// rendered child content as the link text.
/// </summary>
public sealed class AssetHyperlinkMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public AssetHyperlinkMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is AssetHyperlink;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var assetHyperlink = (AssetHyperlink)content;
        var asset = assetHyperlink.Data?.Target;
        var children = assetHyperlink.Content ?? [];
        var url = UrlSafety.Sanitize(asset?.File?.Url);

        if (string.IsNullOrEmpty(url))
        {
            _renderEngine.ReportIssue(new MarkdownRenderingIssue
            {
                Kind = MarkdownRenderingIssueKind.MissingData,
                Content = content,
                Message = "Asset hyperlink has no usable URL; rendering child content without a link."
            });

            foreach (var child in children)
            {
                await _renderEngine.Render(child, context, writer);
            }

            return;
        }

        string linkText;
        if (children.Count > 0)
        {
            var inner = new StringBuilderMarkdownWriter();
            foreach (var child in children)
            {
                await _renderEngine.Render(child, context, inner);
            }

            linkText = inner.ToString();
        }
        else
        {
            linkText = MarkdownEscaping.EscapeLinkText(asset?.Title ?? "Attachment");
        }

        writer.Write($"[{linkText}]({UrlSafety.FormatForMarkdown(url)})");
    }
}
