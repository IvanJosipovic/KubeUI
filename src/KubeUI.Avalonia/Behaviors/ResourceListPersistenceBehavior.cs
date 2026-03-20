using Avalonia;
using Avalonia.Xaml.Interactivity;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Model.Controls;
using Avalonia.Controls;
using KubeUI.Avalonia.Views;
using ProDataGrid;
using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Behaviors;

public sealed class ResourceListPersistenceBehavior : Behavior<DataGrid>
{
    private readonly IFactory _factory = Application.Current.GetRequiredService<IFactory>();
    private IResourceListViewModel? _currentViewModel;
    private bool _isCurrentViewModelActive;
    private IDisposable? _visibilitySubscription;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null)
        {
            return;
        }

        _factory.ActiveDockableChanged += FactoryActiveDockableChanged;
        AssociatedObject.DataContextChanged += AssociatedObjectOnDataContextChanged;

        _visibilitySubscription = AssociatedObject.GetObservable(Visual.IsVisibleProperty)
            .Subscribe(visible =>
            {
                if (!visible)
                {
                    PersistState(force: true);
                    _isCurrentViewModelActive = false;
                }
                else
                {
                    RequestRestoreState();
                }
            });

        UpdateCurrentViewModel(AssociatedObject.DataContext as IResourceListViewModel);
    }

    protected override void OnDetaching()
    {
        PersistState(force: true);

        _factory.ActiveDockableChanged -= FactoryActiveDockableChanged;
        _currentViewModel = null;
        _isCurrentViewModelActive = false;

        if (AssociatedObject != null)
        {
            AssociatedObject.DataContextChanged -= AssociatedObjectOnDataContextChanged;
            _visibilitySubscription?.Dispose();
            _visibilitySubscription = null;
        }

        base.OnDetaching();
    }

    private void AssociatedObjectOnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateCurrentViewModel(AssociatedObject?.DataContext as IResourceListViewModel);
    }

    private void FactoryActiveDockableChanged(object? sender, ActiveDockableChangedEventArgs e)
    {
        HandleFactoryDockableChanged(e.Dockable);
    }

    private void UpdateCurrentViewModel(IResourceListViewModel? nextViewModel)
    {
        if (ReferenceEquals(_currentViewModel, nextViewModel))
        {
            _isCurrentViewModelActive = IsCurrentViewModelActive();
            if (_currentViewModel != null)
            {
                RequestRestoreState();
            }

            return;
        }

        PersistState(force: true);

        _currentViewModel = nextViewModel;
        _isCurrentViewModelActive = IsCurrentViewModelActive();

        if (_currentViewModel != null)
        {
            RequestRestoreState();
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
            RequestRestoreState();
            return;
        }

        if (_isCurrentViewModelActive)
        {
            PersistState(force: true);
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

    private DataGridStateOptions CreateStateOptions(IResourceListViewModel vm, DataGrid grid)
    {
        return new DataGridStateOptions
        {
            ItemKeySelector = (item) =>
            {
                if (item is IKubernetesObject<V1ObjectMeta> k)
                {
                    return $"{k.Metadata?.NamespaceProperty}|{k.Metadata?.Name}";
                }

                return item?.GetHashCode();
            },
            ItemKeyResolver = (key) =>
            {
                if (key == null) return null;
                foreach (var it in vm.View)
                {
                    var id = it is IKubernetesObject<V1ObjectMeta> r
                        ? $"{r.Metadata?.NamespaceProperty}|{r.Metadata?.Name}"
                        : (object?)it?.GetHashCode();

                    if (Equals(id, key)) return it;
                }

                return null;
            },
            ColumnKeySelector = column => column.ColumnKey?.ToString(),
            ColumnKeyResolver = (key) =>
            {
                if (key == null) return null;
                var s = key.ToString();
                return grid.Columns.FirstOrDefault(c => string.Equals(c.ColumnKey?.ToString(), s, StringComparison.Ordinal));
            }
        };
    }

    private void PersistState(bool force = false)
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not IResourceListViewModel vm)
        {
            return;
        }

        if (!force && !_isCurrentViewModelActive)
        {
            return;
        }

        var grid = AssociatedObject;
        if (grid == null)
        {
            return;
        }

        try
        {
            // Use ProDataGrid runtime API to capture an in-memory snapshot.
            // This keeps state dockable-only and avoids serializing to disk.
            var options = CreateStateOptions(vm, grid);
            var runtimeState = grid.CaptureState(DataGridStateSections.All, options);
            vm.DataGridRuntimeState = runtimeState;
            return;
        }
        catch
        {
            // If ProDataGrid API not available, fall back to VM-level persistence
        }
    }

    private void RequestRestoreState()
    {
        if ((_currentViewModel ?? AssociatedObject?.DataContext) is not IResourceListViewModel vm)
        {
            return;
        }

        var grid = AssociatedObject;
        if (grid == null)
        {
            return;
        }

        try
        {
            var runtimeState = vm.DataGridRuntimeState;
            if (runtimeState != null)
            {
                var options = CreateStateOptions(vm, grid);

                grid.RestoreState(runtimeState, DataGridStateSections.All, options);
                return;
            }
        }
        catch
        {
            // ignore failures and fall back
        }

        // No VM-level fallback; if runtime restore fails, ignore.
    }
}
