using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="Text"/> node, escaping Markdown-significant characters and applying
/// standard Contentful text marks (bold, italic, inline code, underline, superscript, subscript).
/// </summary>
public sealed class TextMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public TextMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is Text;

    /// <inheritdoc />
    public Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var text = (Text)content;
        var value = text.Value ?? "";
        var marks = text.Marks ?? [];

        var hasCode = marks.Any(m => m.Type == "code");
        var rendered = hasCode ? MarkdownEscaping.WrapInlineCode(value) : MarkdownEscaping.EscapeText(value);

        // Apply from innermost (last mark) to outermost (first mark), matching Contentful's own
        // HtmlRenderer nesting convention.
        for (var i = marks.Count - 1; i >= 0; i--)
        {
            rendered = ApplyMark(marks[i], rendered, content);
        }

        writer.Write(rendered);
        return Task.CompletedTask;
    }

    private string ApplyMark(Mark mark, string rendered, IContent content)
    {
        switch (mark.Type)
        {
            case "code":
                return rendered; // already wrapped as inline code above
            case "bold":
                return $"**{rendered}**";
            case "italic":
                return $"_{rendered}_";
            case "underline":
                return $"<u>{rendered}</u>";
            case "superscript":
                return $"<sup>{rendered}</sup>";
            case "subscript":
                return $"<sub>{rendered}</sub>";
            default:
                _renderEngine.ReportIssue(new MarkdownRenderingIssue
                {
                    Kind = MarkdownRenderingIssueKind.UnsupportedContent,
                    Content = content,
                    Message = $"Unsupported text mark '{mark.Type}'."
                });
                return rendered;
        }
    }
}
