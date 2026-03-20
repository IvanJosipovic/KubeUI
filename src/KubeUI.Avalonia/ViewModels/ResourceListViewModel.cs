using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources;
using SortDirection = KubeUI.Avalonia.Resources.SortDirection;

namespace KubeUI.Avalonia.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
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

    private readonly SelectionModel<T> _selectionModel = new()
    {
        SingleSelect = false
    };

    private readonly List<T> _selectionSnapshot = [];

    public IList View => _view;

    private ReadOnlyObservableCollection<T> _view;

    private BehaviorSubject<IComparer<T>> _sortSubject;

    public IDataGridSortingAdapterFactory SortingAdapterFactory => _sortingAdapterFactory;

    private DynamicDataSortingAdapterFactory<T> _sortingAdapterFactory = null!;

    [ObservableProperty]
    public partial ISortingModel SortingModel { get; set; } = new SortingModel
    {
        MultiSort = true,
        CycleMode = SortCycleMode.AscendingDescendingNone,
        OwnsViewSorts = true
    };

    private BehaviorSubject<Func<T, bool>> _filterSubject;

    public IDataGridFilteringAdapterFactory FilteringAdapterFactory => _filteringAdapterFactory;

    private DynamicDataFilteringAdapterFactory<T> _filteringAdapterFactory = null!;

    [ObservableProperty]
    public partial IFilteringModel FilteringModel { get; set; } = new FilteringModel
    {
        OwnsViewFilter = true
    };

    private BehaviorSubject<Func<T, bool>> _searchSubject;

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

    public ISelectionModel SelectionModel => _selectionModel;

    public Func<IList, object, int> ReferenceIndexResolver => ResolveReferenceIndex;

    [ObservableProperty]
    public partial ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; private set; } = [];

    private readonly Dictionary<string, IResourceListColumn> _resourceColumnsByKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DataGridColumnDefinition> _columnDefinitionsByKey = new(StringComparer.OrdinalIgnoreCase);
    private IList<IResourceListColumn> _resourceColumns = [];

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
        SettingsService = Application.Current.GetRequiredService<ISettingsService>();

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
        _sortSubject = new BehaviorSubject<IComparer<T>>(_sortingAdapterFactory.SortComparer);

        _filteringAdapterFactory = new DynamicDataFilteringAdapterFactory<T>(_resourceColumnsByKey);
        _filterSubject = new BehaviorSubject<Func<T, bool>>(_filteringAdapterFactory.FilterPredicate);

        _searchAdapterFactory = new DynamicDataSearchAdapterFactory<T>(_resourceColumnsByKey);
        _searchSubject = new BehaviorSubject<Func<T, bool>>(_searchAdapterFactory.SearchPredicate);

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
            ApplySearch();
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

        if (!_columnDefinitionsByKey.TryGetValue("namespace", out var namespaceColumn))
        {
            return;
        }

        if (Cluster.SelectedNamespaces.Count > 0)
        {
            var values = new List<object>(Cluster.SelectedNamespaces.Count);
            foreach (var selectedNamespace in Cluster.SelectedNamespaces)
            {
                values.Add(selectedNamespace.Name()!);
            }

            var descriptor = new FilteringDescriptor(
                namespaceColumn,
                FilteringOperator.In,
                propertyPath: null,
                value: null,
                values: values);

            FilteringModel.SetOrUpdate(descriptor);
        }
        else
        {
            FilteringModel.Remove(namespaceColumn);
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

        if (Cluster != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        }

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
        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        _subscription?.Dispose();

        SetNamespaceFilter();

        _subscription = Objects.Connect()
            .ObserveOn(AvaloniaScheduler.Instance)
            .Do(_ => CaptureSelectionSnapshot())
            .Filter(_filterSubject)
            .Filter(_searchSubject)
            .SortAndBind(out _view, _sortSubject, new()
            {
                ResetOnFirstTimeLoad = true,
                UseReplaceForUpdates = true,
                UseBinarySearch = true,
                InitialCapacity = Objects.Count
            })
            .Subscribe(
                _ => SynchronizeSelectionWithView(),
                ex => _logger.LogError(ex, "Error Setting Resource List Filter: {ns} ", typeof(T))
            );
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
        return new DataGridControlTemplateColumnDefinition
        {
            Header = columnDefinition.Name,
            ColumnKey = columnDefinition.Key,
            Tag = columnDefinition,
            CellTemplate = CreateCellTemplate(columnDefinition),
            CanUserSort = true,
            CustomSortComparer = s_noopSortComparer,
            Width = ParseWidth(columnDefinition.Width, converter),
            ValueAccessor = columnDefinition.ValueAccessor,
            ValueType = columnDefinition.ValueType,
            Options = BuildColumnOptions(columnDefinition)
        };
    }

    private FuncDataTemplate<T> CreateCellTemplate(IResourceListColumn columnDefinition)
    {
        return new FuncDataTemplate<T>((item, _) =>
        {
            try
            {
                var control = Application.Current.GetRequiredService(columnDefinition.CustomControl) as Control;

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
        _sortSubject.OnNext(_sortingAdapterFactory.SortComparer);
    }

    private void FilteringModelOnFilteringChanged(object? sender, FilteringChangedEventArgs e)
    {
        _filteringAdapterFactory.UpdateFilter(e.NewDescriptors);
        _filterSubject.OnNext(_filteringAdapterFactory.FilterPredicate);
    }

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<T> e)
    {
        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(SelectedItems));
        OnPropertyChanged(nameof(ContextMenuItems));
    }

    private void SearchModelOnSearchChanged(object? sender, SearchChangedEventArgs e)
    {
        _searchAdapterFactory.UpdatePredicate(e.NewDescriptors);
        _searchSubject.OnNext(_searchAdapterFactory.SearchPredicate);
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
        if (_selectionSnapshot.Count == 0 || _view.Count == 0)
        {
            return;
        }

        var desiredIndexes = new List<int>(_selectionSnapshot.Count);

        for (var i = 0; i < _view.Count; i++)
        {
            if (_view[i] is not T resource)
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

            IComparable? SafeSelect(T item)
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
        if (!TryResolveResourceColumn(descriptor.ColumnId, out var column))
        {
            return null;
        }

        return column.SortKey;
    }

    private bool TryResolveResourceColumn(object? columnId, out IResourceListColumn column)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                column = taggedColumn;
                return true;
            }

            if (TryResolveKey(definition.ColumnKey, out var key) && _resourceColumnsByKey.TryGetValue(key, out column))
            {
                return true;
            }
        }

        if (TryResolveKey(columnId, out var directKey) && _resourceColumnsByKey.TryGetValue(directKey, out column))
        {
            return true;
        }

        column = null!;
        return false;
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
}

public sealed class DynamicDataFilteringAdapterFactory<T> : IDataGridFilteringAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new ()
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
        if (!TryResolveResourceColumn(descriptor.ColumnId, out var column))
        {
            return null;
        }

        return column.DisplayValue;
    }

    private bool TryResolveResourceColumn(object? columnId, out IResourceListColumn column)
    {
        if (columnId is DataGridColumnDefinition definition)
        {
            if (definition.Tag is IResourceListColumn taggedColumn)
            {
                column = taggedColumn;
                return true;
            }

            if (TryResolveKey(definition.ColumnKey, out var key) && _resourceColumnsByKey.TryGetValue(key, out column))
            {
                return true;
            }
        }

        if (TryResolveKey(columnId, out var directKey) && _resourceColumnsByKey.TryGetValue(directKey, out column))
        {
            return true;
        }

        column = null!;
        return false;
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

        if (left is IComparable comparable)
        {
            return comparable.CompareTo(ChangeType(right, left.GetType(), culture));
        }

        return Comparer<object>.Default.Compare(left, right);
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
                    var decomposed = ch.ToString().Normalize(NormalizationForm.FormD);
                    foreach (var d in decomposed)
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




