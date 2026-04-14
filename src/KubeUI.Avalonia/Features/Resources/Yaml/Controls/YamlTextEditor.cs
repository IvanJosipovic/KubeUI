using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Controls;

public sealed class YamlTextEditor : TextEditor
{
    private static readonly IPen s_diagnosticPen = new Pen(Brushes.IndianRed, 1);
    private static readonly IBrush s_diagnosticBackground = new SolidColorBrush(Color.FromArgb(32, 205, 92, 92));

    private sealed class YamlDiagnosticRenderer : IBackgroundRenderer, ITextViewConnect
    {
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

    private readonly YamlDiagnosticRenderer _diagnosticRenderer = new();
    private ResourceYamlViewModel? _currentViewModel;

    public YamlTextEditor()
    {
        DataContextChanged += OnDataContextChanged;
        TextChanged += Editor_TextChanged;
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        PointerHover += TextView_PointerHover;
        PointerHoverStopped += TextView_PointerHoverStopped;
        TextArea.TextView.BackgroundRenderers.Add(_diagnosticRenderer);
        ToolTip.SetPlacement(this, PlacementMode.Pointer);
        ToolTip.SetVerticalOffset(this, 14);

        if (DataContext is ResourceYamlViewModel vm)
        {
            AttachViewModel(vm);
            UpdateValidationDiagnostics();
        }
    }

    protected override Type StyleKeyOverride => typeof(TextEditor);

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateValidationDiagnostics();
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseHoverPopups();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        CloseHoverPopups();

        var nextViewModel = DataContext as ResourceYamlViewModel;
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            UpdateValidationDiagnostics();
            return;
        }

        DetachViewModel(_currentViewModel);
        _currentViewModel = nextViewModel;
        AttachViewModel(nextViewModel);
        UpdateValidationDiagnostics();
    }

    private void AttachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged += ViewModelOnPropertyChanged;
        }
    }

    private void DetachViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged -= ViewModelOnPropertyChanged;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResourceYamlViewModel.ValidationDiagnostics))
        {
            UpdateValidationDiagnostics();
        }
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        CloseHoverPopups();
        UpdateValidationDiagnostics();
    }

    private void TextView_PointerHover(object? sender, PointerEventArgs e)
    {
        if (Document == null || _currentViewModel?.Object == null || _currentViewModel.Cluster == null)
        {
            return;
        }

        var position = TextArea.TextView.GetPosition(e.GetPosition(TextArea.TextView));
        if (!position.HasValue)
        {
            position = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView));
        }

        if (!position.HasValue)
        {
            CloseHoverPopups();
            return;
        }

        var offset = Document.GetOffset(position.Value.Location);
        TryShowHoverTooltipAtOffset(offset);
    }

    private void TextView_PointerHoverStopped(object? sender, PointerEventArgs e)
    {
        CloseHoverPopups();
    }

    private bool TryShowHoverTooltipAtOffset(int offset)
    {
        if (Document == null || _currentViewModel?.Object == null || _currentViewModel.Cluster == null)
        {
            return false;
        }

        var context = YamlSchemaContext.Resolve(
            Document,
            offset,
            _currentViewModel.Object.GetType(),
            _currentViewModel.Cluster.ModelCache);

        if (context.Documentation == null || context.CurrentProperty == null || !IsWithinFieldName(Document, offset, context))
        {
            var message = _diagnosticRenderer.TryGetMessageAt(offset);
            if (string.IsNullOrEmpty(message))
            {
                CloseHoverPopups();
                return false;
            }

            ShowHoverToolTip(message);
            return true;
        }

        ShowHoverToolTip(YamlDocumentationViewFactory.Create(context.Documentation));
        return true;
    }

    private static bool IsWithinFieldName(TextDocument document, int offset, YamlContextResult context)
    {
        var currentLine = document.GetLineByOffset(offset);
        var startOffset = currentLine.Offset + context.KeyStartColumn;
        var endOffset = currentLine.Offset + context.KeyEndColumn;
        return offset >= startOffset && offset <= endOffset;
    }

    private void UpdateValidationDiagnostics()
    {
        _diagnosticRenderer.Update(Document, _currentViewModel?.ValidationDiagnostics ?? []);
    }

    private void CloseDiagnosticToolTip()
    {
        ToolTip.SetIsOpen(this, false);
    }

    private void ShowHoverToolTip(object tip)
    {
        ToolTip.SetTip(this, tip);
        ToolTip.SetIsOpen(this, true);
    }

    private void CloseHoverPopups()
    {
        CloseDiagnosticToolTip();
        ToolTip.SetTip(this, null);
    }
}
