using Contentful.Core.Models;

namespace Activout.C9.MarkdownRenderer.Tests.Support;

/// <summary>
/// A content node with no registered renderer, used to exercise the fallback renderer.
/// </summary>
internal sealed class UnknownContent : IContent;
