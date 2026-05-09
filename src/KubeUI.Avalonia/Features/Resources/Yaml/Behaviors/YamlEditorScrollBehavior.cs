using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Views;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

public sealed class YamlEditorScrollBehavior : Behavior<TextEditor>
{
    private const double ScrollOffsetTolerance = 0.5;

    private IFactory? _factory;
    private ScrollViewer? _scrollViewer;
    private IDisposable? _visibilitySubscription;
    private ResourceYamlViewModel? _currentViewModel;
    private bool _isCurrentViewModelActive;
    private Vector? _pendingRestoreOffset;
    private bool _isRestoringScrollOffset;

    private bool TryEnsureFactory()
    {
        if (_factory is not null)
        {
            return true;
        }

        if (!TryGetServices(out IServiceProvider services))
        {
            return false;
        }

        _factory = services.GetRequiredService<IFactory>();
        return true;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        TryEnsureFactory();
        _factory?.ActiveDockableChanged += FactoryActiveDockableChanged;
        AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;
        AssociatedObject.DocumentChanged += AssociatedObjectOnDocumentChanged;
        AssociatedObject.AttachedToVisualTree += AssociatedObjectOnAttachedToVisualTree;
        AssociatedObject.DetachedFromVisualTree += AssociatedObjectOnDetachedFromVisualTree;
        AssociatedObject.LayoutUpdated += AssociatedObjectOnLayoutUpdated;

        _visibilitySubscription = AssociatedObject.GetObservable(Visual.IsVisibleProperty)
            .Subscribe(visible =>
            {
                if (!visible)
                {
                    PersistScrollOffset(force: true);
                    _isCurrentViewModelActive = false;
                }
                else
                {
                    AttachScrollViewer();
                    RequestRestoreScrollOffset();
                }
            });

        AttachScrollViewer();
        UpdateCurrentViewModel(AssociatedObject.DataContext as ResourceYamlViewModel);
    }

    protected override void OnDetaching()
    {
        PersistScrollOffset(force: true);

        _factory?.ActiveDockableChanged -= FactoryActiveDockableChanged;
        _currentViewModel = null;
        _isCurrentViewModelActive = false;

        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
            AssociatedObject.DocumentChanged -= AssociatedObjectOnDocumentChanged;
            AssociatedObject.AttachedToVisualTree -= AssociatedObjectOnAttachedToVisualTree;
            AssociatedObject.DetachedFromVisualTree -= AssociatedObjectOnDetachedFromVisualTree;
            AssociatedObject.LayoutUpdated -= AssociatedObjectOnLayoutUpdated;
            _visibilitySubscription?.Dispose();
            _visibilitySubscription = null;

            DetachScrollViewer();
        }

        base.OnDetaching();
    }

    private void AssociatedObjectOnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as ResourceYamlViewModel);
        if (_factory is null && TryEnsureFactory())
        {
            _factory!.ActiveDockableChanged += FactoryActiveDockableChanged;
        }
    }

    private void FactoryActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        HandleFactoryDockableChanged(e.Dockable);
    }

    private void AssociatedObjectOnDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        RefreshScrollViewerAndRestore();
    }

    private void AssociatedObjectOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        RefreshScrollViewerAndRestore();
    }

    private void AssociatedObjectOnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        PersistScrollOffset(force: true);
        _isCurrentViewModelActive = false;
    }

    private void AssociatedObjectOnLayoutUpdated(object? sender, EventArgs e)
    {
        AttachScrollViewer();

        if (_pendingRestoreOffset is not null)
        {
            RestoreScrollOffset();
        }
    }

    private void UpdateCurrentViewModel(ResourceYamlViewModel? nextViewModel)
    {
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            _isCurrentViewModelActive = IsCurrentViewModelActive();
            if (_currentViewModel != null)
            {
                RequestRestoreScrollOffset();
            }

            return;
        }

        PersistScrollOffset(force: true);

        _currentViewModel = nextViewModel;
        _isCurrentViewModelActive = IsCurrentViewModelActive();
        _pendingRestoreOffset = _currentViewModel?.ScrollOffset;

        if (_currentViewModel != null)
        {
            RequestRestoreScrollOffset();
        }
    }

    private void HandleFactoryDockableChanged(IDockable? dockable)
    {
        if (_currentViewModel == null)
        {
            return;
        }

        if (ReferenceEquals(dockable, _currentViewModel))
        {
            _isCurrentViewModelActive = true;
            RefreshScrollViewerAndRestore();
            return;
        }

        if (_isCurrentViewModelActive)
        {
            PersistScrollOffset(force: true);
            _isCurrentViewModelActive = false;
        }
    }

    private bool IsCurrentViewModelActive()
    {
        if (_currentViewModel == null)
        {
            return false;
        }

        if (_factory == null)
        {
            return false;
        }

        foreach (var dock in _factory.Find(_ => true).OfType<IDock>())
        {
            if (ReferenceEquals(dock.ActiveDockable, _currentViewModel))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetServices(out IServiceProvider services)
    {
        if (Application.Current is IServiceProviderHost host)
        {
            services = host.Services;
            return true;
        }

        services = null!;
        return false;
    }

    private void PersistScrollOffset(bool force = false)
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not ResourceYamlViewModel vm)
        {
            return;
        }

        if (!force && !_isCurrentViewModelActive)
        {
            return;
        }

        var sv = _scrollViewer;
        if (sv is not null)
        {
            if (ShouldDeferScrollPersistence(sv.Offset))
            {
                return;
            }

            vm.ScrollOffset = sv.Offset;
            return;
        }

        if (AssociatedObject != null)
        {
            var editorOffset = new Vector(AssociatedObject.HorizontalOffset, AssociatedObject.VerticalOffset);
            if (ShouldDeferScrollPersistence(editorOffset))
            {
                return;
            }

            vm.ScrollOffset = editorOffset;
        }
    }

    private bool ShouldDeferScrollPersistence(Vector currentOffset)
    {
        return _pendingRestoreOffset is Vector pendingRestoreOffset
            && pendingRestoreOffset != default
            && currentOffset != pendingRestoreOffset;
    }

    private void RequestRestoreScrollOffset()
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not ResourceYamlViewModel vm)
        {
            return;
        }

        _pendingRestoreOffset = vm.ScrollOffset;
        Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
    }

    private void RefreshScrollViewerAndRestore()
    {
        AttachScrollViewer();
        RequestRestoreScrollOffset();
    }

    private void RestoreScrollOffset()
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not ResourceYamlViewModel vm || _scrollViewer == null)
        {
            return;
        }

        var targetOffset = _pendingRestoreOffset ?? vm.ScrollOffset;
        var currentOffset = _scrollViewer.Offset;

        if (AreOffsetsClose(currentOffset, targetOffset))
        {
            _pendingRestoreOffset = null;
            return;
        }

        // Wait until the scroll extent is ready before applying a non-zero restore.
        if (targetOffset != default
            && (_scrollViewer.Extent.Width <= _scrollViewer.Viewport.Width
                && _scrollViewer.Extent.Height <= _scrollViewer.Viewport.Height))
        {
            return;
        }

        _isRestoringScrollOffset = true;
        try
        {
            _scrollViewer.Offset = targetOffset;
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

    private static bool AreOffsetsClose(Vector left, Vector right)
    {
        return Math.Abs(left.X - right.X) <= ScrollOffsetTolerance
            && Math.Abs(left.Y - right.Y) <= ScrollOffsetTolerance;
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
        if (_scrollViewer == null)
        {
            return;
        }

        _scrollViewer.PropertyChanged -= ScrollViewerOnPropertyChanged;
        _scrollViewer = null;
    }

    private void ScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ScrollViewer.OffsetProperty)
        {
            return;
        }

        if (_isRestoringScrollOffset)
        {
            return;
        }

        if (_pendingRestoreOffset is Vector pendingRestoreOffset
            && pendingRestoreOffset != default
            && sender is ScrollViewer scrollViewer
            && !AreOffsetsClose(scrollViewer.Offset, pendingRestoreOffset))
        {
            return;
        }

        PersistScrollOffset();
    }
}
