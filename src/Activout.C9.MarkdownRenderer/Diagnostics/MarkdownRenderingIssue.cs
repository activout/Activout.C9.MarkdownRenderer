using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Diagnostics;

/// <summary>
/// Describes a point where rendering continued but some fidelity may have been lost.
/// </summary>
public sealed class MarkdownRenderingIssue
{
    /// <summary>
    /// The category of the issue.
    /// </summary>
    public required MarkdownRenderingIssueKind Kind { get; init; }

    /// <summary>
    /// The content node the issue relates to, if any.
    /// </summary>
    public IContent? Content { get; init; }

    /// <summary>
    /// A human-readable description of the issue.
    /// </summary>
    public string? Message { get; init; }
}
