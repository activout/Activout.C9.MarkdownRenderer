using Activout.C9.MarkdownRenderer.Diagnostics;

namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// Options for <see cref="MarkdownRenderer"/>.
/// </summary>
public sealed class MarkdownRendererOptions
{
    /// <summary>
    /// Optional callback invoked whenever rendering degrades fidelity but continues.
    /// If not set, rendering continues silently.
    /// </summary>
    public Action<MarkdownRenderingIssue>? RenderingIssue { get; set; }
}
