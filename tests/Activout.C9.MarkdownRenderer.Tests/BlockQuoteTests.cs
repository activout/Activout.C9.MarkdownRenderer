using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class BlockQuoteTests
{
    [Fact]
    public async Task SimpleBlockQuote_PrefixesLineWithGreaterThan()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Quote(Paragraph(Text("Quoted text."))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("> Quoted text.\n", result);
    }

    [Fact]
    public async Task MultiParagraphBlockQuote_PrefixesBlankLineWithBareMarker()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Quote(
            Paragraph(Text("First paragraph.")),
            Paragraph(Text("Second paragraph."))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("> First paragraph.\n>\n> Second paragraph.\n", result);
    }

    [Fact]
    public async Task ListInsideBlockQuote_EveryLineIsPrefixed()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Quote(
            Paragraph(Text("Some text.")),
            UnorderedList(
                Item(Paragraph(Text("First"))),
                Item(Paragraph(Text("Second"))))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("> Some text.\n>\n> - First\n> - Second\n", result);
    }
}
