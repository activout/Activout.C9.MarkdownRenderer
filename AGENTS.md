# AGENTS.md

Guidance for AI coding agents working in this repository.

## What this is

`Activout.C9.MarkdownRenderer` is a .NET 10 library that converts Contentful Rich Text
(`Contentful.Core.Models.Document`) into GitHub-Flavoured Markdown, primarily for LLM
consumption. It is a best-effort renderer: content-related problems should degrade gracefully
(reported via `MarkdownRenderingIssue`) rather than throw; exceptions from user-supplied custom
renderers must propagate.

## Layout

```
src/Activout.C9.MarkdownRenderer/       the library (namespace Activout.C9.MarkdownRenderer)
  Writers/                              IMarkdownWriter and decorators
  Diagnostics/                          MarkdownRenderingIssue(Kind)
  Internal/                             escaping, URL safety, block-joining, table analysis/HTML fallback
  Renderers/                            one IMarkdownContentRenderer per Rich Text node type
tests/Activout.C9.MarkdownRenderer.Tests/  xUnit tests, one file per concern area
  Support/DocBuilder.cs                 terse builders for constructing Contentful nodes in tests
```

## Build / test / pack

```bash
dotnet build -c Release      # warnings are errors (Directory.Build.props) — must be 0 warnings
dotnet test -c Release
dotnet pack src/Activout.C9.MarkdownRenderer/Activout.C9.MarkdownRenderer.csproj -c Release -o artifacts
```

Run all three before considering a change done. `artifacts/` and `bin/`/`obj/` are git-ignored.

## Architecture rules (do not casually deviate from these)

- **Container ownership**: a parent renderer owns the formatting effects it introduces (block
  quote `>` prefixes, list markers/indentation) and applies them by wrapping the `IMarkdownWriter`
  passed to its children. Child renderers (text, paragraph, etc.) must never inspect
  `MarkdownRenderContext.Ancestors` to reproduce a parent's formatting.
- **`IMarkdownWriter` stays generic**: `Write`, `WriteLine`, `WriteLine(string)` only. No
  Markdown-semantic methods like `WriteHeading`. New cross-cutting writer behavior belongs in a
  new decorator (like `PrefixMarkdownWriter`/`IndentMarkdownWriter`), not new interface methods.
- **`MarkdownRenderContext` is data only** (the `Ancestors` chain). No rendering, no
  service-locator behavior.
- **`MarkdownRenderEngine`** is the renderer registry + dispatcher + issue-reporting sink
  (`ReportIssue`). Renderers receive it once via constructor, not per-`Render` call.
- **Block-level renderers write no trailing newline.** Sibling blocks are joined by
  `Internal/BlockRendering.RenderBlocks`, which buffers each child to detect empty output (so
  empty blocks don't leave stray blank lines) and inserts exactly one blank line between blocks —
  except inside a list item, where a blank line is only inserted between two consecutive
  `Paragraph` children; anything else (e.g. a nested list right after text) stays "tight" per
  CommonMark, matching the spec's own nested-list example.
- **Complex tables render as self-contained HTML**, not a Markdown/HTML mix — see
  `Internal/CellHtmlRenderer.cs`. A table only becomes a GFM pipe table when every cell is
  simple inline content with no rowspan/colspan (`Internal/InlineContentAnalyzer.cs` decides).
- **Renderer `Order`**: built-in renderers use 100, the fallback (`NullMarkdownContentRenderer`)
  uses 500. Lower wins ties in `SupportsContent`. Preserve this when adding renderers so custom
  renderers can still override built-ins by registering with a lower `Order`.
- Escaping (`Internal/MarkdownEscaping.cs`) is centralized and context-aware — it must not
  over-escape (e.g. `2 * 3 = 6` stays unescaped since neither side is "word-flanked"). Don't
  duplicate escaping logic in individual renderers.

## Conventions

- Async methods return `Task`/`Task<T>` with **no `Async` suffix** (`Render`, not `RenderAsync`).
- Nullable reference types are on; treat compiler warnings as build failures — fix them, don't
  suppress (`Directory.Build.props` sets `TreatWarningsAsErrors`, and there's no blanket `NoWarn`).
- Every public member needs an XML doc `<summary>` (the library has `GenerateDocumentationFile`
  on with no `NoWarn` for missing docs — undocumented public API fails the build).
- Tests use exact string equality (`Assert.Equal`) against full rendered output, not `Contains`,
  except where the output is intentionally HTML-fallback and only specific fragments matter.
  Build new Contentful nodes via `Tests/Support/DocBuilder.cs` rather than hand-rolling model
  objects.
- No DI framework, no Markdown-parsing library, no `Task.Run` to fake async — see the "Non-goals"
  and "Code quality" sections of the original spec (kept locally as
  `~/Downloads/Activout.C9.MarkdownRenderer-SPEC.md` — not part of this repo, but the source of
  truth for intent if you need to check whether a behavior is deliberate).

## CI

`.github/workflows/ci.yml` runs on every push/PR: restore, build, test, pack. Keep it green.
`.github/workflows/publish.yml` packs and pushes to NuGet.org on `v*` tags using the
`NUGET_API_KEY` repo secret — don't change the version scheme without updating both.
