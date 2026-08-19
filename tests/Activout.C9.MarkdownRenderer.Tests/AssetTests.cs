using Activout.C9.MarkdownRenderer.Diagnostics;
using static Activout.C9.MarkdownRenderer.Tests.Support.DocBuilder;

namespace Activout.C9.MarkdownRenderer.Tests;

public class AssetTests
{
    [Fact]
    public async Task ImageAsset_RendersMarkdownImage()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("https://images.ctfassets.net/pic.jpg", "image/jpeg", description: "A picture")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("![A picture](https://images.ctfassets.net/pic.jpg)\n", result);
    }

    [Fact]
    public async Task ImageAssetProtocolRelativeUrl_RemainsValid()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("//images.ctfassets.net/pic.jpg", "image/jpeg", title: "Pic")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("![Pic](//images.ctfassets.net/pic.jpg)\n", result);
    }

    [Fact]
    public async Task NonImageAsset_RendersLink()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("https://example.com/file.pdf", "application/pdf", title: "My File")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[My File](https://example.com/file.pdf)\n", result);
    }

    [Fact]
    public async Task AssetHyperlink_RendersLinkWithChildContent()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(AssetLink("https://example.com/file.pdf", "My File", Text("Download PDF"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[Download PDF](https://example.com/file.pdf)\n", result);
    }

    [Fact]
    public async Task MissingAssetTitle_UsesFallbackAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var doc = Doc(Paragraph(Asset("https://example.com/file.pdf", "application/pdf")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[Attachment](https://example.com/file.pdf)\n", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.MissingData, issue!.Kind);
    }

    [Fact]
    public async Task MissingAssetUrl_RendersFallbackTextAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var doc = Doc(Paragraph(Asset(null, "application/pdf", title: "My File")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("My File\n", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.MissingData, issue!.Kind);
    }

    [Fact]
    public async Task AssetWithNoFileAtAll_RendersNothingAndReportsIssue()
    {
        MarkdownRenderingIssue? issue = null;
        var renderer = new MarkdownRenderer(new MarkdownRendererOptions { RenderingIssue = i => issue = i });
        var doc = Doc(Paragraph(AssetWithoutFile()));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("", result);
        Assert.NotNull(issue);
        Assert.Equal(MarkdownRenderingIssueKind.MissingData, issue!.Kind);
    }

    [Fact]
    public async Task ImageAssetWithUnsafeUrlScheme_IsNeutralized()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("javascript:alert(1)", "image/jpeg", title: "Pic")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("![Pic](#)\n", result);
    }

    [Fact]
    public async Task NonImageAssetWithUnsafeUrlScheme_IsNeutralized()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("javascript:alert(1)", "application/pdf", title: "My File")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[My File](#)\n", result);
    }

    [Fact]
    public async Task ImageAssetDescriptionContainingBracket_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset(
            "https://images.ctfassets.net/pic.jpg", "image/jpeg", description: "A [bracketed] picture")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("![A \\[bracketed\\] picture](https://images.ctfassets.net/pic.jpg)\n", result);
    }

    [Fact]
    public async Task NonImageAssetTitleContainingBracket_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(Asset("https://example.com/file.pdf", "application/pdf", title: "[Draft] File")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[\\[Draft\\] File](https://example.com/file.pdf)\n", result);
    }

    [Fact]
    public async Task AssetHyperlinkWithUnsafeUrlScheme_IsNeutralized()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(AssetLink("javascript:alert(1)", "My File", Text("Download"))));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[Download](#)\n", result);
    }

    [Fact]
    public async Task AssetHyperlinkFallbackTitleContainingBracket_IsEscaped()
    {
        var renderer = new MarkdownRenderer();
        var doc = Doc(Paragraph(AssetLink("https://example.com/file.pdf", "[Draft] File")));

        var result = await renderer.ToMarkdown(doc);

        Assert.Equal("[\\[Draft\\] File](https://example.com/file.pdf)\n", result);
    }
}
