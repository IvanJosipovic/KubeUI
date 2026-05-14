using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

internal sealed class YamlDiagnosticRenderer : IBackgroundRenderer, ITextViewConnect
{
    private static readonly IPen s_diagnosticPen = new Pen(Brushes.IndianRed, 1);
    private static readonly IBrush s_diagnosticBackground = new SolidColorBrush(Color.FromArgb(32, 205, 92, 92));

    private readonly record struct DiagnosticSpan(int Offset, int Length, string Message);

    private IReadOnlyList<DiagnosticSpan> _spans = [];
    private TextView? _textView;

    public KnownLayer Layer => KnownLayer.Caret;

    public IReadOnlyList<string> Messages => _spans.Select(x => x.Message).ToArray();

    public void AddToTextView(TextView textView)
    {
        _textView = textView;
    }

    public void RemoveFromTextView(TextView textView)
    {
        if (ReferenceEquals(_textView, textView))
        {
            _textView = null;
        }
    }

    public void Update(TextDocument? document, IReadOnlyList<YamlDiagnostic> diagnostics)
    {
        _spans = document == null ? [] : BuildSpans(document, diagnostics);
        _textView?.Redraw();
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        foreach (var span in _spans)
        {
            var segment = new TextSegment
            {
                StartOffset = span.Offset,
                Length = span.Length,
            };

            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
            {
                drawingContext.FillRectangle(s_diagnosticBackground, rect);

                var y = rect.Bottom - 1;
                for (var x = rect.Left; x < rect.Right; x += 4)
                {
                    var peak = Math.Min(x + 2, rect.Right);
                    var nextX = Math.Min(x + 4, rect.Right);
                    drawingContext.DrawLine(s_diagnosticPen, new Point(x, y), new Point(peak, y - 2));
                    drawingContext.DrawLine(s_diagnosticPen, new Point(peak, y - 2), new Point(nextX, y));
                }
            }
        }
    }

    public string? TryGetMessageAt(int offset)
    {
        foreach (var span in _spans)
        {
            if (offset >= span.Offset && offset <= span.Offset + span.Length)
            {
                return span.Message;
            }
        }

        return null;
    }

    private static IReadOnlyList<DiagnosticSpan> BuildSpans(TextDocument document, IReadOnlyList<YamlDiagnostic> diagnostics)
    {
        var spans = new List<DiagnosticSpan>(diagnostics.Count);
        foreach (var diagnostic in diagnostics)
        {
            var startOffset = GetOffset(document, diagnostic.StartLine, diagnostic.StartColumn);
            var endOffset = GetOffset(document, diagnostic.EndLine, diagnostic.EndColumn);
            if (endOffset < startOffset)
            {
                (startOffset, endOffset) = (endOffset, startOffset);
            }

            if (startOffset >= document.TextLength && document.TextLength > 0)
            {
                startOffset = document.TextLength - 1;
            }

            spans.Add(new DiagnosticSpan(startOffset, Math.Max(1, endOffset - startOffset), diagnostic.Message));
        }

        return spans;
    }

    private static int GetOffset(TextDocument document, int lineNumber, int columnNumber)
    {
        if (document.LineCount == 0)
        {
            return 0;
        }

        var safeLineNumber = Math.Clamp(lineNumber, 1, document.LineCount);
        var line = document.GetLineByNumber(safeLineNumber);
        var safeColumn = Math.Clamp(columnNumber - 1, 0, line.Length);
        return line.Offset + safeColumn;
    }
}
