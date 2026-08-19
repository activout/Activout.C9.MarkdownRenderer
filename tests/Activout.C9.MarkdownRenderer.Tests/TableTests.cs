using Activout.C9.MarkdownRenderer.Diagnostics;
using Contentful.Core.Models;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class TableTests
{
    [Fact]
    public async Task SimpleTable_RendersGfmPipeTable()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("Name"), Header("Description")),
            Row(Cell("Foo"), CellBlocks(Paragraph(Text("A "), Text("useful", "bold"), Text(" thing")))),
            Row(Cell("Bar"), Cell("Another thing"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "| Name | Description |\n" +
            "| --- | --- |\n" +
            "| Foo | A **useful** thing |\n" +
            "| Bar | Another thing |\n",
            result);
    }

    [Fact]
    public async Task PipeCharacterInsideCell_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A"), Header("B")),
            Row(Cell("x | y"), Cell("z"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "| A | B |\n" +
            "| --- | --- |\n" +
            "| x \\| y | z |\n",
            result);
    }

    [Fact]
    public async Task TableWithoutExplicitHeader_UsesFirstRowAsHeader()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Cell("Name"), Cell("Description")),
            Row(Cell("Foo"), Cell("Bar"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "| Name | Description |\n" +
            "| --- | --- |\n" +
            "| Foo | Bar |\n",
            result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    public async Task TableCellSpanOfOneOrZeroOrNull_StaysSimpleGfmTable(int? span)
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A"), Header("B")),
            Row(Cell("x", rowspan: span, colspan: span), Cell("y", rowspan: span, colspan: span))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal(
            "| A | B |\n" +
            "| --- | --- |\n" +
            "| x | y |\n",
            result);
    }

    [Fact]
    public async Task TableWithRowspan_FallsBackToHtml()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A"), Header("B")),
            Row(Cell("x", rowspan: 2), Cell("y")),
            Row(Cell("z"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Contains("<table>", result);
        Assert.Contains("rowspan=\"2\"", result);
    }

    [Fact]
    public async Task TableWithColspan_FallsBackToHtml()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A"), Header("B")),
            Row(Cell("x", colspan: 2))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Contains("<table>", result);
        Assert.Contains("colspan=\"2\"", result);
    }

    [Fact]
    public async Task TableWithMultipleParagraphsInCell_FallsBackToHtml()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A")),
            Row(CellBlocks(Paragraph(Text("First")), Paragraph(Text("Second"))))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Contains("<table>", result);
        Assert.Contains("<p>First</p>", result);
        Assert.Contains("<p>Second</p>", result);
    }

    [Fact]
    public async Task TableWithNestedListInCell_FallsBackToHtml()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Table(
            Row(Header("A")),
            Row(CellBlocks(UnorderedList(Item(Paragraph(Text("One"))), Item(Paragraph(Text("Two"))))))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Contains("<table>", result);
        Assert.Contains("<ul>", result);
        Assert.Contains("<li><p>One</p></li>", result);
    }

    [Fact]
    public async Task MalformedTableStructure_PreservesContentAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var table = new Table { NodeType = "table", Content = [Paragraph(Text("Not a row"))] };
        var doc = Doc(table);

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("Not a row\n", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.InvalidStructure, issue!.Kind);
    }
}
