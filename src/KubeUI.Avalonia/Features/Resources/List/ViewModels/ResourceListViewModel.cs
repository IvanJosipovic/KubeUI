using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using AvaloniaEdit.Utils;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using SortDirection = KubeUI.Avalonia.Resources.SortDirection;

namespace KubeUI.Avalonia.Features.Resources.List.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(250);
    internal const string NamespaceScopeFilterId = "__namespace_scope__";
    internal const string NamespaceScopePropertyPath = "namespace_scope";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    private static readonly IComparer s_noopSortComparer = Comparer<object>.Create(static (_, _) => 0);

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    public GroupApiVersionKind Kind => GroupApiVersionKind.From<T>();

    [ObservableProperty]
    public partial ISourceCache<T, string> Objects { get; set; }

    public T? SelectedItem => ((SelectionModel<T>)SelectionModel).SelectedItem;

    public IReadOnlyList<T?> SelectedItems => ((SelectionModel<T>)SelectionModel).SelectedItems;

    public IEnumerable<MenuItemViewModel> ContextMenuItems => BuildContextMenuItems();

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    IResourceConfig IResourceListViewModel.ResourceConfig => ResourceConfig;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial Exception? LoadError { get; set; }

    private IDisposable? _subscription;
    private IDisposable? _countSubscription;
    private readonly Subject<string> _searchQueryChanges = new();
    private readonly IDisposable _searchQuerySubscription;

    private readonly SelectionModel<T> _selectionModel = new()
    {
        SingleSelect = false
    };

    private readonly List<T> _selectionSnapshot = [];

    public IList View => _view ?? throw new InvalidOperationException("Resource list view has not been initialized.");

    private ReadOnlyObservableCollection<T>? _view;

    private BehaviorSubject<IComparer<T>>? _sortSubject;

    public IDataGridSortingAdapterFactory SortingAdapterFactory => _sortingAdapterFactory;

    private DynamicDataSortingAdapterFactory<T> _sortingAdapterFactory = null!;

    [ObservableProperty]
    public partial ISortingModel SortingModel { get; set; } = new SortingModel
    {
        MultiSort = true,
        CycleMode = SortCycleMode.AscendingDescendingNone,
        OwnsViewSorts = true
    };

    private BehaviorSubject<Func<T, bool>>? _filterSubject;

    public IDataGridFilteringAdapterFactory FilteringAdapterFactory => _filteringAdapterFactory;

    private DynamicDataFilteringAdapterFactory<T> _filteringAdapterFactory = null!;

    [ObservableProperty]
    public partial IFilteringModel FilteringModel { get; set; } = new FilteringModel
    {
        OwnsViewFilter = true
    };

    private BehaviorSubject<Func<T, bool>>? _searchSubject;

    public IDataGridSearchAdapterFactory SearchAdapterFactory => _searchAdapterFactory;

    private DynamicDataSearchAdapterFactory<T> _searchAdapterFactory = null!;

    [ObservableProperty]
    public partial ISearchModel SearchModel { get; set; } = new SearchModel()
    {
        HighlightMode = SearchHighlightMode.None,
        HighlightCurrent = false,
        WrapNavigation = true,
        UpdateSelectionOnNavigate = false
    };

    [ObservableProperty]
    public partial int ItemCount { get; set; }

    [ObservableProperty]
    public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    public ISelectionModel SelectionModel => _selectionModel;

    public Func<IList, object, int> ReferenceIndexResolver => ResolveReferenceIndex;

    [ObservableProperty]
    public partial ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; private set; } = [];

    private readonly Dictionary<string, IResourceListColumn> _resourceColumnsByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DataGridColumnDefinition> _columnDefinitionsByKey = new(StringComparer.OrdinalIgnoreCase);
    private IList<IResourceListColumn> _resourceColumns = [];
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public ResourceListViewModel(IServiceProvider serviceProvider, ILogger<ResourceListViewModel<T>> logger, ISettingsService settingsService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        SettingsService = settingsService;

        _searchQuerySubscription = _searchQueryChanges
            .Throttle(SearchDebounceDelay)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(System.Reactive.Observer.Create<string>(_ => ApplySearch()));

        SortingModel.SortingChanged += SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged += FilteringModelOnFilteringChanged;
        SearchModel.SearchChanged += SearchModelOnSearchChanged;
        _selectionModel.SelectionChanged += SelectionModelOnSelectionChanged;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        Cluster = cluster;
        Title = Kind.Kind.Humanize(LetterCasing.Title).Pluralize();
        Id = Cluster.Name + "-" + Kind;
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);
        _resourceColumns = ResourceConfig.Columns();
        _resourceColumnsByKey.Clear();
        foreach (var column in _resourceColumns)
        {
            if (!string.IsNullOrWhiteSpace(column.Key))
            {
                _resourceColumnsByKey[column.Key] = column;
            }
        }

        OnPropertyChanged(nameof(ContextMenuItems));

        _sortingAdapterFactory = new DynamicDataSortingAdapterFactory<T>(_resourceColumnsByKey);
        _sortSubject = new(_sortingAdapterFactory.SortComparer);

        _filteringAdapterFactory = new DynamicDataFilteringAdapterFactory<T>(_resourceColumnsByKey);
        _filterSubject = new(_filteringAdapterFactory.FilterPredicate);

        _searchAdapterFactory = new DynamicDataSearchAdapterFactory<T>(_resourceColumnsByKey);
        _searchSubject = new(_searchAdapterFactory.SearchPredicate);

        GenerateColumnDefinitions();
        SetNamespaceFilter();

        var seedTask = Cluster.SeedResource<T>();

        if (seedTask.IsCompletedSuccessfully)
        {
            LoadError = null;
            IsLoading = false;
            BindObjects();
            return;
        }

        _ = LoadAsync(seedTask);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchQuery))
        {
            _searchQueryChanges.OnNext(SearchQuery);
        }
    }

    private IEnumerable<MenuItemViewModel> BuildContextMenuItems()
    {
        if (ResourceConfig == null)
        {
            return [];
        }

        var items = new List<MenuItemViewModel>();
        items.AddRange(ResourceConfig.GetDefaultMenuItems(SelectedItems));

        var custom = ResourceConfig.GetCustomMenuItems(SelectedItems).ToList();
        if (custom.Count > 0)
        {
            items.Add(new MenuItemViewModel
            {
                IsSeparator = true
            });
            items.AddRange(custom);
        }

        return items;
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetNamespaceFilter();
    }

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster == null || ResourceConfig == null || !ResourceConfig.IsNamespaced)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            return;
        }

        if (!value)
        {
            CopyNamespaces(Cluster.SelectedNamespaces, _localSelectedNamespaces);
        }

        SubscribeToSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        SetNamespaceFilter();
    }

    private void SetNamespaceFilter()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(SetNamespaceFilter).GetAwaiter().GetResult();
            return;
        }

        if (!ResourceConfig.IsNamespaced)
        {
            return;
        }

        var selectedNamespaces = SelectedNamespaces;

        if (selectedNamespaces.Count > 0)
        {
            var values = new List<object>(selectedNamespaces.Count);
            foreach (var selectedNamespace in selectedNamespaces)
            {
                values.Add(selectedNamespace.Name()!);
            }

            var descriptor = new FilteringDescriptor(
                NamespaceScopeFilterId,
                FilteringOperator.In,
                propertyPath: NamespaceScopePropertyPath,
                value: null,
                values: values);

            FilteringModel.SetOrUpdate(descriptor);
        }
        else
        {
            FilteringModel.Remove(NamespaceScopeFilterId);
        }
    }

    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            SearchModel.Clear();
            return;
        }

        SearchModel.SetOrUpdate(new(
            SearchQuery.Trim(),
            matchMode: SearchMatchMode.Contains,
            termMode: SearchTermCombineMode.All,
            scope: SearchScope.AllColumns,
            comparison: StringComparison.OrdinalIgnoreCase,
            wholeWord: false,
            normalizeWhitespace: true,
            ignoreDiacritics: true));
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _countSubscription?.Dispose();
        _searchQuerySubscription.Dispose();
        _searchQueryChanges.Dispose();
        UnsubscribeFromSelectedNamespaces();

        SortingModel.SortingChanged -= SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged -= FilteringModelOnFilteringChanged;
        SearchModel.SearchChanged -= SearchModelOnSearchChanged;
        _selectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
    }

    private async Task LoadAsync(Task seedTask)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsLoading = true;
                LoadError = null;
            });

            await seedTask.ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(BindObjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resource list for {Type}", typeof(T));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                LoadError = ex;
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    private void BindObjects()
    {
        Objects = Cluster.GetResourceSourceCache<T>();
        SubscribeToSelectedNamespaces();

        _subscription?.Dispose();

        SetNamespaceFilter();

        BehaviorSubject<IComparer<T>> sortSubject = _sortSubject ?? throw new InvalidOperationException("Sort subject has not been initialized.");
        BehaviorSubject<Func<T, bool>> filterSubject = _filterSubject ?? throw new InvalidOperationException("Filter subject has not been initialized.");
        BehaviorSubject<Func<T, bool>> searchSubject = _searchSubject ?? throw new InvalidOperationException("Search subject has not been initialized.");

        _subscription = Objects.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Do(_ => CaptureSelectionSnapshot())
            .Filter(filterSubject)
            .Filter(searchSubject)
            .SortAndBind(out var view, sortSubject, new()
            {
                ResetOnFirstTimeLoad = true,
                UseReplaceForUpdates = true,
                InitialCapacity = Objects.Count
            })
            .Subscribe(
                _ => SynchronizeSelectionWithView(),
                ex => _logger.LogError(ex, "Error Setting Resource List Filter: {ns} ", typeof(T))
            );

        _countSubscription?.Dispose();
        // Update count from the view (already filtered/searched/sorted). This
        // avoids re-running the entire pipeline. Throttle updates to at most
        // once per 100ms to reduce UI churn.
        var countObs = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => ((INotifyCollectionChanged)view).CollectionChanged += h,
                h => ((INotifyCollectionChanged)view).CollectionChanged -= h)
                .Select(_ => view.Count)
                .StartWith(view.Count)
                .Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance)
                .DistinctUntilChanged();

        _countSubscription = ((IObservable<int>)countObs).Subscribe(System.Reactive.Observer.Create<int>(c => ItemCount = c));

        _view = view;
    }

    private void SubscribeToSelectedNamespaces()
    {
        UnsubscribeFromSelectedNamespaces();
        SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
    }

    private void UnsubscribeFromSelectedNamespaces()
    {
        if (Cluster != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        }

        _localSelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();

        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void GenerateColumnDefinitions()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(GenerateColumnDefinitions).GetAwaiter().GetResult();
            return;
        }

        ColumnDefinitions.Clear();
        _columnDefinitionsByKey.Clear();

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in _resourceColumns)
        {
            try
            {
                var column = CreateColumnDefinition(columnDefinition, converter);

                ColumnDefinitions.Add(column);
                if (!string.IsNullOrWhiteSpace(columnDefinition.Key))
                {
                    _columnDefinitionsByKey[columnDefinition.Key] = column;
                }

                if (columnDefinition.Sort != SortDirection.None)
                {
                    SortingModel.SetOrUpdate(new(column, columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending, null, column.CustomSortComparer));
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }
    }

    private DataGridColumnDefinition CreateColumnDefinition(IResourceListColumn columnDefinition, DataGridLengthConverter converter)
    {
        return CreateTemplateColumnDefinition(columnDefinition, converter);
    }

    private DataGridColumnDefinition CreateTemplateColumnDefinition(IResourceListColumn columnDefinition, DataGridLengthConverter converter)
    {
        var column = new DataGridControlTemplateColumnDefinition
        {
            Header = columnDefinition.Name,
            ColumnKey = columnDefinition.Key,
            Tag = columnDefinition,
            CellTemplate = CreateCellTemplate(columnDefinition),
            CanUserSort = true,
            ShowFilterButton = true,
            CustomSortComparer = s_noopSortComparer,
            Width = ParseWidth(columnDefinition.Width, converter),
            ValueAccessor = columnDefinition.ValueAccessor,
            ValueType = columnDefinition.ValueType,
            Options = BuildColumnOptions(columnDefinition)
        };

        return column;
    }

    private FuncDataTemplate<T> CreateCellTemplate(IResourceListColumn columnDefinition)
    {
        return new FuncDataTemplate<T>((item, _) =>
        {
            try
            {
                var control = _serviceProvider.GetRequiredService(columnDefinition.CustomControl) as Control;

                if (control == null)
                {
                    throw new InvalidOperationException($"Unable to resolve control type {columnDefinition.CustomControl.FullName}");
                }

                if (control is IDisplayFunc displayFunc)
                {
                    displayFunc.SetDisplayFunc(columnDefinition.DisplayValue);
                }

                if (control is IInitializeCluster initializeCluster)
                {
                    initializeCluster.Initialize(Cluster);
                }

                control.DataContext = item;

                return control;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Control");
                return new TextBlock { Text = ex.Message };
            }
        }, supportsRecycling: true);
    }

    private static DataGridColumnDefinitionOptions BuildColumnOptions(IResourceListColumn columnDefinition)
    {
        return new()
        {
            IsSearchable = true,
            FilterValueAccessor = columnDefinition.ValueAccessor,
            SortValueAccessor = columnDefinition.ValueAccessor
        };
    }

    private static DataGridLength? ParseWidth(string? width, DataGridLengthConverter converter)
    {
        if (width == null)
        {
            return null;
        }

        return converter.ConvertFromString(width) as DataGridLength?;
    }

    private void SortingModelOnSortingChanged(object? sender, SortingChangedEventArgs e)
    {
        _sortingAdapterFactory.UpdateComparer(e.NewDescriptors);
        BehaviorSubject<IComparer<T>> sortSubject = _sortSubject ?? throw new InvalidOperationException("Sort subject has not been initialized.");
        sortSubject.OnNext(_sortingAdapterFactory.SortComparer);
    }

    private void FilteringModelOnFilteringChanged(object? sender, FilteringChangedEventArgs e)
    {
        _filteringAdapterFactory.UpdateFilter(e.NewDescriptors);
        BehaviorSubject<Func<T, bool>> filterSubject = _filterSubject ?? throw new InvalidOperationException("Filter subject has not been initialized.");
        filterSubject.OnNext(_filteringAdapterFactory.FilterPredicate);
    }

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<T> e)
    {
        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(SelectedItems));
        OnPropertyChanged(nameof(ContextMenuItems));
    }

    // Runtime DataGrid state captured from ProDataGrid (in-memory snapshot)
    public DataGridState? DataGridRuntimeState { get; set; }


    private void SearchModelOnSearchChanged(object? sender, SearchChangedEventArgs e)
    {
        _searchAdapterFactory.UpdatePredicate(e.NewDescriptors);
        BehaviorSubject<Func<T, bool>> searchSubject = _searchSubject ?? throw new InvalidOperationException("Search subject has not been initialized.");
        searchSubject.OnNext(_searchAdapterFactory.SearchPredicate);
    }

    private int ResolveReferenceIndex(IList list, object item)
    {
        if (list == null || item is not T resource)
        {
            return -1;
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], resource))
            {
                return i;
            }
        }

        return -1;
    }

    private void CaptureSelectionSnapshot()
    {
        _selectionSnapshot.Clear();

        foreach (var item in _selectionModel.SelectedItems)
        {
            if (item is T resource)
            {
                _selectionSnapshot.Add(resource);
            }
        }
    }

    private void SynchronizeSelectionWithView()
    {
        ReadOnlyObservableCollection<T>? view = _view;
        if (view is null)
        {
            return;
        }

        if (_selectionSnapshot.Count == 0 || view.Count == 0)
        {
            return;
        }

        var desiredIndexes = new List<int>(_selectionSnapshot.Count);

        for (var i = 0; i < view.Count; i++)
        {
            if (view[i] is not T resource)
            {
                continue;
            }

            for (var j = 0; j < _selectionSnapshot.Count; j++)
            {
                if (ReferenceEquals(resource, _selectionSnapshot[j]))
                {
                    desiredIndexes.Add(i);
                    break;
                }
            }
        }

        _selectionSnapshot.Clear();

        if (desiredIndexes.Count == 0 || SelectionMatches(desiredIndexes))
        {
            return;
        }

        using (SelectionModelExtensions.BatchUpdate(_selectionModel))
        {
            _selectionModel.Clear();

            if (_selectionModel.SingleSelect)
            {
                _selectionModel.Select(desiredIndexes[0]);
                return;
            }

            foreach (var index in desiredIndexes)
            {
                _selectionModel.Select(index);
            }
        }
    }

    private bool SelectionMatches(IReadOnlyList<int> desiredIndexes)
    {
        if (_selectionModel.SelectedIndexes.Count != desiredIndexes.Count)
        {
            return false;
        }

        for (var i = 0; i < desiredIndexes.Count; i++)
        {
            if (_selectionModel.SelectedIndexes[i] != desiredIndexes[i])
            {
                return false;
            }
        }

        return true;
    }

}

public sealed class DynamicDataSortingAdapterFactory<T> : IDataGridSortingAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly IReadOnlyDictionary<string, IResourceListColumn> _resourceColumnsByKey;

    private static readonly IComparer<T> s_noopComparer = Comparer<T>.Create(static (_, _) => 0);
    private static readonly IComparable s_nullSortValue = new NullSortValue();

    public IComparer<T> SortComparer { get; private set; }

    public DynamicDataSortingAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        SortComparer = s_noopComparer;
        _resourceColumnsByKey = resourceColumnsByKey;
    }

    public DataGridSortingAdapter Create(DataGrid grid, ISortingModel model)
    {
        return new DynamicDataSortingAdapter(model, () => grid.Columns, UpdateComparer);
    }

    public void UpdateComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        SortComparer = BuildComparer(descriptors);
    }

    private IComparer<T> BuildComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return s_noopComparer;
        }

        SortExpressionComparer<T>? comparer = null;

        for (var i = 0; i < descriptors.Count; i++)
        {
            var descriptor = descriptors[i];
            if (descriptor == null)
            {
                continue;
            }

            var selector = CreateSelector(descriptor);

            if (selector == null)
            {
                continue;
            }

            IComparable SafeSelect(T item)
            {
                try
                {
                    return selector(item) ?? s_nullSortValue;
                }
                catch
                {
                    return s_nullSortValue;
                }
            }

            comparer = comparer == null
                ? descriptor.Direction == ListSortDirection.Ascending
                    ? SortExpressionComparer<T>.Ascending(SafeSelect)
                    : SortExpressionComparer<T>.Descending(SafeSelect)
                : descriptor.Direction == ListSortDirection.Ascending
                    ? comparer.ThenByAscending(SafeSelect)
                    : comparer.ThenByDescending(SafeSelect);
        }

        return comparer ?? s_noopComparer;
    }

    private Func<T, IComparable?>? CreateSelector(SortingDescriptor descriptor)
    {
        if (ResolveResourceColumn(descriptor.ColumnId) is not IResourceListColumn column)
        {
            return null;
        }

        return column.SortKey;
    }

    private IResourceListColumn? ResolveResourceColumn(object? columnId)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                return taggedColumn;
            }

            if (TryResolveKey(definition.ColumnKey, out var key))
            {
                IResourceListColumn? resolvedColumn = null;
                if (_resourceColumnsByKey.TryGetValue(key, out resolvedColumn))
                {
                    return resolvedColumn;
                }
            }
        }

        if (TryResolveKey(columnId, out var directKey))
        {
            IResourceListColumn? directColumn = null;
            if (_resourceColumnsByKey.TryGetValue(directKey, out directColumn))
            {
                return directColumn;
            }
        }

        return null;
    }

    private static bool TryResolveKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private sealed class DynamicDataSortingAdapter : DataGridSortingAdapter
    {
        private readonly Action<IReadOnlyList<SortingDescriptor>> _update;

        public DynamicDataSortingAdapter(
            ISortingModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<SortingDescriptor>> update)
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<SortingDescriptor> descriptors,
            IReadOnlyList<SortingDescriptor> previousDescriptors,
            out bool changed)
        {
            _update(descriptors);
            changed = true;
            return true;
        }
    }

    private sealed class NullSortValue : IComparable
    {
        public int CompareTo(object? obj)
        {
            return obj is NullSortValue ? 0 : -1;
        }
    }
}

public sealed class DynamicDataFilteringAdapterFactory<T> : IDataGridFilteringAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private static readonly Func<T, bool> s_alwaysTrue = static _ => true;

    private readonly IReadOnlyDictionary<string, IResourceListColumn> _resourceColumnsByKey;

    public DynamicDataFilteringAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        FilterPredicate = s_alwaysTrue;
        _resourceColumnsByKey = resourceColumnsByKey;
    }

    public DataGridFilteringAdapter Create(DataGrid grid, IFilteringModel model)
    {
        return new DynamicDataFilteringAdapter(model, () => grid.Columns, UpdateFilter);
    }

    public Func<T, bool> FilterPredicate { get; private set; }

    public void UpdateFilter(IReadOnlyList<FilteringDescriptor> descriptors)
    {
        FilterPredicate = BuildPredicate(descriptors);
    }

    private Func<T, bool> BuildPredicate(IReadOnlyList<FilteringDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return s_alwaysTrue;
        }

        var compiled = new List<Func<T, bool>>(descriptors.Count);
        for (var i = 0; i < descriptors.Count; i++)
        {
            var descriptor = descriptors[i];
            var predicate = Compile(descriptor);
            if (predicate != null)
            {
                compiled.Add(predicate);
            }
        }

        if (compiled.Count == 0)
        {
            return s_alwaysTrue;
        }

        if (compiled.Count == 1)
        {
            return compiled[0];
        }

        return item =>
        {
            for (int i = 0; i < compiled.Count; i++)
            {
                if (!compiled[i](item))
                {
                    return false;
                }
            }

            return true;
        };
    }

    private Func<T, bool>? Compile(FilteringDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        if (descriptor.Predicate != null)
        {
            var predicate = descriptor.Predicate;
            return item =>
            {
                try
                {
                    return predicate(item);
                }
                catch
                {
                    return false;
                }
            };
        }

        var selector = CreateSelector(descriptor);
        if (selector == null)
        {
            return null;
        }

        object? SafeSelect(T item)
        {
            try
            {
                return selector(item);
            }
            catch
            {
                return null;
            }
        }

        var culture = descriptor.Culture ?? System.Globalization.CultureInfo.InvariantCulture;
        var stringComparison = descriptor.StringComparisonMode ?? StringComparison.OrdinalIgnoreCase;
        var values = descriptor.Values;
        var value = descriptor.Value;

        return descriptor.Operator switch
        {
            FilteringOperator.Equals => item => Equals(SafeSelect(item), value),
            FilteringOperator.NotEquals => item => !Equals(SafeSelect(item), value),
            FilteringOperator.Contains => item => Contains(SafeSelect(item), value, stringComparison),
            FilteringOperator.StartsWith => item => StartsWith(SafeSelect(item), value, stringComparison),
            FilteringOperator.EndsWith => item => EndsWith(SafeSelect(item), value, stringComparison),
            FilteringOperator.GreaterThan => item => Compare(SafeSelect(item), value, culture) > 0,
            FilteringOperator.GreaterThanOrEqual => item => Compare(SafeSelect(item), value, culture) >= 0,
            FilteringOperator.LessThan => item => Compare(SafeSelect(item), value, culture) < 0,
            FilteringOperator.LessThanOrEqual => item => Compare(SafeSelect(item), value, culture) <= 0,
            FilteringOperator.Between => item => Between(SafeSelect(item), values, culture),
            FilteringOperator.In => item => In(SafeSelect(item), values),
            _ => s_alwaysTrue
        };
    }

    private Func<T, object?>? CreateSelector(FilteringDescriptor descriptor)
    {
        if (string.Equals(descriptor.PropertyPath, ResourceListViewModel<T>.NamespaceScopePropertyPath, StringComparison.Ordinal))
        {
            return item =>
            {
                try
                {
                    return item.Namespace();
                }
                catch
                {
                    return null;
                }
            };
        }

        if (ResolveResourceColumn(descriptor.ColumnId) is not IResourceListColumn column)
        {
            return null;
        }

        return item =>
        {
            try
            {
                return column.ValueAccessor.GetValue(item!);
            }
            catch
            {
                return null;
            }
        };
    }

    private IResourceListColumn? ResolveResourceColumn(object? columnId)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                return taggedColumn;
            }

            if (TryResolveKey(definition.ColumnKey, out var key))
            {
                IResourceListColumn? resolvedColumn = null;
                if (_resourceColumnsByKey.TryGetValue(key, out resolvedColumn))
                {
                    return resolvedColumn;
                }
            }
        }

        if (TryResolveKey(columnId, out var directKey))
        {
            IResourceListColumn? directColumn = null;
            if (_resourceColumnsByKey.TryGetValue(directKey, out directColumn))
            {
                return directColumn;
            }
        }

        return null;
    }

    private static bool TryResolveKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private static bool Contains(object? source, object? target, StringComparison comparison)
    {
        if (source == null || target == null)
        {
            return false;
        }

        if (source is string s && target is string t)
        {
            return s.Contains(t, comparison);
        }

        if (source is IEnumerable<object> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (Equals(item, target))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool StartsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.StartsWith(t, comparison);
    }

    private static bool EndsWith(object? source, object? target, StringComparison comparison)
    {
        return source is string s && target is string t && s.EndsWith(t, comparison);
    }

    private static int Compare(object? left, object? right, System.Globalization.CultureInfo culture)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return -1;
        }

        if (right == null)
        {
            return 1;
        }

        if (TryGetDateTimeOffset(left, out var leftDate) && TryGetDateTimeOffset(right, out var rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        if (left is IComparable comparable)
        {
            return comparable.CompareTo(ChangeType(right, left.GetType(), culture));
        }

        return Comparer<object>.Default.Compare(left, right);
    }

    private static bool TryGetDateTimeOffset(object? value, out DateTimeOffset dateTimeOffset)
    {
        switch (value)
        {
            case DateTimeOffset dto:
                dateTimeOffset = dto;
                return true;
            case DateTime dateTime:
                dateTimeOffset = dateTime.Kind == DateTimeKind.Unspecified
                    ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))
                    : new DateTimeOffset(dateTime.ToUniversalTime());
                return true;
            default:
                dateTimeOffset = default;
                return false;
        }
    }

    private static bool Between(object? source, IReadOnlyList<object?>? values, System.Globalization.CultureInfo culture)
    {
        if (values == null || values.Count < 2)
        {
            return false;
        }

        return Compare(source, values[0], culture) >= 0 && Compare(source, values[1], culture) <= 0;
    }

    private static bool In(object? source, IReadOnlyList<object?>? values)
    {
        if (values == null || values.Count == 0)
        {
            return false;
        }

        foreach (var candidate in values)
        {
            if (Equals(source, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static object? ChangeType(object? value, Type targetType, System.Globalization.CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        try
        {
            return Convert.ChangeType(value, targetType, culture);
        }
        catch (Exception)
        {
            return value;
        }
    }

    private sealed class DynamicDataFilteringAdapter : DataGridFilteringAdapter
    {
        private readonly Action<IReadOnlyList<FilteringDescriptor>> _update;

        public DynamicDataFilteringAdapter(
            IFilteringModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<FilteringDescriptor>> update)
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<FilteringDescriptor> descriptors,
            IReadOnlyList<FilteringDescriptor> previousDescriptors,
            out bool changed)
        {
            _update(descriptors);
            changed = true;
            return true;
        }
    }
}

/// <summary>
/// Adapter factory that translates SearchModel descriptors into a DynamicData search predicate.
/// It can push search criteria upstream while letting the grid compute match highlighting.
/// </summary>
public sealed class DynamicDataSearchAdapterFactory<T> : IDataGridSearchAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ColumnSelector[] _allColumns;
    private readonly Dictionary<string, ColumnSelector> _columnsByKey;

    public DynamicDataSearchAdapterFactory(IReadOnlyDictionary<string, IResourceListColumn> resourceColumnsByKey)
    {
        _allColumns = new ColumnSelector[resourceColumnsByKey.Count];
        _columnsByKey = new Dictionary<string, ColumnSelector>(resourceColumnsByKey.Count, StringComparer.OrdinalIgnoreCase);

        var index = 0;
        foreach (var pair in resourceColumnsByKey)
        {
            var selector = new ColumnSelector(pair.Key, pair.Value.DisplayValue);
            _allColumns[index++] = selector;
            _columnsByKey[pair.Key] = selector;
        }

        SearchPredicate = static _ => true;
    }

    public Func<T, bool> SearchPredicate { get; private set; }

    public DataGridSearchAdapter Create(DataGrid grid, ISearchModel model)
    {
        return new DynamicDataSearchAdapter(model, () => grid.ColumnDefinitions, UpdatePredicate);
    }

    public void UpdatePredicate(IReadOnlyList<SearchDescriptor> descriptors)
    {
        SearchPredicate = BuildPredicate(descriptors);
    }

    private Func<T, bool> BuildPredicate(IReadOnlyList<SearchDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return static _ => true;
        }

        var compiled = new List<Func<T, bool>>();
        foreach (var descriptor in descriptors)
        {
            var predicate = Compile(descriptor);
            if (predicate != null)
            {
                compiled.Add(predicate);
            }
        }

        if (compiled.Count == 0)
        {
            return static _ => true;
        }

        return item =>
        {
            for (int i = 0; i < compiled.Count; i++)
            {
                if (compiled[i](item))
                {
                    return true;
                }
            }

            return false;
        };
    }

    private Func<T, bool>? Compile(SearchDescriptor descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        var columns = SelectColumns(descriptor);
        if (columns.Length == 0)
        {
            return null;
        }

        return item =>
        {
            for (var i = 0; i < columns.Length; i++)
            {
                string? text;
                try
                {
                    text = columns[i].Getter(item);
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (TextMatcher.HasMatch(text, descriptor))
                {
                    return true;
                }
            }

            return false;
        };
    }

    private ColumnSelector[] SelectColumns(SearchDescriptor descriptor)
    {
        if (descriptor.Scope != SearchScope.ExplicitColumns)
        {
            return _allColumns;
        }

        if (descriptor.ColumnIds == null || descriptor.ColumnIds.Count == 0)
        {
            return [];
        }

        var selected = new List<ColumnSelector>(descriptor.ColumnIds.Count);
        for (var i = 0; i < descriptor.ColumnIds.Count; i++)
        {
            var id = descriptor.ColumnIds[i];
            if (!TryResolveColumnKey(id, out var key))
            {
                continue;
            }

            if (_columnsByKey.TryGetValue(key, out var selector))
            {
                selected.Add(selector);
            }
        }

        return selected.Count == 0 ? [] : selected.ToArray();
    }

    private static bool TryResolveColumnKey(object? columnId, out string key)
    {
        switch (columnId)
        {
            case null:
                key = string.Empty;
                return false;
            case string stringKey when !string.IsNullOrWhiteSpace(stringKey):
                key = stringKey;
                return true;
            case DataGridColumnDefinition definition when definition.ColumnKey is not null:
                key = definition.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            case DataGridColumn column when column.ColumnKey is not null:
                key = column.ColumnKey.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(key);
            default:
                key = string.Empty;
                return false;
        }
    }

    private sealed class DynamicDataSearchAdapter : DataGridSearchAdapter
    {
        private readonly Action<IReadOnlyList<SearchDescriptor>> _update;

        public DynamicDataSearchAdapter(
            ISearchModel model,
            Func<IEnumerable<DataGridColumn>> columns,
            Action<IReadOnlyList<SearchDescriptor>> update
            )
            : base(model, columns)
        {
            _update = update;
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<SearchDescriptor> descriptors,
            IReadOnlyList<SearchDescriptor> previousDescriptors,
            out IReadOnlyList<SearchResult> results)
        {
            _update(descriptors);
            results = [];
            return false;
        }
    }

    private sealed class ColumnSelector
    {
        public ColumnSelector(string id, Func<T, string> getter)
        {
            Id = id;
            Getter = getter;
        }

        public string Id { get; }

        public Func<T, string> Getter { get; }
    }

    private static class TextMatcher
    {
        public static bool HasMatch(string text, SearchDescriptor descriptor)
        {
            if (descriptor == null || string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (string.IsNullOrEmpty(descriptor.Query))
            {
                return descriptor.AllowEmpty && text.Length > 0;
            }

            var normalizedText = NormalizeText(text, descriptor.NormalizeWhitespace, descriptor.IgnoreDiacritics);
            var query = NormalizeQuery(descriptor.Query, descriptor.NormalizeWhitespace, descriptor.IgnoreDiacritics);

            if (descriptor.MatchMode == SearchMatchMode.Regex || descriptor.MatchMode == SearchMatchMode.Wildcard)
            {
                var pattern = descriptor.MatchMode == SearchMatchMode.Wildcard
                    ? WildcardToRegex(query)
                    : query;

                if (descriptor.WholeWord)
                {
                    pattern = $@"\b(?:{pattern})\b";
                }

                var options = RegexOptions.Compiled;
                if (IsIgnoreCase(descriptor.Comparison))
                {
                    options |= RegexOptions.IgnoreCase;
                }

                if (IsCultureInvariant(descriptor.Comparison))
                {
                    options |= RegexOptions.CultureInvariant;
                }

                try
                {
                    return Regex.IsMatch(normalizedText, pattern, options);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            var terms = Tokenize(query);
            if (terms.Count == 0)
            {
                return false;
            }

            var comparison = descriptor.Comparison ?? StringComparison.OrdinalIgnoreCase;
            bool any = descriptor.TermMode == SearchTermCombineMode.Any;

            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term))
                {
                    continue;
                }

                var matched = FindTermMatch(normalizedText, term, descriptor.MatchMode, comparison, descriptor.WholeWord);
                if (matched && any)
                {
                    return true;
                }

                if (!matched && !any)
                {
                    return false;
                }
            }

            return !any;
        }

        private static bool FindTermMatch(
            string text,
            string term,
            SearchMatchMode mode,
            StringComparison comparison,
            bool wholeWord)
        {
            switch (mode)
            {
                case SearchMatchMode.StartsWith:
                    return text.StartsWith(term, comparison) && IsWholeWord(text, 0, term.Length, wholeWord);
                case SearchMatchMode.EndsWith:
                    if (!text.EndsWith(term, comparison))
                    {
                        return false;
                    }

                    var start = text.Length - term.Length;
                    return IsWholeWord(text, start, term.Length, wholeWord);
                case SearchMatchMode.Equals:
                    return string.Equals(text, term, comparison) && IsWholeWord(text, 0, term.Length, wholeWord);
                default:
                    return FindAllOccurrences(text, term, comparison, wholeWord);
            }
        }

        private static bool FindAllOccurrences(
            string text,
            string term,
            StringComparison comparison,
            bool wholeWord)
        {
            if (string.IsNullOrEmpty(term))
            {
                return false;
            }

            int startIndex = 0;
            while (startIndex < text.Length)
            {
                int index = text.IndexOf(term, startIndex, comparison);
                if (index < 0)
                {
                    break;
                }

                if (IsWholeWord(text, index, term.Length, wholeWord))
                {
                    return true;
                }

                startIndex = index + term.Length;
            }

            return false;
        }

        private static bool IsWholeWord(string text, int start, int length, bool wholeWord)
        {
            if (!wholeWord)
            {
                return true;
            }

            bool startBoundary = start == 0 || !IsWordChar(text[start - 1]);
            int endIndex = start + length;
            bool endBoundary = endIndex >= text.Length || !IsWordChar(text[endIndex]);

            return startBoundary && endBoundary;
        }

        private static bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static List<string> Tokenize(string query)
        {
            var terms = new List<string>();
            if (string.IsNullOrWhiteSpace(query))
            {
                return terms;
            }

            var builder = new StringBuilder();
            bool inQuote = false;

            foreach (var ch in query)
            {
                if (ch == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote && char.IsWhiteSpace(ch))
                {
                    Flush(builder, terms);
                    continue;
                }

                builder.Append(ch);
            }

            Flush(builder, terms);
            return terms;
        }

        private static void Flush(StringBuilder builder, List<string> terms)
        {
            if (builder.Length == 0)
            {
                return;
            }

            var term = builder.ToString().Trim();
            if (!string.IsNullOrEmpty(term))
            {
                terms.Add(term);
            }

            builder.Clear();
        }

        private static string NormalizeText(string text, bool normalizeWhitespace, bool ignoreDiacritics)
        {
            if (!normalizeWhitespace && !ignoreDiacritics)
            {
                return text;
            }

            var chars = new List<char>();
            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ignoreDiacritics)
                {
                    foreach (var d in ch.ToString().Normalize(NormalizationForm.FormD))
                    {
                        if (IsDiacritic(d))
                        {
                            continue;
                        }

                        chars.Add(d);
                    }
                }
                else
                {
                    chars.Add(ch);
                }
            }

            if (!normalizeWhitespace)
            {
                return new string(chars.ToArray());
            }

            var builder = new StringBuilder();
            bool wasWhitespace = false;

            for (int i = 0; i < chars.Count; i++)
            {
                var ch = chars[i];
                bool isWhitespace = char.IsWhiteSpace(ch);
                if (isWhitespace)
                {
                    if (wasWhitespace)
                    {
                        continue;
                    }

                    builder.Append(' ');
                    wasWhitespace = true;
                }
                else
                {
                    builder.Append(ch);
                    wasWhitespace = false;
                }
            }

            return builder.ToString();
        }

        private static string NormalizeQuery(string query, bool normalizeWhitespace, bool ignoreDiacritics)
        {
            if (!normalizeWhitespace && !ignoreDiacritics)
            {
                return query;
            }

            var normalized = NormalizeText(query, normalizeWhitespace, ignoreDiacritics);
            return normalizeWhitespace ? normalized.Trim() : normalized;
        }

        private static bool IsDiacritic(char ch)
        {
            return CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark;
        }

        private static string WildcardToRegex(string pattern)
        {
            var builder = new StringBuilder();
            foreach (var ch in pattern)
            {
                switch (ch)
                {
                    case '*':
                        builder.Append(".*");
                        break;
                    case '?':
                        builder.Append(".");
                        break;
                    default:
                        builder.Append(Regex.Escape(ch.ToString()));
                        break;
                }
            }

            return builder.ToString();
        }

        private static bool IsIgnoreCase(StringComparison? comparison)
        {
            if (!comparison.HasValue)
            {
                return true;
            }

            switch (comparison.Value)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsCultureInvariant(StringComparison? comparison)
        {
            if (!comparison.HasValue)
            {
                return true;
            }

            switch (comparison.Value)
            {
                case StringComparison.Ordinal:
                case StringComparison.OrdinalIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    return true;
                default:
                    return false;
            }
        }
    }
}




