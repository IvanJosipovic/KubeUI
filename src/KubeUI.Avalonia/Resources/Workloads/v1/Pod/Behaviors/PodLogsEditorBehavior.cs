using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Search;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;

public sealed class PodLogsEditorBehavior : Behavior<TextEditor>
{
    public static readonly DirectProperty<PodLogsEditorBehavior, bool> AutoScrollToBottomProperty =
        AvaloniaProperty.RegisterDirect<PodLogsEditorBehavior, bool>(
            nameof(AutoScrollToBottom),
            behavior => behavior.AutoScrollToBottom,
            (behavior, value) => behavior.AutoScrollToBottom = value,
            true,
            BindingMode.TwoWay);

    public static readonly DirectProperty<PodLogsEditorBehavior, bool> JumpToPresentRequestedProperty =
        AvaloniaProperty.RegisterDirect<PodLogsEditorBehavior, bool>(
            nameof(JumpToPresentRequested),
            behavior => behavior.JumpToPresentRequested,
            (behavior, value) => behavior.JumpToPresentRequested = value,
            false,
            BindingMode.TwoWay);

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
    private bool _jumpToPresentRequested;
    private Vector _scrollOffset;
    private bool _isRestoringScrollOffset;
    private bool _suppressScrollSync;
    private bool _isStuckToBottom = true;
    private bool _stickToBottomQueued;
    private bool _jumpToPresentQueued;

    public bool AutoScrollToBottom
    {
        get => _autoScrollToBottom;
        set => SetAndRaise(AutoScrollToBottomProperty, ref _autoScrollToBottom, value);
    }

    public bool JumpToPresentRequested
    {
        get => _jumpToPresentRequested;
        set
        {
            if (_jumpToPresentRequested == value)
            {
                return;
            }

            SetAndRaise(JumpToPresentRequestedProperty, ref _jumpToPresentRequested, value);
            if (value)
            {
                _jumpToPresentQueued = true;
                Dispatcher.UIThread.Post(JumpToPresent, DispatcherPriority.Loaded);
            }
        }
    }

    public Vector ScrollOffset
    {
        get => _scrollOffset;
        set => SetAndRaise(ScrollOffsetProperty, ref _scrollOffset, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null)
        {
            return;
        }

        _registryOptions = new RegistryOptions(GetThemeName());
        _textMateInstallation = AssociatedObject.InstallTextMate(_registryOptions, false);
        SearchPanel.Install(AssociatedObject);

        AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
        AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        AssociatedObject.LayoutUpdated += AssociatedObjectOnLayoutUpdated;

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

        if (Application.Current is not null)
        {
            Application.Current.ActualThemeVariantChanged -= CurrentOnActualThemeVariantChanged;
        }

        PersistScrollOffset();
        DetachScrollViewer();

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
        _jumpToPresentQueued = false;
        AttachScrollViewer();
        RequestRestoreScrollOffset();
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        AttachScrollViewer();
        RequestRestoreScrollOffset();
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PersistScrollOffset();
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

        if (_jumpToPresentQueued)
        {
            JumpToPresent();
        }
    }

    private void CurrentOnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (AssociatedObject is null || _textMateInstallation is null || _registryOptions is null)
        {
            return;
        }

        _textMateInstallation.SetTheme(_registryOptions.LoadTheme(GetThemeName()));
    }

    private void RestoreScrollOffset()
    {
        if (_suppressScrollSync || _isRestoringScrollOffset || AssociatedObject is null || _scrollViewer is null)
        {
            return;
        }

        Vector targetOffset = _pendingRestoreOffset ?? ScrollOffset;
        if (targetOffset == default)
        {
            _isStuckToBottom = true;
            return;
        }

        if (_scrollViewer.Extent.Width <= _scrollViewer.Viewport.Width && _scrollViewer.Extent.Height <= _scrollViewer.Viewport.Height)
        {
            return;
        }

        _isRestoringScrollOffset = true;
        try
        {
            _scrollViewer.Offset = targetOffset;
            _isStuckToBottom = IsAtBottom(_scrollViewer);
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
        _isStuckToBottom = IsAtBottom(_scrollViewer);
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
        _scrollViewer.PropertyChanged += ScrollViewerOnPropertyChanged;
    }

    private void DetachScrollViewer()
    {
        if (_scrollViewer is null)
        {
            return;
        }

        _scrollViewer.PropertyChanged -= ScrollViewerOnPropertyChanged;
        _scrollViewer = null;
    }

    private void ScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ScrollViewer.OffsetProperty || sender is not ScrollViewer scrollViewer)
        {
            return;
        }

        if (_isRestoringScrollOffset || _suppressScrollSync)
        {
            return;
        }

        _isStuckToBottom = IsAtBottom(scrollViewer);
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
            PersistScrollOffset();
        }
        finally
        {
            _suppressScrollSync = false;
            _isRestoringScrollOffset = false;
        }
    }

    private void JumpToPresent()
    {
        _jumpToPresentQueued = false;

        if (!AutoScrollToBottom)
        {
            AutoScrollToBottom = true;
        }

        JumpToPresentRequested = false;
        StickToBottom();
    }

    private static bool IsAtBottom(ScrollViewer scrollViewer)
    {
        const double threshold = 1.0;
        return scrollViewer.ScrollBarMaximum.Y - scrollViewer.Offset.Y <= threshold;
    }

    private static ThemeName GetThemeName()
    {
        return Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? ThemeName.Light
            : ThemeName.DarkPlus;
    }
}
