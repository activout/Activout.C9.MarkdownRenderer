using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders an <see cref="AssetStructure"/> node: as a Markdown image for image assets, or as a
/// link for any other asset type.
/// </summary>
public sealed class AssetMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public AssetMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is AssetStructure;

    /// <inheritdoc />
    public Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var assetStructure = (AssetStructure)content;
        var asset = assetStructure.Data?.Target;
        var url = UrlSafety.Sanitize(asset?.File?.Url);
        var isImage = asset?.File?.ContentType?.Contains("image", StringComparison.OrdinalIgnoreCase) ?? false;

        if (string.IsNullOrEmpty(url))
        {
            _renderEngine.ReportIssue(new MarkdownRenderingIssue
            {
                Kind = MarkdownRenderingIssueKind.MissingData,
                Content = content,
                Message = "Asset has no usable URL."
            });

            var fallbackText = asset?.Description ?? asset?.Title;
            if (!string.IsNullOrEmpty(fallbackText))
            {
                writer.Write(MarkdownEscaping.EscapeText(fallbackText));
            }

            return Task.CompletedTask;
        }

        var formattedUrl = UrlSafety.FormatForMarkdown(url);

        if (isImage)
        {
            var alt = asset?.Description ?? asset?.Title ?? "";
            writer.Write($"![{MarkdownEscaping.EscapeLinkText(alt)}]({formattedUrl})");
            return Task.CompletedTask;
        }

        var title = asset?.Title;
        if (string.IsNullOrEmpty(title))
        {
            _renderEngine.ReportIssue(new MarkdownRenderingIssue
            {
                Kind = MarkdownRenderingIssueKind.MissingData,
                Content = content,
                Message = "Asset has no title; using a generic link label."
            });
            title = "Attachment";
        }

        writer.Write($"[{MarkdownEscaping.EscapeLinkText(title)}]({formattedUrl})");
        return Task.CompletedTask;
    }
}
