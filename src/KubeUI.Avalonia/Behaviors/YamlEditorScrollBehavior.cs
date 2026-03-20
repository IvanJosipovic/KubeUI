using Avalonia;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Model.Core.Events;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Views;

namespace KubeUI.Avalonia.Behaviors;

public sealed class YamlEditorScrollBehavior : Behavior<TextEditor>
{
    private readonly IFactory _factory = Application.Current.GetRequiredService<IFactory>();
    private ScrollViewer? _scrollViewer;
    private IDisposable? _visibilitySubscription;
    private ResourceYamlViewModel? _currentViewModel;
    private bool _isCurrentViewModelActive;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        _currentViewModel = AssociatedObject.DataContext as ResourceYamlViewModel;
        _isCurrentViewModelActive = IsCurrentViewModelActive();
        _factory.ActiveDockableChanged += FactoryActiveDockableChanged;

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
                    if (_isCurrentViewModelActive)
                    {
                        Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
                    }
                }
            });

        AttachScrollViewer();
    }

    protected override void OnDetaching()
    {
        PersistScrollOffset(force: true);

        _factory.ActiveDockableChanged -= FactoryActiveDockableChanged;
        _currentViewModel = null;
        _isCurrentViewModelActive = false;

        if (AssociatedObject != null)
        {
            _visibilitySubscription?.Dispose();
            _visibilitySubscription = null;

            DetachScrollViewer();
        }

        base.OnDetaching();
    }

    private void FactoryActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        HandleFactoryDockableChanged(e.Dockable);
    }

    private void FactoryDockableChanged(object? sender, object args)
    {
        var dockable = args.GetType().GetProperty("Dockable")?.GetValue(args) as IDockable;
        HandleFactoryDockableChanged(dockable);
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
            AttachScrollViewer();
            Dispatcher.UIThread.Post(RestoreScrollOffset, DispatcherPriority.Loaded);
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

        foreach (var dock in _factory.Find(_ => true).OfType<IDock>())
        {
            if (ReferenceEquals(dock.ActiveDockable, _currentViewModel))
            {
                return true;
            }
        }

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

        if (AssociatedObject != null)
        {
            vm.ScrollOffset = new Vector(AssociatedObject.HorizontalOffset, AssociatedObject.VerticalOffset);
            return;
        }

        // Fallback: use the internal ScrollViewer when the editor is no longer available.
        var sv = _scrollViewer;
        if (sv is not null)
        {
            vm.ScrollOffset = sv.Offset;
        }
    }

    private void RestoreScrollOffset()
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not ResourceYamlViewModel vm || _scrollViewer == null)
        {
            return;
        }

        _scrollViewer.Offset = vm.ScrollOffset;
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

        PersistScrollOffset();
    }
}
