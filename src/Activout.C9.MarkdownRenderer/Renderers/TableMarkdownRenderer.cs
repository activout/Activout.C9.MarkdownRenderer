using Activout.C9.MarkdownRenderer.Diagnostics;
using Activout.C9.MarkdownRenderer.Internal;
using Activout.C9.MarkdownRenderer.Writers;
using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Renderers;

/// <summary>
/// Renders a <see cref="Table"/> node: as a GFM pipe table when the whole table can be
/// represented reliably, otherwise as an HTML table fragment.
/// </summary>
public sealed class TableMarkdownRenderer : IMarkdownContentRenderer
{
    private readonly MarkdownRenderEngine _renderEngine;

    /// <summary>
    /// Creates the renderer.
    /// </summary>
    public TableMarkdownRenderer(MarkdownRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public bool SupportsContent(IContent content) => content is Table;

    /// <inheritdoc />
    public async Task Render(IContent content, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var table = (Table)content;
        var tableContent = table.Content ?? [];
        var rows = tableContent.OfType<TableRow>().ToList();

        if (rows.Count == 0 || rows.Count != tableContent.Count)
        {
            _renderEngine.ReportIssue(new MarkdownRenderingIssue
            {
                Kind = MarkdownRenderingIssueKind.InvalidStructure,
                Content = content,
                Message = "Table does not consist entirely of rows; preserving content best-effort."
            });

            await BlockRendering.RenderBlocks(
                _renderEngine, tableContent, context.WithAncestor(MarkdownContainer.Table), writer);
            return;
        }

        var rowCells = rows.Select(r => (IReadOnlyList<IContent>)(r.Content ?? [])).ToList();

        if (IsSimpleTable(rowCells))
        {
            await RenderSimpleTable(rowCells, context, writer);
        }
        else
        {
            writer.Write(RenderComplexTableHtml(rows));
        }
    }

    private static bool IsSimpleTable(IReadOnlyList<IReadOnlyList<IContent>> rowCells)
    {
        var columnCount = rowCells[0].Count;
        if (columnCount == 0) return false;

        foreach (var cells in rowCells)
        {
            if (cells.Count != columnCount) return false;

            foreach (var cell in cells)
            {
                var (cellContent, data) = cell switch
                {
                    TableCell tc => ((IReadOnlyList<IContent>)(tc.Content ?? []), tc.Data),
                    TableHeader th => ((IReadOnlyList<IContent>)(th.Content ?? []), th.Data),
                    _ => (null, null)
                };

                if (cellContent is null) return false;
                if (data is { Rowspan: > 1 } or { Colspan: > 1 }) return false;
                if (!InlineContentAnalyzer.IsSimpleCell(cellContent)) return false;
            }
        }

        return true;
    }

    private async Task RenderSimpleTable(
        IReadOnlyList<IReadOnlyList<IContent>> rowCells, MarkdownRenderContext context, IMarkdownWriter writer)
    {
        var columnCount = rowCells[0].Count;
        var cellContext = context
            .WithAncestor(MarkdownContainer.Table)
            .WithAncestor(MarkdownContainer.TableRow)
            .WithAncestor(MarkdownContainer.TableCell);

        var renderedRows = new List<string[]>(rowCells.Count);
        foreach (var cells in rowCells)
        {
            var renderedCells = new string[columnCount];
            for (var c = 0; c < columnCount; c++)
            {
                renderedCells[c] = await RenderCellInline(cells[c], cellContext);
            }

            renderedRows.Add(renderedCells);
        }

        writer.Write(FormatRow(renderedRows[0]));
        writer.WriteLine();
        writer.Write(FormatRow(Enumerable.Repeat("---", columnCount).ToArray()));

        for (var r = 1; r < renderedRows.Count; r++)
        {
            writer.WriteLine();
            writer.Write(FormatRow(renderedRows[r]));
        }
    }

    private async Task<string> RenderCellInline(IContent cell, MarkdownRenderContext context)
    {
        var content = cell switch
        {
            TableCell tc => tc.Content ?? [],
            TableHeader th => th.Content ?? [],
            _ => []
        };

        var buffer = new StringBuilderMarkdownWriter();
        foreach (var child in content)
        {
            if (child is Paragraph paragraph)
            {
                foreach (var inline in paragraph.Content ?? [])
                {
                    await _renderEngine.Render(inline, context, buffer);
                }
            }
            else
            {
                await _renderEngine.Render(child, context, buffer);
            }
        }

        return buffer.ToString().Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");
    }

    private static string FormatRow(IReadOnlyList<string> cells) => "| " + string.Join(" | ", cells) + " |";

    private static string RenderComplexTableHtml(List<TableRow> rows)
    {
        var sb = new System.Text.StringBuilder("<table>\n");
        foreach (var row in rows)
        {
            sb.Append(CellHtmlRenderer.Render(row));
            sb.Append('\n');
        }

        sb.Append("</table>");
        return sb.ToString();
    }
}
