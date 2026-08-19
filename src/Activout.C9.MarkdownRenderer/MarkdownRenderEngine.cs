using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// Stores registered <see cref="IMarkdownContentRenderer"/> implementations, selects the correct
/// renderer for a given <see cref="IContent"/> node, and dispatches rendering recursively.
/// </summary>
public sealed class MarkdownRenderEngine
{
    private readonly List<IMarkdownContentRenderer> _renderers = [];
    private readonly Action<MarkdownRenderingIssue>? _issueCallback;
    private List<IMarkdownContentRenderer>? _sortedRenderers;

    /// <summary>
    /// Creates an engine, optionally reporting rendering issues to <paramref name="issueCallback"/>.
    /// </summary>
    public MarkdownRenderEngine(Action<MarkdownRenderingIssue>? issueCallback = null)
    {
        _issueCallback = issueCallback;
    }

    /// <summary>
    /// Registers a renderer.
    /// </summary>
    public void AddRenderer(IMarkdownContentRenderer renderer)
    {
        _renderers.Add(renderer);
        _sortedRenderers = null;
    }

    /// <summary>
    /// Registers multiple renderers.
    /// </summary>
    public void AddRenderers(IEnumerable<IMarkdownContentRenderer> renderers)
    {
        _renderers.AddRange(renderers);
        _sortedRenderers = null;
    }

    /// <summary>
    /// Selects the renderer with the lowest <see cref="IMarkdownContentRenderer.Order"/> whose
    /// <see cref="IMarkdownContentRenderer.SupportsContent"/> returns <c>true</c> for <paramref name="content"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No registered renderer supports the content, and no fallback renderer is registered.</exception>
    public IMarkdownContentRenderer GetRendererForContent(IContent content)
    {
        foreach (var renderer in GetSortedRenderers())
        {
            if (renderer.SupportsContent(content))
            {
                return renderer;
            }
        }

        throw new InvalidOperationException(
            $"No renderer registered for content type '{content.GetType()}'. " +
            "Register a fallback renderer (e.g. NullMarkdownContentRenderer) to handle all content.");
    }

    /// <summary>
    /// Selects and invokes the correct renderer for <paramref name="content"/>.
    /// </summary>
    public Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var renderer = GetRendererForContent(content);
        return renderer.Render(content, context, writer);
    }

    /// <summary>
    /// Reports a rendering issue to the configured diagnostics callback, if any. Built-in and
    /// custom renderers may call this to surface degraded rendering without throwing.
    /// </summary>
    public void ReportIssue(MarkdownRenderingIssue issue) => _issueCallback?.Invoke(issue);

    private List<IMarkdownContentRenderer> GetSortedRenderers() =>
        _sortedRenderers ??= _renderers.OrderBy(r => r.Order).ToList();
}
