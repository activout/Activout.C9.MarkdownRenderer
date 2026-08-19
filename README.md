# Activout.C9.MarkdownRenderer

Converts Contentful Rich Text documents (`Contentful.Core.Models.Document`) into GitHub-Flavoured
Markdown (GFM). Built primarily for feeding structured content to LLMs, while remaining readable
for humans.

Rendering is **best-effort**: malformed or incomplete Contentful data degrades gracefully instead
of throwing, and degradation is observable through a diagnostics callback. Exceptions thrown by
your own custom renderers always propagate.

## Installation

```bash
dotnet add package Activout.C9.MarkdownRenderer
```

## Basic usage

```csharp
using Activout.C9.MarkdownRenderer;

var renderer = new MarkdownRenderer();

var markdown = await renderer.ToMarkdown(document);
```

`document` is a `Contentful.Core.Models.Document`, typically deserialized from a Contentful Rich
Text field via the official `Contentful.Csharp` SDK. `ToMarkdown` returns `""` for a `null`
document.

## Supported Rich Text nodes

| Contentful node | Output |
| --- | --- |
| Heading 1–6 | ATX headings (`#` … `######`) |
| Paragraph | Plain text block |
| Text + marks | See marks below |
| Hyperlink | `[text](url "title")` |
| Block quote | `>`-prefixed lines, arbitrary nested content |
| Unordered list | `-` markers, nested lists indent |
| Ordered list | `1.`, `2.`, … markers, nested lists indent |
| Horizontal ruler | `---` |
| Embedded asset (image) | `![alt](url)` |
| Embedded asset (other) | `[title](url)` |
| Asset hyperlink | `[child content](url)` |
| Table | GFM pipe table, or HTML fallback (see below) |
| Embedded entry / unknown node | Fallback: child content preserved where present, otherwise nothing |

## Supported marks

| Mark | Output |
| --- | --- |
| Bold | `**text**` |
| Italic | `_text_` |
| Inline code | `` `code` `` (backtick-run length grows to fit code containing backticks) |
| Underline | `<u>text</u>` (no GFM equivalent) |
| Superscript | `<sup>text</sup>` |
| Subscript | `<sub>text</sub>` |
| Unknown mark | Underlying text is rendered without the mark; reported as an issue |

Marks may be nested and combine correctly, e.g. `**_bold italic_**`.

## GFM output and HTML fallback

The renderer produces exactly one Markdown dialect: GitHub-Flavoured Markdown. Where GFM cannot
represent Contentful semantics accurately — underline/superscript/subscript, or a table with
rowspan, colspan, multiple paragraphs in a cell, or other nested block content in a cell — the
renderer intentionally falls back to a conservative, well-formed HTML fragment rather than
distorting the structure to force it into Markdown.

A table is rendered as a GFM pipe table only when every cell is simple inline content, with no
rowspan/colspan and no nested block structure. If Contentful supplies no explicit header row, the
first row is used as the header row.

## Best-effort rendering and `MarkdownRenderingIssue`

Content-related problems (an unsupported node, a missing asset URL, a malformed table row, …)
never throw. Instead, rendering continues with the best available representation, and you can
observe what was lost via an optional callback:

```csharp
var renderer = new MarkdownRenderer(new MarkdownRendererOptions
{
    RenderingIssue = issue => logger.LogWarning("Markdown rendering issue: {Kind} {Message}", issue.Kind, issue.Message)
});
```

`MarkdownRenderingIssue.Kind` is one of:

- `UnsupportedContent` — a node type has no registered renderer.
- `MissingData` — expected data was absent (e.g. an asset with no URL).
- `InvalidStructure` — a known node type was structured unexpectedly (e.g. a malformed table row).

If no callback is configured, rendering continues silently.

## Custom renderers

Register a custom `IMarkdownContentRenderer` to handle additional node types or override built-in
behavior:

```csharp
using Activout.C9.MarkdownRenderer;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

public sealed class CustomQuoteRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    public CustomQuoteRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public int Order => 50; // lower than the built-in BlockQuoteMarkdownRenderer's 100

    public bool SupportsContent(IContent content) => content is Quote;

    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var quote = (Quote)content;
        var quoteWriter = new PrefixMarkdownWriter(writer, "custom> ");

        foreach (var child in quote.Content ?? [])
        {
            await _renderEngine.Render(child, context, quoteWriter);
        }
    }
}
```

```csharp
var renderer = new MarkdownRenderer();
renderer.AddRenderer(new CustomQuoteRenderer(renderer.RenderEngine));

var markdown = await renderer.ToMarkdown(document);
```

### Renderer `Order`

Renderers are tried in ascending `Order`; the first whose `SupportsContent` returns `true` wins.
Built-in renderers use `100`. The final fallback renderer (`NullMarkdownContentRenderer`) uses
`500`. Register a custom renderer with a lower `Order` to override a built-in renderer for the
same node type, or to override the fallback for entirely unknown node types.

### Writers and writer decorators

`IMarkdownWriter` is a minimal, synchronous text-flow abstraction (`Write`, `WriteLine`,
`WriteLine(string)`) — it knows nothing about Markdown semantics. Parent/container renderers own
the formatting effects they introduce and apply them by wrapping the writer passed to their
children, so a child renderer never needs to know it is nested inside a block quote or list:

- `StringBuilderMarkdownWriter` — the default root writer, backed by a `StringBuilder`.
- `PrefixMarkdownWriter` — prefixes every produced line (used for block quotes, and for list
  markers via distinct first-line/continuation prefixes).
- `IndentMarkdownWriter` — indents every produced line by a fixed amount.

These are public so custom renderers can reuse exactly the same mechanisms as the built-in block
quote and list renderers.

### Unsupported nodes

Any `IContent` not claimed by a renderer (including a custom one) is handled by
`NullMarkdownContentRenderer`: it reports `MarkdownRenderingIssueKind.UnsupportedContent` and
preserves child content where the node exposes any meaningful children, otherwise it emits
nothing. It never throws.

## Licence

MIT. See [LICENSE](LICENSE).
