namespace Activout.C9.MarkdownRenderer.Writers;

/// <summary>
/// A generic, synchronous text-flow writer used to build Markdown output.
/// </summary>
public interface IMarkdownWriter
{
    /// <summary>
    /// Writes a value to the current line without ending it.
    /// </summary>
    void Write(string value);

    /// <summary>
    /// Ends the current line.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes a value followed by a line ending.
    /// </summary>
    void WriteLine(string value);
}
