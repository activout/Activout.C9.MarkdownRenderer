using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class EndToEndTests
{
    [Fact]
    public async Task CombinedDocument_RendersExactExpectedMarkdown()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(
            Heading(1, Text("Heading")),
            Paragraph(
                Text("Paragraph with "),
                Text("bold text", "bold"),
                Text(" and a "),
                Link("https://example.com", Text("link")),
                Text(".")),
            UnorderedList(
                Item(Paragraph(Text("First"))),
                Item(
                    Paragraph(Text("Second")),
                    UnorderedList(Item(Paragraph(Text("Nested")))))),
            Quote(Paragraph(Text("Quote"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "# Heading\n" +
            "\n" +
            "Paragraph with **bold text** and a [link](https://example.com).\n" +
            "\n" +
            "- First\n" +
            "- Second\n" +
            "  - Nested\n" +
            "\n" +
            "> Quote\n",
            result);
    }

    [Fact]
    public async Task DocumentWithTableAndAssets_RendersExactExpectedMarkdown()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(
            Heading(2, Text("Gallery")),
            Table(
                Row(Header("Item"), Header("Preview")),
                Row(Cell("Logo"), CellBlocks(Paragraph(Asset("https://images.ctfassets.net/logo.png", "image/png", title: "Logo"))))),
            Paragraph(AssetLink("https://example.com/file.pdf", "Spec", Text("Download the spec"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "## Gallery\n" +
            "\n" +
            "| Item | Preview |\n" +
            "| --- | --- |\n" +
            "| Logo | ![Logo](https://images.ctfassets.net/logo.png) |\n" +
            "\n" +
            "[Download the spec](https://example.com/file.pdf)\n",
            result);
    }
}
