using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Search;
using KubeUI.Avalonia.Infrastructure.Platform;
using System.Text;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;

/// <summary>Provides scrolling, syntax highlighting, and search behavior for the pod log editor.</summary>
public sealed class PodLogsEditorBehavior : Behavior<TextEditor>, IDeclarativeViewBase
{
    /// <summary>Gets or sets whether new output keeps the editor at the bottom. Supports two-way binding.</summary>
    public static readonly DirectProperty<PodLogsEditorBehavior, bool> AutoScrollToBottomProperty =
        AvaloniaProperty.RegisterDirect<PodLogsEditorBehavior, bool>(
            nameof(AutoScrollToBottom),
            behavior => behavior.AutoScrollToBottom,
            (behavior, value) => behavior.AutoScrollToBottom = value,
            true,
            BindingMode.TwoWay);

    /// <summary>Identifies the two-way follow-logs request property.</summary>
    public static readonly DirectProperty<PodLogsEditorBehavior, bool> FollowLogsRequestedProperty =
        AvaloniaProperty.RegisterDirect<PodLogsEditorBehavior, bool>(
            nameof(FollowLogsRequested),
            behavior => behavior.FollowLogsRequested,
            (behavior, value) => behavior.FollowLogsRequested = value,
            false,
            BindingMode.TwoWay);

    /// <summary>Identifies the two-way persisted scroll-offset property.</summary>
    public static readonly DirectProperty<PodLogsEditorBehavior, Vector> ScrollOffsetProperty =
        AvaloniaProperty.RegisterDirect<PodLogsEditorBehavior, Vector>(
            nameof(ScrollOffset),
            behavior => behavior.ScrollOffset,
            (behavior, value) => behavior.ScrollOffset = value,
            default,
            BindingMode.TwoWay);

    private Installation? _textMateInstallation;
    private RegistryOptions? _registryOptions;
    private ScrollViewer? _scrollViewer;
    private Vector? _pendingRestoreOffset;
    private bool _autoScrollToBottom = true;
    private bool _followLogsRequested;
    private Vector _scrollOffset;
    private bool _isRestoringScrollOffset;
    private bool _suppressScrollSync;
    private bool _isStuckToBottom = true;
    private bool _stickToBottomQueued;
    private bool _followLogsQueued;
    private SearchPanel? _searchPanel;
    private TextDocument? _sourceDocument;
    private readonly TextDocument _filteredDocument = new();
    private ToggleButton? _filterToggle;
    private bool _filterEnabled;
    private IDisposable? _documentSubscription;

    /// <summary>Gets or sets whether new output keeps the editor at the bottom.</summary>
    public bool AutoScrollToBottom
    {
        get => _autoScrollToBottom;
        set => SetAndRaise(AutoScrollToBottomProperty, ref _autoScrollToBottom, value);
    }

    /// <summary>Gets or sets the request state for resuming live log following.</summary>
    public bool FollowLogsRequested
    {
        get => _followLogsRequested;
        set
        {
            if (_followLogsRequested == value)
            {
                return;
            }

            SetAndRaise(FollowLogsRequestedProperty, ref _followLogsRequested, value);
            if (value)
            {
                _followLogsQueued = true;
                Dispatcher.UIThread.Post(FollowLogs, DispatcherPriority.Loaded);
            }
        }
    }

    /// <summary>Gets or sets the persisted editor scroll offset.</summary>
    public Vector ScrollOffset
    {
        get => _scrollOffset;
        set => SetAndRaise(ScrollOffsetProperty, ref _scrollOffset, value);
    }

    internal bool IsTextMateInstalled => _textMateInstallation is not null;

    protected override void OnAttached()
    {
        base.OnAttached();

        _registryOptions = new RegistryOptions(GetThemeName());
        EnsureTextMateInstallation();
        _filteredDocument.UndoStack.SizeLimit = 0;

        AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
        AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        AssociatedObject.LayoutUpdated += AssociatedObjectOnLayoutUpdated;
        _documentSubscription = AssociatedObject.GetObservable(TextEditor.DocumentProperty).Subscribe(OnEditorDocumentChanged);

        if (Application.Current is not null)
        {
            Application.Current.ActualThemeVariantChanged += CurrentOnActualThemeVariantChanged;
        }

        ApplyTheme();
        AttachScrollViewer();
        RestoreScrollOffset();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= AssociatedObjectOnDetachedFromVisualTree;
            AssociatedObject.LayoutUpdated -= AssociatedObjectOnLayoutUpdated;
        }

        if (_filterEnabled && _sourceDocument is not null)
        {
            _filterEnabled = false;
            SetDisplayedDocument(_sourceDocument);
        }

        _documentSubscription?.Dispose();
        _documentSubscription = null;
        DetachSourceDocument();
        if (_searchPanel is not null)
        {
            _searchPanel.SearchOptionsChanged -= SearchPanelOnSearchOptionsChanged;
            _searchPanel = null;
        }
        if (_filterToggle is not null)
        {
            _filterToggle.PropertyChanged -= FilterToggleOnPropertyChanged;
            if (_filterToggle.Parent is Panel parent)
            {
                parent.Children.Remove(_filterToggle);
            }
        }
        _filterToggle = null;
        _filterEnabled = false;

        if (Application.Current is not null)
        {
            Application.Current.ActualThemeVariantChanged -= CurrentOnActualThemeVariantChanged;
        }

        PersistScrollOffset();
        DetachScrollViewer();

        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
        _registryOptions = null;

        base.OnDetaching();
    }

    private void AssociatedObjectOnTextChanged(object? sender, EventArgs e)
    {
        if (!AutoScrollToBottom || !_isStuckToBottom)
        {
            return;
        }

        QueueStickToBottom();
    }

    private void AssociatedObjectOnDataContextChanged(object? sender, EventArgs e)
    {
        PersistScrollOffset();
        _isStuckToBottom = true;
        _stickToBottomQueued = false;
        _followLogsQueued = false;
        AttachScrollViewer();
        RequestRestoreScrollOffset();
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        EnsureTextMateInstallation();
        ApplyTheme();
        AttachScrollViewer();
        RequestRestoreScrollOffset();
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PersistScrollOffset();
        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
    }

    private void AssociatedObjectOnLayoutUpdated(object? sender, EventArgs e)
    {
        AttachScrollViewer();

        if (_pendingRestoreOffset is not null)
        {
            RequestRestoreScrollOffset();
        }

        if (_stickToBottomQueued)
        {
            StickToBottom();
        }

        if (_followLogsQueued)
        {
            FollowLogs();
        }

        EnsureFilterToggle();
    }

    private void CurrentOnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplyTheme();
    }

    private void OnEditorDocumentChanged(TextDocument? document)
    {
        if (ReferenceEquals(document, _filteredDocument) || ReferenceEquals(document, _sourceDocument))
        {
            return;
        }

        DetachSourceDocument();
        _sourceDocument = document;
        if (_sourceDocument is not null)
        {
            _sourceDocument.Changed += SourceDocumentOnTextChanged;
        }

        UpdateFilteredDocument();
    }

    private void DetachSourceDocument()
    {
        if (_sourceDocument is not null)
        {
            _sourceDocument.Changed -= SourceDocumentOnTextChanged;
            _sourceDocument = null;
        }
    }

    private void SourceDocumentOnTextChanged(object? sender, DocumentChangeEventArgs e)
    {
        if (_filterEnabled)
        {
            UpdateFilteredDocument(e);
        }
    }

    private void SearchPanelOnSearchOptionsChanged(object? sender, SearchOptionsChangedEventArgs e)
    {
        if (_filterEnabled)
        {
            UpdateFilteredDocument();
        }
    }

    private void EnsureSearchPanel()
    {
        if (_searchPanel is not null || AssociatedObject?.SearchPanel is null)
        {
            return;
        }

        _searchPanel = AssociatedObject.SearchPanel;
        _searchPanel.SearchOptionsChanged += SearchPanelOnSearchOptionsChanged;
    }

    private void EnsureFilterToggle()
    {
        EnsureSearchPanel();
        if (_searchPanel is { IsOpened: false } && _filterEnabled)
        {
            _filterToggle!.IsChecked = false;
        }

        if (_filterToggle is not null || _searchPanel is null || !_searchPanel.IsOpened)
        {
            return;
        }

        TextBox? searchTextBox = _searchPanel.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        if (searchTextBox?.InnerRightContent is not Panel rightContent)
        {
            return;
        }

        _filterToggle = new ToggleButton()
            .Classes("PodLogsFilterToggle")
            .Width(22)
            .Height(22)
            .Padding(4)
            .Focusable(false)
            .ToolTip_Tip(Assets.Resources.PodLogsView_Filter)
            .Content(new FluentIcons.Avalonia.FluentIcon().Icon(FluentIcons.Common.Icon.Filter));
        _filterToggle.PropertyChanged += FilterToggleOnPropertyChanged;
        rightContent.Children.Add(_filterToggle);
    }

    private void FilterToggleOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ToggleButton.IsCheckedProperty)
        {
            _filterEnabled = _filterToggle?.IsChecked == true;
            UpdateFilteredDocument();
            if (!_filterEnabled && _searchPanel is { IsOpened: true })
            {
                _searchPanel?.FindNext(0);
            }
        }
    }

    private void UpdateFilteredDocument(DocumentChangeEventArgs? change = null)
    {
        if (AssociatedObject is null || _sourceDocument is null)
        {
            return;
        }

        var searchPattern = _searchPanel?.SearchPattern;
        if (!_filterEnabled || string.IsNullOrEmpty(searchPattern))
        {
            SetDisplayedDocument(_sourceDocument);
            return;
        }

        try
        {
            var strategy = SearchStrategyFactory.Create(
                searchPattern,
                ignoreCase: !_searchPanel!.MatchCase,
                matchWholeWords: _searchPanel.WholeWords,
                mode: _searchPanel.UseRegex ? SearchMode.RegEx : SearchMode.Normal);

            if (change is not null && TryAppendFilteredDocument(strategy, change))
            {
                SetDisplayedDocument(_filteredDocument);
                return;
            }

            var matchingLines = new bool[_sourceDocument.LineCount + 1];
            foreach (ISearchResult result in strategy.FindAll(_sourceDocument, 0, _sourceDocument.TextLength))
            {
                var firstLineNumber = _sourceDocument.GetLineByOffset(result.Offset).LineNumber;
                var lastMatchOffset = result.Offset + Math.Max(result.Length - 1, 0);
                var lastLineNumber = _sourceDocument.GetLineByOffset(lastMatchOffset).LineNumber;
                for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
                {
                    matchingLines[lineNumber] = true;
                }
            }

            var filteredText = new StringBuilder(_sourceDocument.TextLength);
            foreach (DocumentLine line in _sourceDocument.Lines)
            {
                if (matchingLines[line.LineNumber])
                {
                    filteredText.Append(_sourceDocument.GetText(line.Offset, line.TotalLength));
                }
            }

            var text = filteredText.ToString();
            if (_filteredDocument.Text != text)
            {
                _filteredDocument.Text = text;
            }
            SetDisplayedDocument(_filteredDocument);
        }
        catch (SearchPatternException)
        {
            // Keep the last valid view while the user edits an incomplete regex.
        }
    }

    private bool TryAppendFilteredDocument(ISearchStrategy strategy, DocumentChangeEventArgs change)
    {
        if (_sourceDocument is null
            || change.RemovalLength != 0
            || change.InsertionLength == 0
            || change.Offset + change.InsertionLength != _sourceDocument.TextLength
            || change.Offset > 0 && _sourceDocument.GetCharAt(change.Offset - 1) != '\n'
            || _searchPanel?.UseRegex == true)
        {
            return false;
        }

        var matchingLines = new bool[_sourceDocument.LineCount + 1];
        foreach (ISearchResult result in strategy.FindAll(
            _sourceDocument,
            change.Offset,
            change.InsertionLength))
        {
            var firstLineNumber = _sourceDocument.GetLineByOffset(result.Offset).LineNumber;
            var lastMatchOffset = result.Offset + Math.Max(result.Length - 1, 0);
            var lastLineNumber = _sourceDocument.GetLineByOffset(lastMatchOffset).LineNumber;
            for (var lineNumber = firstLineNumber; lineNumber <= lastLineNumber; lineNumber++)
            {
                matchingLines[lineNumber] = true;
            }
        }

        StringBuilder appendedText = new(change.InsertionLength);
        var firstLine = _sourceDocument.GetLineByOffset(change.Offset).LineNumber;
        for (var lineNumber = firstLine; lineNumber <= _sourceDocument.LineCount; lineNumber++)
        {
            if (matchingLines[lineNumber])
            {
                DocumentLine line = _sourceDocument.GetLineByNumber(lineNumber);
                appendedText.Append(_sourceDocument.GetText(line.Offset, line.TotalLength));
            }
        }

        _filteredDocument.Insert(_filteredDocument.TextLength, appendedText.ToString());
        return true;
    }

    private void SetDisplayedDocument(TextDocument document)
    {
        if (AssociatedObject is not null && !ReferenceEquals(AssociatedObject.Document, document))
        {
            AssociatedObject.SetCurrentValue(TextEditor.DocumentProperty, document);
        }
    }

    private void ApplyTheme()
    {
        if (AssociatedObject is null || _textMateInstallation is null || _registryOptions is null)
        {
            return;
        }

        _textMateInstallation.SetTheme(_registryOptions.LoadTheme(GetThemeName()));
    }

    private void EnsureTextMateInstallation()
    {
        if (AssociatedObject is null || _textMateInstallation is not null || _registryOptions is null)
        {
            return;
        }

        _textMateInstallation = AssociatedObject.InstallTextMate(_registryOptions, false);
    }

    private void RestoreScrollOffset()
    {
        if (_suppressScrollSync || _isRestoringScrollOffset || AssociatedObject is null || _scrollViewer is null)
        {
            return;
        }

        if (_pendingRestoreOffset is null && ScrollOffset == default)
        {
            _isStuckToBottom = true;
            return;
        }

        var targetOffset = _pendingRestoreOffset ?? ScrollOffset;

        if (_scrollViewer.Extent.Width <= _scrollViewer.Viewport.Width && _scrollViewer.Extent.Height <= _scrollViewer.Viewport.Height)
        {
            _pendingRestoreOffset = null;
            return;
        }

        _isRestoringScrollOffset = true;
        try
        {
            _scrollViewer.Offset = targetOffset;
            SynchronizePinnedState(_scrollViewer);
            if (_scrollViewer.Offset == targetOffset)
            {
                _pendingRestoreOffset = null;
            }
        }
        finally
        {
            _isRestoringScrollOffset = false;
        }
    }

    private void RequestRestoreScrollOffset()
    {
        _pendingRestoreOffset = ScrollOffset;
        Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
    }

    private void PersistScrollOffset()
    {
        if (AssociatedObject is null || _scrollViewer is null)
        {
            return;
        }

        if (_isRestoringScrollOffset)
        {
            return;
        }

        ScrollOffset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y);
        SynchronizePinnedState(_scrollViewer);
    }

    private void AttachScrollViewer()
    {
        if (AssociatedObject?.GetScrollViewer() is not ScrollViewer scrollViewer)
        {
            return;
        }

        if (ReferenceEquals(_scrollViewer, scrollViewer))
        {
            return;
        }

        DetachScrollViewer();
        _scrollViewer = scrollViewer;
        _scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
    }

    private void DetachScrollViewer()
    {
        if (_scrollViewer is null)
        {
            return;
        }

        _scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
        _scrollViewer = null;
    }

    private void ScrollViewerOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer)
        {
            return;
        }

        if (_isRestoringScrollOffset || _suppressScrollSync)
        {
            return;
        }

        PersistScrollOffset();
    }

    private void QueueStickToBottom()
    {
        if (_stickToBottomQueued)
        {
            return;
        }

        _stickToBottomQueued = true;
        Dispatcher.UIThread.Post(StickToBottom, DispatcherPriority.Loaded);
    }

    private void StickToBottom()
    {
        _stickToBottomQueued = false;

        if (!AutoScrollToBottom || _scrollViewer is null)
        {
            return;
        }

        _isRestoringScrollOffset = true;
        try
        {
            _suppressScrollSync = true;
            _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.ScrollBarMaximum.Y);
            _suppressScrollSync = false;
            _isStuckToBottom = IsAtBottom(_scrollViewer);
            ScrollOffset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y);
        }
        finally
        {
            _suppressScrollSync = false;
            _isRestoringScrollOffset = false;
        }
    }

    private void FollowLogs()
    {
        _followLogsQueued = false;

        if (!AutoScrollToBottom)
        {
            AutoScrollToBottom = true;
        }

        FollowLogsRequested = false;
        StickToBottom();
    }

    private static bool IsAtBottom(ScrollViewer scrollViewer)
    {
        const double threshold = 1.0;
        return scrollViewer.ScrollBarMaximum.Y - scrollViewer.Offset.Y <= threshold;
    }

    private void SynchronizePinnedState(ScrollViewer scrollViewer)
    {
        _isStuckToBottom = IsAtBottom(scrollViewer);
        AutoScrollToBottom = _isStuckToBottom;
    }

    private static ThemeName GetThemeName()
    {
        return Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus;
    }
}
