using System.Text;

namespace Activout.C9.MarkdownRenderer.Writers;

/// <summary>
/// The default root <see cref="IMarkdownWriter"/>, backed by a <see cref="StringBuilder"/>.
/// </summary>
public sealed class StringBuilderMarkdownWriter : IMarkdownWriter
{
    private readonly StringBuilder _builder;

    /// <summary>
    /// Creates a writer backed by a new, empty <see cref="StringBuilder"/>.
    /// </summary>
    public StringBuilderMarkdownWriter() : this(new StringBuilder())
    {
    }

    /// <summary>
    /// Creates a writer backed by the given <see cref="StringBuilder"/>.
    /// </summary>
    public StringBuilderMarkdownWriter(StringBuilder builder)
    {
        _builder = builder;
    }

    /// <inheritdoc />
    public void Write(string value) => _builder.Append(value);

    /// <inheritdoc />
    public void WriteLine() => _builder.Append('\n');

    /// <inheritdoc />
    public void WriteLine(string value)
    {
        _builder.Append(value);
        _builder.Append('\n');
    }

    /// <summary>
    /// Returns the accumulated Markdown text.
    /// </summary>
    public override string ToString() => _builder.ToString();
}
