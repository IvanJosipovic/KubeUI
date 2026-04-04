using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Search;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Behaviors;

public sealed class PodLogsEditorBehavior : Behavior<TextEditor>
{
    private Installation? _textMateInstallation;
    private RegistryOptions? _registryOptions;
    private ScrollViewer? _scrollViewer;
    private INotifyPropertyChanged? _currentViewModel;
    private Vector? _pendingRestoreOffset;
    private bool _isRestoringScrollOffset;
    private bool _suppressScrollSync;
    private bool _isStuckToBottom = true;
    private bool _stickToBottomQueued;
    private bool _jumpToPresentQueued;

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
        AttachViewModelListener();
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
        DetachViewModelListener();

        _textMateInstallation = null;
        _registryOptions = null;

        base.OnDetaching();
    }

    private void AssociatedObjectOnTextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject?.DataContext is not PodLogsViewModel viewModel || !viewModel.AutoScrollToBottom || !_isStuckToBottom)
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
        AttachViewModelListener();
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

        if (AssociatedObject.DataContext is not PodLogsViewModel viewModel)
        {
            return;
        }

        Vector targetOffset = _pendingRestoreOffset ?? viewModel.ScrollOffset;
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
        if (AssociatedObject?.DataContext is not PodLogsViewModel viewModel)
        {
            return;
        }

        _pendingRestoreOffset = viewModel.ScrollOffset;
        Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
    }

    private void AttachViewModelListener()
    {
        if (_currentViewModel is not null)
        {
            _currentViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            _currentViewModel = null;
        }

        if (AssociatedObject?.DataContext is not INotifyPropertyChanged viewModel)
        {
            return;
        }

        _currentViewModel = viewModel;
        _currentViewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    private void DetachViewModelListener()
    {
        if (_currentViewModel is null)
        {
            return;
        }

        _currentViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        _currentViewModel = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PodLogsViewModel viewModel)
        {
            return;
        }

        if (e.PropertyName == nameof(PodLogsViewModel.JumpToPresentRequested) && viewModel.JumpToPresentRequested)
        {
            _jumpToPresentQueued = true;
            Dispatcher.UIThread.Post(JumpToPresent, DispatcherPriority.Loaded);
        }
    }

    private void PersistScrollOffset()
    {
        if (AssociatedObject is null || _scrollViewer is null)
        {
            return;
        }

        if (AssociatedObject.DataContext is not PodLogsViewModel viewModel)
        {
            return;
        }

        if (_isRestoringScrollOffset)
        {
            return;
        }

        viewModel.ScrollOffset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Offset.Y);
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

        if (AssociatedObject?.DataContext is not PodLogsViewModel viewModel || !viewModel.AutoScrollToBottom || _scrollViewer is null)
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

        if (AssociatedObject?.DataContext is not PodLogsViewModel viewModel)
        {
            return;
        }

        if (!viewModel.AutoScrollToBottom)
        {
            viewModel.AutoScrollToBottom = true;
        }

        viewModel.JumpToPresentRequested = false;
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
