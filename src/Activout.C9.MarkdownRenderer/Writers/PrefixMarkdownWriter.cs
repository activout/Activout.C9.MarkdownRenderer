namespace Activout.C9.MarkdownRenderer.Writers;

/// <summary>
/// Wraps another <see cref="IMarkdownWriter"/>, prefixing every line it produces.
/// Suitable for block quotes and list item markers.
/// </summary>
public sealed class PrefixMarkdownWriter : IMarkdownWriter
{
    private readonly IMarkdownWriter _inner;
    private readonly string _firstLinePrefix;
    private readonly string _continuationPrefix;
    private bool _usedFirstLine;
    private bool _atLineStart = true;

    /// <summary>
    /// Creates a writer that applies the same prefix to every line.
    /// </summary>
    public PrefixMarkdownWriter(IMarkdownWriter inner, string prefix)
        : this(inner, prefix, prefix)
    {
    }

    /// <summary>
    /// Creates a writer that applies <paramref name="firstLinePrefix"/> to the first line
    /// and <paramref name="continuationPrefix"/> to every subsequent line.
    /// </summary>
    public PrefixMarkdownWriter(IMarkdownWriter inner, string firstLinePrefix, string continuationPrefix)
    {
        _inner = inner;
        _firstLinePrefix = firstLinePrefix;
        _continuationPrefix = continuationPrefix;
    }

    /// <inheritdoc />
    public void Write(string value)
    {
        if (value.Length == 0) return;

        var segments = value.Split('\n');
        for (var i = 0; i < segments.Length; i++)
        {
            if (i > 0)
            {
                FinishLine();
            }

            var segment = segments[i];
            if (segment.Length == 0) continue;

            WritePrefixIfNeeded();
            _inner.Write(segment);
        }
    }

    /// <inheritdoc />
    public void WriteLine() => FinishLine();

    /// <inheritdoc />
    public void WriteLine(string value)
    {
        Write(value);
        FinishLine();
    }

    private void FinishLine()
    {
        if (_atLineStart)
        {
            var prefix = (_usedFirstLine ? _continuationPrefix : _firstLinePrefix).TrimEnd(' ');
            if (prefix.Length > 0)
            {
                _inner.Write(prefix);
            }

            _usedFirstLine = true;
        }

        _inner.WriteLine();
        _atLineStart = true;
    }

    private void WritePrefixIfNeeded()
    {
        if (!_atLineStart) return;

        _inner.Write(_usedFirstLine ? _continuationPrefix : _firstLinePrefix);
        _usedFirstLine = true;
        _atLineStart = false;
    }
}
