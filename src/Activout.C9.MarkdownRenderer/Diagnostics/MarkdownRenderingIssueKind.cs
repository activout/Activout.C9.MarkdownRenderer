namespace Activout.C9.MarkdownRenderer.Diagnostics;

/// <summary>
/// Categorizes a <see cref="MarkdownRenderingIssue"/>. All kinds mean approximately:
/// rendering continued, but some fidelity may have been lost.
/// </summary>
public enum MarkdownRenderingIssueKind
{
    /// <summary>
    /// The content node type is not recognized by any registered renderer.
    /// </summary>
    UnsupportedContent,

    /// <summary>
    /// Optional or expected data was missing (e.g. an asset with no URL).
    /// </summary>
    MissingData,

    /// <summary>
    /// A known node type was structured unexpectedly (e.g. a malformed table row).
    /// </summary>
    InvalidStructure
}
