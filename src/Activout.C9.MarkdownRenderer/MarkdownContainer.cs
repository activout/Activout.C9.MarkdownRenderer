namespace Activout.C9.MarkdownRenderer;

/// <summary>
/// The kind of structural container an ancestor node represents, for use by
/// <see cref="MarkdownRenderContext.Ancestors"/>.
/// </summary>
public enum MarkdownContainer
{
    /// <summary>The root document.</summary>
    Document,

    /// <summary>A paragraph.</summary>
    Paragraph,

    /// <summary>A block quote.</summary>
    BlockQuote,

    /// <summary>An ordered or unordered list.</summary>
    List,

    /// <summary>A list item.</summary>
    ListItem,

    /// <summary>A table.</summary>
    Table,

    /// <summary>A table row.</summary>
    TableRow,

    /// <summary>A table cell or header.</summary>
    TableCell
}
