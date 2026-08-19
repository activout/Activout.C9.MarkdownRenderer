using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Tests.Support;

/// <summary>
/// Small helpers for constructing Contentful Rich Text nodes in tests without excessive
/// boilerplate.
/// </summary>
internal static class DocBuilder
{
    public static Document Doc(params IContent[] content) => new() { NodeType = "document", Content = [..content] };

    public static Text Text(string value, params string[] marks) => new()
    {
        NodeType = "text",
        Value = value,
        Marks = marks.Select(m => new Mark { Type = m }).ToList()
    };

    public static Paragraph Paragraph(params IContent[] content) =>
        new() { NodeType = "paragraph", Content = [..content] };

    public static IContent Heading(int level, params IContent[] content)
    {
        IContent heading = level switch
        {
            1 => new Heading1 { NodeType = "heading-1", Content = [..content] },
            2 => new Heading2 { NodeType = "heading-2", Content = [..content] },
            3 => new Heading3 { NodeType = "heading-3", Content = [..content] },
            4 => new Heading4 { NodeType = "heading-4", Content = [..content] },
            5 => new Heading5 { NodeType = "heading-5", Content = [..content] },
            6 => new Heading6 { NodeType = "heading-6", Content = [..content] },
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
        return heading;
    }

    public static HorizontalRuler Hr() => new() { NodeType = "hr" };

    public static Quote Quote(params IContent[] content) => new() { NodeType = "blockquote", Content = [..content] };

    public static List UnorderedList(params IContent[] items) =>
        new() { NodeType = "unordered-list", Content = [..items] };

    public static List OrderedList(params IContent[] items) =>
        new() { NodeType = "ordered-list", Content = [..items] };

    public static ListItem Item(params IContent[] content) => new() { NodeType = "list-item", Content = [..content] };

    public static Hyperlink Link(string? uri, params IContent[] content) => new()
    {
        NodeType = "hyperlink",
        Data = new HyperlinkData { Uri = uri! },
        Content = [..content]
    };

    public static Hyperlink LinkWithTitle(string uri, string title, params IContent[] content) => new()
    {
        NodeType = "hyperlink",
        Data = new HyperlinkData { Uri = uri, Title = title },
        Content = [..content]
    };

    public static Table Table(params TableRow[] rows) => new() { NodeType = "table", Content = [..rows] };

    public static TableRow Row(params IContent[] cells) => new() { NodeType = "table-row", Content = [..cells] };

    public static TableCell Cell(string text, int? rowspan = null, int? colspan = null) => new()
    {
        NodeType = "table-cell",
        Data = new TableCellData { Rowspan = rowspan, Colspan = colspan },
        Content = [Paragraph(Text(text))]
    };

    public static TableCell CellBlocks(params IContent[] content) => new()
    {
        NodeType = "table-cell",
        Content = [..content]
    };

    public static TableHeader Header(string text) => new()
    {
        NodeType = "table-header-cell",
        Content = [Paragraph(Text(text))]
    };

    public static AssetStructure Asset(string? url, string? contentType, string? title = null, string? description = null) =>
        new()
        {
            NodeType = "embedded-asset-block",
            Data = new AssetStructureData
            {
                Target = new Asset
                {
                    Title = title!,
                    Description = description!,
                    File = new Contentful.Core.Models.File { Url = url!, ContentType = contentType! }
                }
            }
        };

    public static AssetHyperlink AssetLink(string? url, string? title, params IContent[] content) => new()
    {
        NodeType = "asset-hyperlink",
        Data = new AssetHyperlinkData
        {
            Target = new Asset { Title = title!, File = new Contentful.Core.Models.File { Url = url! } }
        },
        Content = [..content]
    };
}
