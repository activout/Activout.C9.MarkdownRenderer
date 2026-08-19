using System.Net;
using System.Text;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Internal;

/// <summary>
/// A compact, self-contained HTML renderer used only for complex table cells (rowspan, colspan,
/// multiple paragraphs, nested lists/quotes/tables) that cannot be represented as a GFM pipe
/// table cell. Kept separate from the main Markdown renderer pipeline since HTML fallback output
/// must be well-formed HTML throughout, not a mix of HTML and Markdown syntax.
/// </summary>
internal static class CellHtmlRenderer
{
    public static string RenderAll(IEnumerable<IContent> content) => string.Concat(content.Select(Render));

    public static string Render(IContent content) => content switch
    {
        Text text => RenderText(text),
        Paragraph p => $"<p>{RenderAll(p.Content ?? [])}</p>",
        Heading1 h => $"<h1>{RenderAll(h.Content ?? [])}</h1>",
        Heading2 h => $"<h2>{RenderAll(h.Content ?? [])}</h2>",
        Heading3 h => $"<h3>{RenderAll(h.Content ?? [])}</h3>",
        Heading4 h => $"<h4>{RenderAll(h.Content ?? [])}</h4>",
        Heading5 h => $"<h5>{RenderAll(h.Content ?? [])}</h5>",
        Heading6 h => $"<h6>{RenderAll(h.Content ?? [])}</h6>",
        HorizontalRuler => "<hr />",
        Quote q => $"<blockquote>{RenderAll(q.Content ?? [])}</blockquote>",
        List list => RenderList(list),
        ListItem li => $"<li>{RenderAll(li.Content ?? [])}</li>",
        Hyperlink link => RenderHyperlink(link),
        AssetStructure asset => RenderAsset(asset),
        AssetHyperlink assetLink => RenderAssetHyperlink(assetLink),
        Table table => RenderTable(table),
        TableRow row => $"<tr>{RenderAll(row.Content ?? [])}</tr>",
        TableHeader th => RenderCell("th", th.Content ?? [], th.Data),
        TableCell td => RenderCell("td", td.Content ?? [], td.Data),
        _ => ""
    };

    private static string RenderText(Text text)
    {
        var value = WebUtility.HtmlEncode(text.Value ?? "");
        foreach (var mark in text.Marks ?? [])
        {
            var tag = mark.Type switch
            {
                "bold" => "strong",
                "italic" => "em",
                "underline" => "u",
                "code" => "code",
                "superscript" => "sup",
                "subscript" => "sub",
                _ => null
            };

            if (tag is not null)
            {
                value = $"<{tag}>{value}</{tag}>";
            }
        }

        return value;
    }

    private static string RenderList(List list)
    {
        var tag = list.NodeType == "ordered-list" ? "ol" : "ul";
        return $"<{tag}>{RenderAll(list.Content ?? [])}</{tag}>";
    }

    private static string RenderHyperlink(Hyperlink link)
    {
        var url = WebUtility.HtmlEncode(UrlSafety.Sanitize(link.Data?.Uri) ?? "#");
        return $"<a href=\"{url}\">{RenderAll(link.Content ?? [])}</a>";
    }

    private static string RenderAsset(AssetStructure assetStructure)
    {
        var asset = assetStructure.Data?.Target;
        var url = UrlSafety.Sanitize(asset?.File?.Url);
        var isImage = asset?.File?.ContentType?.Contains("image", StringComparison.OrdinalIgnoreCase) ?? false;

        if (isImage)
        {
            var alt = WebUtility.HtmlEncode(asset?.Description ?? asset?.Title ?? "");
            return $"<img src=\"{WebUtility.HtmlEncode(url ?? "")}\" alt=\"{alt}\" />";
        }

        var title = WebUtility.HtmlEncode(asset?.Title ?? "Attachment");
        return string.IsNullOrEmpty(url) ? title : $"<a href=\"{WebUtility.HtmlEncode(url)}\">{title}</a>";
    }

    private static string RenderAssetHyperlink(AssetHyperlink assetHyperlink)
    {
        var asset = assetHyperlink.Data?.Target;
        var url = UrlSafety.Sanitize(asset?.File?.Url);
        var children = assetHyperlink.Content ?? [];
        var text = children.Count > 0 ? RenderAll(children) : WebUtility.HtmlEncode(asset?.Title ?? "Attachment");
        return string.IsNullOrEmpty(url) ? text : $"<a href=\"{WebUtility.HtmlEncode(url)}\">{text}</a>";
    }

    private static string RenderTable(Table table)
    {
        var sb = new StringBuilder("<table>");
        foreach (var row in table.Content ?? [])
        {
            sb.Append(Render(row));
        }

        sb.Append("</table>");
        return sb.ToString();
    }

    private static string RenderCell(string tag, IReadOnlyList<IContent> content, TableCellData? data)
    {
        var rowspan = data?.Rowspan is > 1 ? $" rowspan=\"{data.Rowspan}\"" : "";
        var colspan = data?.Colspan is > 1 ? $" colspan=\"{data.Colspan}\"" : "";
        return $"<{tag}{rowspan}{colspan}>{RenderAll(content)}</{tag}>";
    }
}
