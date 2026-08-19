using Activout.C9.MarkdownRenderer.Diagnostics;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class LinksTests
{
    [Fact]
    public async Task Hyperlink_RendersMarkdownLink()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Link("https://example.com", Text("example"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[example](https://example.com)\n", result);
    }

    [Fact]
    public async Task HyperlinkWithTitle_RendersTitleAttribute()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(LinkWithTitle("https://example.com", "Example Site", Text("example"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[example](https://example.com \"Example Site\")\n", result);
    }

    [Fact]
    public async Task MarkedTextInsideHyperlink_RendersMark()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Link("https://example.com", Text("important link", "bold"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[**important link**](https://example.com)\n", result);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("JAVASCRIPT:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("vbscript:msgbox(1)")]
    public async Task UnsafeHyperlinkScheme_IsNeutralized(string unsafeUrl)
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Link(unsafeUrl, Text("click"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[click](#)\n", result);
    }

    [Fact]
    public async Task RelativeHyperlink_IsPreserved()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Link("/about", Text("about"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[about](/about)\n", result);
    }

    [Fact]
    public async Task ProtocolRelativeHyperlink_IsPreserved()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Link("//example.com/about", Text("about"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[about](//example.com/about)\n", result);
    }

    [Fact]
    public async Task HyperlinkTitleContainingQuote_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(LinkWithTitle("https://example.com", "Click \"here\"", Text("example"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[example](https://example.com \"Click \\\"here\\\"\")\n", result);
    }

    [Fact]
    public async Task HyperlinkMissingUri_RendersTextOnlyAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var doc = Doc(Paragraph(Link(null, Text("no link"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("no link\n", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.MissingData, issue!.Kind);
    }
}
