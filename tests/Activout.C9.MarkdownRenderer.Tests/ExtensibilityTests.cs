using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Tests.Support;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class ExtensibilityTests
{
    private sealed class DelegateRenderer(
        int order,
        Func<IContent, bool> supports,
        Func<IContent, MarkdownRenderContext, IMarkdownWriter, Task> render) : IMarkdownContentRenderer
    {
        public int Order => order;
        public bool SupportsContent(IContent content) => supports(content);
        public Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer) =>
            render(content, context, writer);
    }

    [Fact]
    public async Task RenderingIssueCallback_IsInvokedForDegradedContent()
    {
        var issues = new List<MarkdownRenderingIssue>();
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = issues.Add });
        var doc = Doc(Paragraph(new UnknownContent()));

        await renderer.ToMarkdown(doc);

        Assert.Single(issues);
        Assert.Equal(MarkdownRenderingIssueKind.UnsupportedContent, issues[0].Kind);
    }

    [Fact]
    public async Task CustomRenderer_CanOverrideFallbackForUnknownContent()
    {
        var renderer = new MarkdownRenderer();
        renderer.AddRenderer(new DelegateRenderer(
            order: 50,
            supports: c => c is UnknownContent,
            render: (_, _, writer) =>
            {
                writer.Write("custom");
                return Task.CompletedTask;
            }));

        var doc = Doc(Paragraph(new UnknownContent()));
        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("custom\n", result);
    }

    [Fact]
    public async Task CustomRendererWithLowerOrder_OverridesBuiltInRenderer()
    {
        var renderer = new MarkdownRenderer();
        renderer.AddRenderer(new DelegateRenderer(
            order: 10,
            supports: c => c is Paragraph,
            render: (_, _, writer) =>
            {
                writer.Write("overridden");
                return Task.CompletedTask;
            }));

        var doc = Doc(Paragraph(Text("original")));
        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("overridden\n", result);
    }

    [Fact]
    public async Task ExceptionFromCustomRenderer_Propagates()
    {
        var renderer = new MarkdownRenderer();
        renderer.AddRenderer(new DelegateRenderer(
            order: 10,
            supports: c => c is Paragraph,
            render: (_, _, _) => throw new InvalidOperationException("boom")));

        var doc = Doc(Paragraph(Text("text")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => renderer.ToMarkdown(doc));
    }

    [Fact]
    public async Task CustomRenderer_CanRenderChildrenThroughEngineAndReuseWriters()
    {
        var renderer = new MarkdownRenderer();
        renderer.AddRenderer(new DelegateRenderer(
            order: 10,
            supports: c => c is Quote,
            render: async (content, context, writer) =>
            {
                var quote = (Quote)content;
                var quoteWriter = new PrefixMarkdownWriter(writer, "custom> ");
                foreach (var child in quote.Content ?? [])
                {
                    await renderer.RenderEngine.Render(child, context, quoteWriter);
                }
            }));

        var doc = Doc(Quote(Paragraph(Text("Quoted"))));
        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("custom> Quoted\n", result);
    }
}
