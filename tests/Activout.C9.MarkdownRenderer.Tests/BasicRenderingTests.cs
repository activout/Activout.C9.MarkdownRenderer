using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Tests.Support;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class BasicRenderingTests
{
    [Fact]
    public async Task NullDocument_ReturnsEmptyString()
    {
        var renderer = new MarkdownRenderer();

        var result = await renderer.ToMarkdown(null);

        Assert.Equal("", result);
    }

    [Fact]
    public async Task EmptyDocument_ReturnsEmptyString()
    {
        var renderer = new MarkdownRenderer();

        var result = await renderer.ToMarkdown(Doc());

        Assert.Equal("", result);
    }

    [Fact]
    public async Task PlainParagraph_RendersText()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("Hello world.")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("Hello world.\n", result);
    }

    [Fact]
    public async Task MultipleParagraphs_SeparatedByBlankLine()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(
            Paragraph(Text("First paragraph.")),
            Paragraph(Text("Second paragraph.")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("First paragraph.\n\nSecond paragraph.\n", result);
    }

    [Theory]
    [InlineData(1, "#")]
    [InlineData(2, "##")]
    [InlineData(3, "###")]
    [InlineData(4, "####")]
    [InlineData(5, "#####")]
    [InlineData(6, "######")]
    public async Task Heading_RendersAtxSyntax(int level, string hashes)
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Heading(level, Text("Title")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal($"{hashes} Title\n", result);
    }

    [Fact]
    public async Task HorizontalRuler_RendersThreeDashes()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("Before")), Hr(), Paragraph(Text("After")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("Before\n\n---\n\nAfter\n", result);
    }

    [Fact]
    public async Task MarkdownSpecialCharactersInNormalText_AreEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("Use `code`, [brackets], <tags>, and a\\backslash.")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "Use \\`code\\`, \\[brackets\\], \\<tags\\>, and a\\\\backslash.\n",
            result);
    }

    [Fact]
    public async Task AsteriskSurroundedBySpaces_IsNotEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("2 * 3 = 6")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("2 * 3 = 6\n", result);
    }

    [Fact]
    public async Task TextStartingWithDash_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("- Not a list")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("\\- Not a list\n", result);
    }

    [Fact]
    public async Task TextStartingWithPlus_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("+ Not a list")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("\\+ Not a list\n", result);
    }

    [Fact]
    public async Task TextStartingWithAsteriskBullet_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("* Not a list")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("\\* Not a list\n", result);
    }

    [Fact]
    public async Task TextStartingWithOrderedMarker_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("1. Not a list")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("1\\. Not a list\n", result);
    }

    [Fact]
    public async Task BulletLikeCharactersMidSentence_AreNotEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Text("a - b + c * d")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("a - b + c * d\n", result);
    }

    [Fact]
    public async Task UnsupportedNode_UsesFallbackAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions
        {
            RenderingIssue = i => issue = i
        });
        var doc = Doc(new UnknownContent());

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.UnsupportedContent, issue!.Kind);
    }
}
