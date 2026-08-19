using Activout.C9.MarkdownRenderer.Diagnostics;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class MarksTests
{
    [Fact]
    public async Task Bold_RendersDoubleAsterisks()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("bold", "bold")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("**bold**\n", result);
    }

    [Fact]
    public async Task Italic_RendersUnderscore()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("italic", "italic")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("_italic_\n", result);
    }

    [Fact]
    public async Task BoldAndItalic_RendersBothTogether()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("both", "bold", "italic")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("**_both_**\n", result);
    }

    [Fact]
    public async Task Underline_RendersUTag()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("underlined", "underline")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("<u>underlined</u>\n", result);
    }

    [Fact]
    public async Task InlineCode_RendersBackticks()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("code", "code")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("`code`\n", result);
    }

    [Fact]
    public async Task InlineCodeContainingBacktick_UsesLongerDelimiter()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("a ` b", "code")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("``a ` b``\n", result);
    }

    [Fact]
    public async Task Superscript_RendersSupTag()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("2", "superscript")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("<sup>2</sup>\n", result);
    }

    [Fact]
    public async Task Subscript_RendersSubTag()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("2", "subscript")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("<sub>2</sub>\n", result);
    }

    [Fact]
    public async Task UnknownMark_RendersPlainTextAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var doc = Doc(Paragraph(Text("plain", "strikethrough")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("plain\n", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.UnsupportedContent, issue!.Kind);
    }
}
