using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class ListTests
{
    [Fact]
    public async Task UnorderedList_RendersDashMarkers()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(UnorderedList(
            Item(Paragraph(Text("First"))),
            Item(Paragraph(Text("Second"))),
            Item(Paragraph(Text("Third")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("- First\n- Second\n- Third\n", result);
    }

    [Fact]
    public async Task OrderedList_RendersSequenceNumbers()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(OrderedList(
            Item(Paragraph(Text("First"))),
            Item(Paragraph(Text("Second"))),
            Item(Paragraph(Text("Third")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("1. First\n2. Second\n3. Third\n", result);
    }

    [Fact]
    public async Task NestedUnorderedList_IsIndented()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(UnorderedList(
            Item(Paragraph(Text("One")),
                UnorderedList(
                    Item(Paragraph(Text("One A"))),
                    Item(Paragraph(Text("One B"))))),
            Item(Paragraph(Text("Two")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("- One\n  - One A\n  - One B\n- Two\n", result);
    }

    [Fact]
    public async Task NestedOrderedList_IsIndented()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(OrderedList(
            Item(Paragraph(Text("One")),
                OrderedList(
                    Item(Paragraph(Text("One A"))),
                    Item(Paragraph(Text("One B"))))),
            Item(Paragraph(Text("Two")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("1. One\n   1. One A\n   2. One B\n2. Two\n", result);
    }

    [Fact]
    public async Task MixedNestedLists_ComposeCorrectly()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(OrderedList(
            Item(Paragraph(Text("First")),
                UnorderedList(
                    Item(Paragraph(Text("Child"))),
                    Item(Paragraph(Text("Child"))))),
            Item(Paragraph(Text("Second")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("1. First\n   - Child\n   - Child\n2. Second\n", result);
    }

    [Fact]
    public async Task MultipleParagraphsInsideListItem_SeparatedByBlankLine()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(UnorderedList(
            Item(Paragraph(Text("First paragraph.")), Paragraph(Text("Second paragraph.")))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("- First paragraph.\n\n  Second paragraph.\n", result);
    }

    [Fact]
    public async Task BlockQuoteInsideListItem_RendersQuotedContent()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(UnorderedList(
            Item(Quote(Paragraph(Text("Quoted"))))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("- > Quoted\n", result);
    }
}
