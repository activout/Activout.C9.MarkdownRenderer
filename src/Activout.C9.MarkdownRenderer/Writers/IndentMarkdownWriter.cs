namespace Activout.C9.MarkdownRenderer.Writers;

/// <summary>
/// Wraps another <see cref="IMarkdownWriter"/>, indenting every line it produces by a fixed amount.
/// Suitable for list item continuation content.
/// </summary>
public sealed class IndentMarkdownWriter : IMarkdownWriter
{
    private readonly PrefixMarkdownWriter _prefixWriter;

    /// <summary>
    /// Creates a writer that indents every line with <paramref name="indent"/>.
    /// </summary>
    public IndentMarkdownWriter(IMarkdownWriter inner, string indent)
    {
        _prefixWriter = new PrefixMarkdownWriter(inner, indent);
    }

    /// <inheritdoc />
    public void Write(string value) => _prefixWriter.Write(value);

    /// <inheritdoc />
    public void WriteLine() => _prefixWriter.WriteLine();

    /// <inheritdoc />
    public void WriteLine(string value) => _prefixWriter.WriteLine(value);
}
