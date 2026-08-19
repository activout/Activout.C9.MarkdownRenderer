using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="Hyperlink"/> node as a Markdown link, recursively rendering its
/// children so marks continue to work inside link text.
/// </summary>
public sealed class HyperlinkMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public HyperlinkMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is Hyperlink;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var link = (Hyperlink)content;
        var children = link.Content ?? [];
        var url = UrlSafety.Sanitize(link.Data?.Uri);

        if (string.IsNullOrEmpty(url))
        {
            _renderEngine.ReportIssue(new MarkdownRenderingIssue
            {
                Kind = MarkdownRenderingIssueKind.MissingData,
                Content = content,
                Message = "Hyperlink has no usable URL; rendering child content without a link."
            });

            foreach (var child in children)
            {
                await _renderEngine.Render(child, context, writer);
            }

            return;
        }

        var linkText = new StringBuilderMarkdownWriter();
        foreach (var child in children)
        {
            await _renderEngine.Render(child, context, linkText);
        }

        var title = link.Data?.Title;
        var titlePart = string.IsNullOrEmpty(title)
            ? ""
            : $" \"{title.Replace("\"", "\\\"")}\"";

        writer.Write($"[{linkText}]({UrlSafety.FormatForMarkdown(url)}{titlePart})");
    }
}
