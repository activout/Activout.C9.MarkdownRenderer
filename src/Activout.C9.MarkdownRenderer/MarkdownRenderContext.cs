namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// Immutable, data-only structural context passed through rendering. Carries the chain of
/// container ancestors for the node currently being rendered, for the rare cases where a
/// renderer genuinely needs structural information. Formatting effects owned by a container
/// (indentation, block quote prefixes, etc.) are applied through writer decorators, not by
/// descendants inspecting this context.
/// </summary>
public sealed class MarkdownRenderContext
{
    private static readonly IReadOnlyList<MarkdownContainer> RootAncestors = Array.Empty<MarkdownContainer>();

    /// <summary>
    /// Creates the root context, with no ancestors.
    /// </summary>
    public MarkdownRenderContext() : this(RootAncestors)
    {
    }

    private MarkdownRenderContext(IReadOnlyList<MarkdownContainer> ancestors)
    {
        Ancestors = ancestors;
    }

    /// <summary>
    /// The chain of container ancestors, outermost first, of the node currently being rendered.
    /// </summary>
    public IReadOnlyList<MarkdownContainer> Ancestors { get; }

    /// <summary>
    /// Returns a new context with <paramref name="container"/> appended as the innermost ancestor.
    /// </summary>
    public MarkdownRenderContext WithAncestor(MarkdownContainer container)
    {
        var ancestors = new List<MarkdownContainer>(Ancestors.Count + 1);
        ancestors.AddRange(Ancestors);
        ancestors.Add(container);
        return new MarkdownRenderContext(ancestors);
    }
}
