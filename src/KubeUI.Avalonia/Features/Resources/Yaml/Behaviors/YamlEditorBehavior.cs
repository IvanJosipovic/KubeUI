using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Indentation;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlEditorBehavior : Behavior<TextEditor>
{
    private const int IndentationSize = 2;
    private static readonly IIndentationStrategy s_yamlIndentationStrategy = new YamlIndentationStrategy();
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

    private sealed class EditorInputHandler(
        TextArea textArea,
        System.Action triggerCompletion,
        Func<bool> triggerBackspaceUnindent,
        Func<bool> triggerSelectionIndent,
        Func<bool> triggerSelectionUnindent) : TextAreaStackedInputHandler(textArea)
    {
        public override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                triggerCompletion();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.None && triggerSelectionIndent())
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back && e.KeyModifiers == KeyModifiers.None && triggerBackspaceUnindent())
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.Shift && triggerSelectionUnindent())
            {
                e.Handled = true;
            }
        }
    }

    private Installation? _textMateInstallation;
    private RegistryOptions _registryOptions = null!;
    private FoldingManager? _foldingManager;
    private ResourceYamlViewModel? _currentViewModel;
    private readonly Dictionary<string, Queue<bool>> _savedFoldStates = new(StringComparer.Ordinal);
    private bool _isRefreshingFromViewModel;
    private CompletionWindow? _completionWindow;
    private OverloadInsightWindow? _insightWindow;
    private EditorInputHandler? _editorInputHandler;
    private readonly YamlDiagnosticRenderer _diagnosticRenderer = new();

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        _registryOptions = new RegistryOptions(Application.Current!.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus);

        // Keep behavior lifecycle tied to ViewModel attach/detach only.

        AssociatedObject.TextChanged += Editor_TextChanged;
        Application.Current!.ActualThemeVariantChanged += ThemeChanged;

        AssociatedObject.DataContextChanged += OnDataContextChanged;
        AssociatedObject.AttachedToVisualTree += AttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += DetachedFromVisualTree;
        AssociatedObject.TextArea.TextView.PointerReleased += TextView_PointerReleased;
        AssociatedObject.TextArea.TextView.PointerHover += TextView_PointerHover;
        AssociatedObject.TextArea.TextView.PointerHoverStopped += TextView_PointerHoverStopped;
        AssociatedObject.TextArea.TextView.BackgroundRenderers.Add(_diagnosticRenderer);
        AssociatedObject.TextArea.TextEntering += TextArea_TextEntering;
        AssociatedObject.TextArea.TextEntered += TextArea_TextEntered;
        AssociatedObject.TextArea.SelectionChanged += TextArea_SelectionChanged;
        AssociatedObject.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        _editorInputHandler = new EditorInputHandler(
            AssociatedObject.TextArea,
            ShowCompletionWindow,
            TryUnindentOnBackspace,
            TryIndentSelection,
            TryUnindentSelection);
        AssociatedObject.TextArea.PushStackedInputHandler(_editorInputHandler);

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            _currentViewModel = vm;
            SubscribeViewModel(vm);
            InitializeEditor(vm);
        }
    }

    // Factory event wiring removed. Behavior relies on DataContext attach/detach
    // and visual tree attach/detach for fold-state persistence.
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        var nextViewModel = AssociatedObject.DataContext as ResourceYamlViewModel;
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            if (nextViewModel != null)
            {
                InitializeEditor(nextViewModel);
            }

            return;
        }

        var previousViewModel = _currentViewModel;
        PersistFoldingState(previousViewModel, persistToViewModel: true);
        UnsubscribeViewModel(_currentViewModel);
        _currentViewModel = nextViewModel;

        if (nextViewModel != null && !ReferenceEquals(previousViewModel, nextViewModel))
        {
            _savedFoldStates.Clear();
            LoadSavedFoldStates(nextViewModel);
        }

        SubscribeViewModel(_currentViewModel);

        if (nextViewModel != null)
        {
            InitializeEditor(nextViewModel);
        }
    }

    private void InitializeEditor(ResourceYamlViewModel vm)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        AssociatedObject.Document = vm.YamlDocument;
        AssociatedObject.TextArea.IndentationStrategy = s_yamlIndentationStrategy;

        if (_textMateInstallation == null)
        {
            _textMateInstallation = AssociatedObject.InstallTextMate(_registryOptions, true);
            _textMateInstallation.SetGrammar(_registryOptions
                .GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".yaml").Id));
        }

        if (_foldingManager == null)
        {
            _foldingManager = FoldingManager.Install(AssociatedObject.TextArea);
        }

        UpdateFoldings();
        UpdateValidationDiagnostics();
    }

    private void AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (AssociatedObject?.DataContext is ResourceYamlViewModel)
        {
            UpdateFoldings();
        }
    }

    private void DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PersistFoldingState(_currentViewModel, persistToViewModel: true);
    }

    protected override void OnDetaching()
    {
        PersistFoldingState(_currentViewModel, persistToViewModel: true);

        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.AttachedToVisualTree -= AttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= DetachedFromVisualTree;
            AssociatedObject.TextArea.TextView.PointerReleased -= TextView_PointerReleased;
            AssociatedObject.TextArea.TextView.PointerHover -= TextView_PointerHover;
            AssociatedObject.TextArea.TextView.PointerHoverStopped -= TextView_PointerHoverStopped;
            AssociatedObject.TextArea.TextView.BackgroundRenderers.Remove(_diagnosticRenderer);
            AssociatedObject.TextChanged -= Editor_TextChanged;
            AssociatedObject.TextArea.TextEntering -= TextArea_TextEntering;
            AssociatedObject.TextArea.TextEntered -= TextArea_TextEntered;
            AssociatedObject.TextArea.SelectionChanged -= TextArea_SelectionChanged;
            AssociatedObject.TextArea.Caret.PositionChanged -= Caret_PositionChanged;

            if (_editorInputHandler != null)
            {
                AssociatedObject.TextArea.PopStackedInputHandler(_editorInputHandler);
                _editorInputHandler = null;
            }
        }

        Application.Current!.ActualThemeVariantChanged -= ThemeChanged;
        CloseCompletionWindow();
        CloseInsightWindow();
        CloseDiagnosticToolTip();

        _textMateInstallation?.Dispose();
        _textMateInstallation = null;

        if (_foldingManager != null)
        {
            FoldingManager.Uninstall(_foldingManager);
            _foldingManager = null;
        }

        UnsubscribeViewModel(_currentViewModel);
        _currentViewModel = null;

        base.OnDetaching();
    }

    private void ThemeChanged(object? sender, EventArgs e)
    {
        if (_textMateInstallation == null)
        {
            return;
        }

        if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.Light));
        }
        else
        {
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
        }
    }

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (!_isRefreshingFromViewModel)
        {
            PersistFoldingState(_currentViewModel);
        }

        UpdateFoldings();
        UpdateDocumentationForCaret();
        _isRefreshingFromViewModel = false;
        UpdateDocumentationForCaret();
    }

    private void PersistFoldingState(ResourceYamlViewModel? vm, bool persistToViewModel = false)
    {
        if (vm == null || _foldingManager == null)
        {
            return;
        }

        _savedFoldStates.Clear();
        List<NewFolding>? foldings = persistToViewModel ? [] : null;

        foreach (var folding in _foldingManager.AllFoldings)
        {
            var tag = (NewFolding)folding.Tag;
            tag.DefaultClosed = folding.IsFolded;

            var title = tag.Name.TrimEnd();
            if (!_savedFoldStates.TryGetValue(title, out var states))
            {
                states = new Queue<bool>();
                _savedFoldStates.Add(title, states);
            }

            states.Enqueue(folding.IsFolded);
            foldings?.Add(tag);
        }

        if (foldings != null)
        {
            vm.AllFoldings = foldings;
        }
    }

    private void SubscribeViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged += ViewModelOnPropertyChanged;
            vm.CompletionRequested += ViewModelOnCompletionRequested;
        }
    }

    private void UnsubscribeViewModel(ResourceYamlViewModel? vm)
    {
        if (vm != null)
        {
            vm.PropertyChanged -= ViewModelOnPropertyChanged;
            vm.CompletionRequested -= ViewModelOnCompletionRequested;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResourceYamlViewModel.Object)
            or nameof(ResourceYamlViewModel.EditMode)
            or nameof(ResourceYamlViewModel.HideNoisyFields))
        {
            PersistFoldingState(_currentViewModel);
            _isRefreshingFromViewModel = true;
        }

        if (e.PropertyName == nameof(ResourceYamlViewModel.ValidationDiagnostics))
        {
            UpdateValidationDiagnostics();
        }
    }

    private void TextArea_TextEntered(object? sender, TextInputEventArgs e)
    {
        if (AssociatedObject == null || _currentViewModel?.Object == null || !_currentViewModel.EditMode)
        {
            return;
        }

        if (string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        var entered = e.Text[0];
        if (char.IsLetterOrDigit(entered) || entered is '_' or '-')
        {
            ShowCompletionWindow();
        }
    }

    private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
    {
        if (TryExitListOnNewLine(e))
        {
            return;
        }

        if (TryInsertSequenceEntryOnNewLine(e))
        {
            return;
        }

        if (TryContinueListItemOnNewLine(e))
        {
            return;
        }

        if (_completionWindow != null && !string.IsNullOrEmpty(e.Text) && !char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }

        _insightWindow?.Hide();
    }

    private bool TryExitListOnNewLine(TextInputEventArgs e)
    {
        if (AssociatedObject?.Document == null
            || AssociatedObject.IsReadOnly
            || _currentViewModel?.EditMode != true
            || (e.Text != "\n" && e.Text != "\r" && e.Text != "\r\n")
            || !AssociatedObject.TextArea.Selection.IsEmpty)
        {
            return false;
        }

        var document = AssociatedObject.Document;
        var caretOffset = AssociatedObject.CaretOffset;
        var line = document.GetLineByOffset(caretOffset);
        var lineText = document.GetText(line);
        if (caretOffset != line.EndOffset)
        {
            return false;
        }

        var trimmed = lineText.Trim();
        if (trimmed != "-")
        {
            return false;
        }

        var indent = CountLeadingSpaces(lineText);
        var nextIndent = Math.Max(0, indent - IndentationSize);
        document.Replace(line.Offset, line.Length, new string(' ', nextIndent));
        AssociatedObject.TextArea.Caret.Offset = line.Offset + nextIndent;
        e.Handled = true;
        return true;
    }

    private bool TryContinueListItemOnNewLine(TextInputEventArgs e)
    {
        if (AssociatedObject?.Document == null
            || AssociatedObject.IsReadOnly
            || _currentViewModel?.EditMode != true
            || (e.Text != "\n" && e.Text != "\r" && e.Text != "\r\n")
            || !AssociatedObject.TextArea.Selection.IsEmpty)
        {
            return false;
        }

        var document = AssociatedObject.Document;
        var caretOffset = AssociatedObject.CaretOffset;
        var line = document.GetLineByOffset(caretOffset);
        if (caretOffset != line.EndOffset)
        {
            return false;
        }

        var lineText = document.GetText(line);
        var trimmed = lineText.Trim();
        if (!trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed == "-")
        {
            return false;
        }

        var insertText = GetDocumentNewLine(document) + new string(' ', CountLeadingSpaces(lineText)) + "- ";
        document.Insert(caretOffset, insertText);
        AssociatedObject.TextArea.Caret.Offset = caretOffset + insertText.Length;
        e.Handled = true;
        return true;
    }

    private bool TryInsertSequenceEntryOnNewLine(TextInputEventArgs e)
    {
        if (AssociatedObject?.Document == null
            || AssociatedObject.IsReadOnly
            || _currentViewModel?.Object == null
            || _currentViewModel.Cluster == null
            || !_currentViewModel.EditMode
            || (e.Text != "\n" && e.Text != "\r" && e.Text != "\r\n"))
        {
            return false;
        }

        if (!YamlSchemaContext.TryCreateSequenceEntryInsertion(
                AssociatedObject.Document,
                AssociatedObject.CaretOffset,
                _currentViewModel.Object.GetType(),
                _currentViewModel.Cluster.ModelCache,
                out var insertionText))
        {
            return false;
        }

        var offset = AssociatedObject.CaretOffset;
        AssociatedObject.Document.Insert(offset, insertionText);
        AssociatedObject.TextArea.Caret.Offset = Math.Min(offset + insertionText.Length, AssociatedObject.Document.TextLength);
        e.Handled = true;
        return true;
    }

    internal static bool TryUnindentEmptyLine(TextArea textArea)
    {
        ArgumentNullException.ThrowIfNull(textArea);

        var document = textArea.Document;
        if (document == null || !textArea.Selection.IsEmpty)
        {
            return false;
        }

        var caretOffset = textArea.Caret.Offset;
        var line = document.GetLineByOffset(caretOffset);
        var lineText = document.GetText(line);
        if (!string.IsNullOrWhiteSpace(lineText))
        {
            return false;
        }

        var caretColumn = caretOffset - line.Offset;
        if (caretColumn != line.Length || caretColumn == 0)
        {
            return false;
        }

        var removeCount = Math.Min(2, caretColumn);
        document.Remove(caretOffset - removeCount, removeCount);
        textArea.Caret.Offset = caretOffset - removeCount;
        return true;
    }

    internal static bool TryIndentSelectedLines(TextArea textArea)
    {
        ArgumentNullException.ThrowIfNull(textArea);

        var document = textArea.Document;
        if (document == null || textArea.Selection.IsEmpty)
        {
            return false;
        }

        var segment = textArea.Selection.SurroundingSegment;
        var startLine = document.GetLineByOffset(segment.Offset);
        var endLine = GetSelectionEndLine(document, segment);
        for (var line = startLine; line.LineNumber <= endLine.LineNumber; line = line.NextLine!)
        {
            document.Insert(line.Offset, new string(' ', IndentationSize));
            if (line.NextLine == null)
            {
                break;
            }
        }

        textArea.Selection = Selection.Create(textArea, segment.Offset + IndentationSize, segment.EndOffset + (IndentationSize * (endLine.LineNumber - startLine.LineNumber + 1)));
        return true;
    }

    internal static bool TryUnindentSelectedLines(TextArea textArea)
    {
        ArgumentNullException.ThrowIfNull(textArea);

        var document = textArea.Document;
        if (document == null)
        {
            return false;
        }

        if (TryUnindentEmptyLine(textArea))
        {
            return true;
        }

        if (textArea.Selection.IsEmpty)
        {
            return TryUnindentCurrentLine(textArea);
        }

        var segment = textArea.Selection.SurroundingSegment;
        var startLine = document.GetLineByOffset(segment.Offset);
        var endLine = GetSelectionEndLine(document, segment);
        var removed = 0;
        for (var line = startLine; line.LineNumber <= endLine.LineNumber; line = line.NextLine!)
        {
            var lineText = document.GetText(line);
            var removeCount = Math.Min(IndentationSize, CountLeadingSpaces(lineText));
            if (removeCount > 0)
            {
                document.Remove(line.Offset, removeCount);
                removed += removeCount;
            }

            if (line.NextLine == null)
            {
                break;
            }
        }

        var startOffset = Math.Max(startLine.Offset, segment.Offset - Math.Min(IndentationSize, CountLeadingSpaces(document.GetText(startLine))));
        var endOffset = Math.Max(startOffset, segment.EndOffset - removed);
        textArea.Selection = Selection.Create(textArea, startOffset, endOffset);
        return true;
    }

    internal static bool TryUnindentCurrentLine(TextArea textArea)
    {
        ArgumentNullException.ThrowIfNull(textArea);

        var document = textArea.Document;
        if (document == null || !textArea.Selection.IsEmpty)
        {
            return false;
        }

        var caretOffset = textArea.Caret.Offset;
        var line = document.GetLineByOffset(caretOffset);
        var lineText = document.GetText(line);
        var removeCount = Math.Min(IndentationSize, CountLeadingSpaces(lineText));
        if (removeCount == 0)
        {
            return false;
        }

        document.Remove(line.Offset, removeCount);
        textArea.Caret.Offset = Math.Max(line.Offset, caretOffset - removeCount);
        return true;
    }

    private bool TryUnindentOnBackspace()
    {
        if (AssociatedObject == null || AssociatedObject.IsReadOnly || _currentViewModel?.EditMode != true)
        {
            return false;
        }

        return TryUnindentEmptyLine(AssociatedObject.TextArea);
    }

    private bool TryIndentSelection()
    {
        if (AssociatedObject == null || AssociatedObject.IsReadOnly || _currentViewModel?.EditMode != true)
        {
            return false;
        }

        return TryIndentSelectedLines(AssociatedObject.TextArea);
    }

    private bool TryUnindentSelection()
    {
        if (AssociatedObject == null || AssociatedObject.IsReadOnly || _currentViewModel?.EditMode != true)
        {
            return false;
        }

        return TryUnindentSelectedLines(AssociatedObject.TextArea);
    }

    private static DocumentLine GetSelectionEndLine(TextDocument document, ISegment segment)
    {
        var endOffset = segment.EndOffset;
        if (endOffset > segment.Offset)
        {
            var endLine = document.GetLineByOffset(Math.Max(0, endOffset - 1));
            if (endLine.Offset == endOffset && endLine.PreviousLine != null)
            {
                return endLine.PreviousLine;
            }

            return endLine;
        }

        return document.GetLineByOffset(endOffset);
    }

    private static int CountLeadingSpaces(string text)
    {
        var count = 0;
        while (count < text.Length && text[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private static string GetDocumentNewLine(TextDocument document)
    {
        return document.Text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    }

    private void TextArea_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateDocumentationForCaret();
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        UpdateDocumentationForCaret();
    }

    private void TextView_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject?.Document == null || _currentViewModel?.Object == null)
        {
            return;
        }

        AssociatedObject.Focus();
        AssociatedObject.TextArea.Focus();

        var position = AssociatedObject.TextArea.TextView.GetPosition(e.GetPosition(AssociatedObject.TextArea.TextView));
        if (!position.HasValue)
        {
            CloseInsightWindow();
            return;
        }

        ShowDocumentation(position.Value.Location);
    }

    private void TextView_PointerHover(object? sender, PointerEventArgs e)
    {
        if (AssociatedObject?.Document == null)
        {
            return;
        }

        var position = AssociatedObject.TextArea.TextView.GetPosition(e.GetPosition(AssociatedObject.TextArea.TextView));
        if (!position.HasValue)
        {
            CloseDiagnosticToolTip();
            return;
        }

        var offset = AssociatedObject.Document.GetOffset(position.Value.Location);
        var message = _diagnosticRenderer.TryGetMessageAt(offset);
        if (string.IsNullOrEmpty(message))
        {
            CloseDiagnosticToolTip();
            return;
        }

        ToolTip.SetTip(AssociatedObject, message);
        ToolTip.SetIsOpen(AssociatedObject, true);
    }

    private void TextView_PointerHoverStopped(object? sender, PointerEventArgs e)
    {
        CloseDiagnosticToolTip();
    }

    private void ShowCompletionWindow()
    {
        if (AssociatedObject?.Document == null
            || AssociatedObject.IsReadOnly
            || _currentViewModel?.Object == null
            || _currentViewModel.Cluster == null
            || !_currentViewModel.EditMode)
        {
            return;
        }

        var context = YamlSchemaContext.Resolve(
            AssociatedObject.Document,
            AssociatedObject.CaretOffset,
            _currentViewModel.Object.GetType(),
            _currentViewModel.Cluster.ModelCache);

        if (context.CompletionItems.Count == 0)
        {
            CloseCompletionWindow();
            return;
        }

        if (_completionWindow == null)
        {
            _completionWindow = new CompletionWindow(AssociatedObject.TextArea);
            _completionWindow.Closed += CompletionWindow_Closed;
        }
        else
        {
            _completionWindow.CompletionList.CompletionData.Clear();
        }

        var currentLine = AssociatedObject.Document.GetLineByOffset(AssociatedObject.CaretOffset);
        var completionStartOffset = currentLine.Offset + context.KeyStartColumn;
        var completionEndOffset = currentLine.Offset + Math.Max(context.KeyStartColumn, context.KeyEndColumn);

        _completionWindow.StartOffset = completionStartOffset;
        _completionWindow.EndOffset = completionEndOffset;

        foreach (var item in context.CompletionItems)
        {
            _completionWindow.CompletionList.CompletionData.Add(new YamlCompletionData(item));
        }

        _completionWindow.CompletionList.SelectItem(context.KeyPrefix);

        if (_completionWindow.IsOpen)
        {
            return;
        }

        _completionWindow.Show();
    }

    private void ViewModelOnCompletionRequested(object? sender, EventArgs e)
    {
        ShowCompletionWindow();
    }

    private void UpdateValidationDiagnostics()
    {
        _diagnosticRenderer.Update(AssociatedObject?.Document, _currentViewModel?.ValidationDiagnostics ?? []);
    }

    private void CompletionWindow_Closed(object? sender, EventArgs e)
    {
        CloseCompletionWindow();
    }

    private void CloseCompletionWindow()
    {
        if (_completionWindow == null)
        {
            return;
        }

        _completionWindow.Closed -= CompletionWindow_Closed;
        if (_completionWindow.IsOpen)
        {
            _completionWindow.Hide();
        }

        _completionWindow = null;
    }

    private void CloseDiagnosticToolTip()
    {
        if (AssociatedObject != null)
        {
            ToolTip.SetIsOpen(AssociatedObject, false);
        }
    }

    private void UpdateDocumentationForCaret()
    {
        if (AssociatedObject?.Document == null || _currentViewModel?.Object == null)
        {
            return;
        }

        var location = AssociatedObject.TextArea.Selection.IsEmpty
            ? AssociatedObject.Document.GetLocation(AssociatedObject.CaretOffset)
            : AssociatedObject.Document.GetLocation(AssociatedObject.TextArea.Selection.SurroundingSegment.Offset);

        ShowDocumentation(location);
    }

    private void ShowDocumentation(TextLocation location)
    {
        if (AssociatedObject?.Document == null || _currentViewModel?.Object == null || _currentViewModel.Cluster == null)
        {
            return;
        }

        var context = ResolveDocumentationContext(location);
        if (context.Documentation == null || context.CurrentProperty == null || !IsWithinFieldName(location, context))
        {
            CloseInsightWindow();
            return;
        }

        var currentLine = AssociatedObject.Document.GetLineByNumber(location.Line);
        var startOffset = currentLine.Offset + context.KeyStartColumn;
        var endOffset = currentLine.Offset + context.KeyEndColumn;

        CloseInsightWindow();

        _insightWindow = new OverloadInsightWindow(AssociatedObject.TextArea)
        {
            CloseAutomatically = true,
            Provider = new YamlDocumentationOverloadProvider(context.Documentation)
        };
        _insightWindow.Closed += InsightWindow_Closed;
        _insightWindow.StartOffset = startOffset;
        _insightWindow.EndOffset = endOffset;
        _insightWindow.Show();
    }

    private YamlContextResult ResolveDocumentationContext(TextLocation location)
    {
        var offset = AssociatedObject!.Document!.GetOffset(location);
        return YamlSchemaContext.Resolve(
            AssociatedObject.Document,
            offset,
            _currentViewModel!.Object!.GetType(),
            _currentViewModel.Cluster!.ModelCache);
    }

    private bool IsWithinFieldName(TextLocation location, YamlContextResult context)
    {
        var currentLine = AssociatedObject!.Document!.GetLineByNumber(location.Line);
        var offset = AssociatedObject.Document.GetOffset(location);
        var startOffset = currentLine.Offset + context.KeyStartColumn;
        var endOffset = currentLine.Offset + context.KeyEndColumn;
        return offset >= startOffset && offset <= endOffset;
    }

    private void InsightWindow_Closed(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _insightWindow))
        {
            _insightWindow = null;
        }
    }

    private void CloseInsightWindow()
    {
        if (_insightWindow == null)
        {
            return;
        }

        _insightWindow.Closed -= InsightWindow_Closed;
        if (_insightWindow.IsOpen)
        {
            _insightWindow.Hide();
        }

        _insightWindow = null;
    }

    private void UpdateFoldings()
    {
        if (_foldingManager == null || AssociatedObject == null)
        {
            return;
        }

        if (AssociatedObject.DataContext is ResourceYamlViewModel vm)
        {
            try
            {
                if (_savedFoldStates.Count == 0)
                {
                    LoadSavedFoldStates(vm);
                }

                var newFoldings = YamlFoldingStrategy.CreateNewFoldings(AssociatedObject.Document!, out var firstErrorOffset)
                    .ToList();

                _foldingManager.UpdateFoldings(newFoldings, firstErrorOffset);
                RestoreFoldStates();
            }
            catch (Exception ex)
            {
                var logger = Application.Current?.GetRequiredService<ILogger<YamlEditorBehavior>>();
                logger?.LogWarning(ex, "Error loading foldings");
            }
        }
    }

    private void LoadSavedFoldStates(ResourceYamlViewModel vm)
    {
        if (vm.AllFoldings == null)
        {
            return;
        }

        foreach (var folding in vm.AllFoldings)
        {
            var title = folding.Name.TrimEnd();
            if (!_savedFoldStates.TryGetValue(title, out var states))
            {
                states = new Queue<bool>();
                _savedFoldStates.Add(title, states);
            }

            states.Enqueue(folding.DefaultClosed);
        }
    }

    private void RestoreFoldStates()
    {
        foreach (var folding in _foldingManager!.AllFoldings)
        {
            var title = folding.Title.TrimEnd();
            if (_savedFoldStates.TryGetValue(title, out var states) && states.Count > 0)
            {
                folding.IsFolded = states.Dequeue();
            }
        }
    }
}
