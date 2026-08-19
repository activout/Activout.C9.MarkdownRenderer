using Activout.C9.MarkdownRenderer.Writers;

namespace Activout.C9.MarkdownRenderer.Tests;

public class WriterTests
{
    [Fact]
    public void StringBuilderMarkdownWriter_AccumulatesWrites()
    {
        var writer = new StringBuilderMarkdownWriter();

        writer.Write("Hello, ");
        writer.WriteLine("world!");
        writer.Write("Second line");

        Assert.Equal("Hello, world!\nSecond line", writer.ToString());
    }

    [Fact]
    public void PrefixMarkdownWriter_PrefixesEveryProducedLine()
    {
        var root = new StringBuilderMarkdownWriter();
        var writer = new PrefixMarkdownWriter(root, "> ");

        writer.WriteLine("First paragraph.");
        writer.WriteLine();
        writer.Write("- One");
        writer.WriteLine();
        writer.Write("- Two");

        Assert.Equal("> First paragraph.\n>\n> - One\n> - Two", root.ToString());
    }

    [Fact]
    public void PrefixMarkdownWriter_SupportsDistinctFirstLineAndContinuationPrefixes()
    {
        var root = new StringBuilderMarkdownWriter();
        var writer = new PrefixMarkdownWriter(root, "- ", "  ");

        writer.Write("First paragraph.");
        writer.WriteLine();
        writer.WriteLine();
        writer.Write("Second paragraph.");

        Assert.Equal("- First paragraph.\n\n  Second paragraph.", root.ToString());
    }

    [Fact]
    public void NestedWriterDecorators_ComposeCorrectly()
    {
        var root = new StringBuilderMarkdownWriter();
        var quoteWriter = new PrefixMarkdownWriter(root, "> ");
        var indentWriter = new IndentMarkdownWriter(quoteWriter, "  ");

        indentWriter.Write("nested");

        Assert.Equal("> ", root.ToString()[..2]);
        Assert.Equal("> " + "  " + "nested", root.ToString());
    }

    [Fact]
    public void PrefixMarkdownWriter_HandlesMultiLineWriteInASingleCall()
    {
        var root = new StringBuilderMarkdownWriter();
        var writer = new PrefixMarkdownWriter(root, "> ");

        writer.Write("line one\nline two");

        Assert.Equal("> line one\n> line two", root.ToString());
    }
}
