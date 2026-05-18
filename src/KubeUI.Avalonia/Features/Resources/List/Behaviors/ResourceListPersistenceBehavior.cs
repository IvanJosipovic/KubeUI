using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using ProDataGrid;

namespace KubeUI.Avalonia.Features.Resources.List.Behaviors;

public sealed class ResourceListPersistenceBehavior : Behavior<DataGrid>
{
    private IFactory? _factory;
    private IResourceListViewModel? _currentViewModel;
    private bool _isCurrentViewModelActive;
    private bool _hasBeenVisible;
    private IDisposable? _visibilitySubscription;
    private bool _restoreScheduled;
    private int _restoreAttempts;
    private const int MaxRestoreAttempts = 50;

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
                    _hasBeenVisible = true;
                    ScheduleRestoreState();
                }
            });

        UpdateCurrentViewModel(AssociatedObject.DataContext as IResourceListViewModel);
    }

    protected override void OnDetaching()
    {
        PersistState(force: true);

        _factory?.ActiveDockableChanged -= FactoryActiveDockableChanged;
        _currentViewModel = null;
        _isCurrentViewModelActive = false;
        _hasBeenVisible = false;
        _restoreScheduled = false;
        _restoreAttempts = 0;

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
        if (_factory is null && TryEnsureFactory())
        {
            _factory!.ActiveDockableChanged += FactoryActiveDockableChanged;
        }
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
                ScheduleRestoreState();
            }

            return;
        }

        PersistState(force: true);

        _currentViewModel = nextViewModel;
        _isCurrentViewModelActive = IsCurrentViewModelActive();

        if (_currentViewModel != null)
        {
            ScheduleRestoreState();
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
            ScheduleRestoreState();
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
        if (global::Avalonia.Application.Current is IServiceProviderHost host)
        {
            services = host.Services;
            return true;
        }

        services = null!;
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
                if (key == null)
                    return null;
                foreach (var it in vm.View)
                {
                    var id = it is IKubernetesObject<V1ObjectMeta> r
                        ? $"{r.Metadata?.NamespaceProperty}|{r.Metadata?.Name}"
                        : (object?)it?.GetHashCode();

                    if (Equals(id, key))
                        return it;
                }

                return null;
            },
            ColumnKeySelector = column => column.ColumnKey?.ToString(),
            ColumnKeyResolver = (key) =>
            {
                if (key == null)
                    return null;
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

        if (!_hasBeenVisible && vm.DataGridRuntimeState == null)
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

        if (grid.Columns.Count == 0)
        {
            ScheduleRestoreState();
            return;
        }

        try
        {
            var runtimeState = vm.DataGridRuntimeState;
            if (runtimeState != null)
            {
                var options = CreateStateOptions(vm, grid);

                var sections = DataGridStateSections.All & ~DataGridStateSections.Sorting;
                grid.RestoreState(runtimeState, sections, options);

                if (runtimeState.Sorting != null && vm.SortingModel != null)
                {
                    using (vm.SortingModel.DeferRefresh())
                    {
                        vm.SortingModel.MultiSort = runtimeState.Sorting.MultiSort;
                        vm.SortingModel.CycleMode = runtimeState.Sorting.CycleMode;
                        vm.SortingModel.OwnsViewSorts = runtimeState.Sorting.OwnsViewSorts;
                        vm.SortingModel.Apply(runtimeState.Sorting.Descriptors ?? []);
                    }
                }
                return;
            }
        }
        catch
        {
            // ignore failures and fall back
        }

        // No VM-level fallback; if runtime restore fails, ignore.
    }

    private void ScheduleRestoreState()
    {
        if (_restoreScheduled)
        {
            return;
        }

        if (_restoreAttempts >= MaxRestoreAttempts)
        {
            return;
        }

        _restoreScheduled = true;
        _restoreAttempts++;
        Dispatcher.UIThread.Post(() =>
        {
            _restoreScheduled = false;
            RequestRestoreState();
        }, DispatcherPriority.Background);
    }
}
