using System.Collections.Specialized;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSelection;
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
using KubeUI.Client;
using KubeUI.Resources;
using SortDirection = KubeUI.Resources.SortDirection;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    private static readonly IComparer s_noopSortComparer = Comparer<object>.Create(static (_, _) => 0);

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    public GroupApiVersionKind Kind => GroupApiVersionKind.From<T>();

    [ObservableProperty]
    public partial ISourceCache<T, string> Objects { get; set; }

    public T? SelectedItem => ((SelectionModel<T>)SelectionModel).SelectedItem;

    public IReadOnlyList<T?> SelectedItems => ((SelectionModel<T>)SelectionModel).SelectedItems;

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    private IDisposable? _subscription;

    public IEnumerable View => _view;

    private ReadOnlyObservableCollection<T> _view;

    private BehaviorSubject<IComparer<T>> _sortSubject;

    public IDataGridSortingAdapterFactory SortingAdapterFactory => _sortingAdapterFactory;

    private DynamicDataSortingAdapterFactory<T> _sortingAdapterFactory { get; set; }

    [ObservableProperty]
    public partial ISortingModel SortingModel { get; set; } = new SortingModel
    {
        MultiSort = true,
        CycleMode = SortCycleMode.AscendingDescendingNone,
        OwnsViewSorts = true
    };

    private BehaviorSubject<Func<T, bool>> _filterSubject;

    public IDataGridFilteringAdapterFactory FilteringAdapterFactory => _filteringAdapterFactory;

    private DynamicDataFilteringAdapterFactory<T> _filteringAdapterFactory { get; set; }

    [ObservableProperty]
    public partial IFilteringModel FilteringModel { get; set; } = new FilteringModel
    {
        OwnsViewFilter = true
    };

    private BehaviorSubject<Func<T, bool>> _searchSubject;

    public IDataGridSearchAdapterFactory SearchAdapterFactory => _searchAdapterFactory;

    private DynamicDataSearchAdapterFactory<T> _searchAdapterFactory;

    [ObservableProperty]
    public partial ISearchModel SearchModel { get; set; } = new SearchModel()
    {
        HighlightMode = SearchHighlightMode.None,
        HighlightCurrent = false,
        WrapNavigation = true,
        UpdateSelectionOnNavigate = false
    };

    public IDataGridSelectionModelFactory SelectionModelFactory => _selectionModelFactory;

    private DynamicDataSelectionModelFactory<T> _selectionModelFactory { get; set; }

    private readonly HashSet<string> _selectedKeys = new(StringComparer.Ordinal);
    private bool _suppressSelectionChanged;
    private bool _isUpdatingCollection;

    public ISelectionModel SelectionModel { get; } = new SelectionModel<T>()
    {
        SingleSelect = false
    };

    [ObservableProperty]
    public partial ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; private set; } = [];

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
        SettingsService = Application.Current.GetRequiredService<ISettingsService>();

        SortingModel.SortingChanged += SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged += FilteringModelOnFilteringChanged;
        SearchModel.SearchChanged += SearchModelOnSearchChanged;
        ((SelectionModel<T>)SelectionModel).SelectionChanged += SelectionModelOnSelectionChanged;
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Title = Kind.Kind.Humanize(LetterCasing.Title).Pluralize();
        Id = Cluster.Name + "-" + Kind;
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);
        GenerateColumnDefinitions();
        Cluster.SeedResource<T>().GetAwaiter().GetResult();

        Objects = Cluster.GetResourceSourceCache<T>();
        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        _subscription?.Dispose();

        _sortingAdapterFactory = new DynamicDataSortingAdapterFactory<T>(ResourceConfig);
        _sortSubject = new BehaviorSubject<IComparer<T>>(_sortingAdapterFactory.SortComparer);
        _filteringAdapterFactory = new DynamicDataFilteringAdapterFactory<T>(ResourceConfig);
        _filterSubject = new BehaviorSubject<Func<T, bool>>(_filteringAdapterFactory.FilterPredicate);
        _searchAdapterFactory = new DynamicDataSearchAdapterFactory<T>(ResourceConfig);
        _searchSubject = new BehaviorSubject<Func<T, bool>>(_searchAdapterFactory.SearchPredicate);


        _selectionModelFactory = new DynamicDataSelectionModelFactory<T>(SelectionModel);

        SetFilter();
        ApplySearch();

        _subscription = Objects.Connect()
        .Do(_ => _isUpdatingCollection = true)
        .Filter(_filterSubject)
        .Filter(_searchSubject)
        .ObserveOn(AvaloniaScheduler.Instance)
        .SortAndBind(out _view, _sortSubject, new()
        {
            ResetOnFirstTimeLoad = true,
            UseReplaceForUpdates = true,
            UseBinarySearch = true,
            InitialCapacity = Objects.Count
        })
        .Subscribe(change =>
        {
            PreserveSelectionByKey();
            _isUpdatingCollection = false;
        }, ex =>
        {
            _isUpdatingCollection = false;
            _logger.LogError(ex, "Error Setting Resource List Filter: {ns} ", typeof(T));
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchQuery))
        {
            ApplySearch();
        }
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetFilter();
    }

    private void SetFilter()
    {
        if (!ResourceConfig.IsNamespaced)
        {
            return;
        }

        FilteringModel.Clear();

        foreach (var col in ColumnDefinitions)
        {
            if (col.Header == "Namespace") //todo better lookup for standard columns
            {
                if (Cluster.SelectedNamespaces.Count > 0)
                {
                    var descriptor = new FilteringDescriptor(
                        col,
                        FilteringOperator.In,
                        propertyPath: null,
                        value: null,
                        values: [.. Cluster.SelectedNamespaces.Select(x => x.Name() as object)]);

                    FilteringModel.SetOrUpdate(descriptor);
                }
                else
                {
                    FilteringModel.Remove(col);
                }

                return;
            }
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

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        SortingModel.SortingChanged -= SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged -= FilteringModelOnFilteringChanged;
        SearchModel.SearchChanged -= SearchModelOnSearchChanged;
        ((SelectionModel<T>)SelectionModel).SelectionChanged -= SelectionModelOnSelectionChanged;
    }

    private void GenerateColumnDefinitions()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(GenerateColumnDefinitions).GetAwaiter().GetResult();
            return;
        }

        ColumnDefinitions.Clear();

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in ResourceConfig.Columns())
        {
            try
            {
                var column = new DataGridControlTemplateColumnDefinition()
                {
                    Header = columnDefinition.Name,
                    CellTemplate = new FuncDataTemplate<T>((item, _) =>
                    {
                        try
                        {
                            var control = Application.Current.GetRequiredService(columnDefinition.CustomControl) as Control;

                            if (control is IDisplayFunc init2)
                            {
                                init2.SetDisplayFunc(columnDefinition.DisplayValue);
                            }

                            if (control is IInitializeCluster init)
                            {
                                init.Initialize(Cluster);
                            }

                            control.DataContext = item;

                            return control;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating Control");
                            return new TextBlock() { Text = ex.Message };
                        }
                    }, supportsRecycling: true),
                    CanUserSort = true,
                    CustomSortComparer = s_noopSortComparer,
                    SortDirection = columnDefinition.Sort == SortDirection.None ? null : columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending,
                    Width = columnDefinition.Width != null ? converter.ConvertFromString(columnDefinition.Width) as DataGridLength? : null,
                    ValueType = columnDefinition.ValueType,
                    Options = new()
                    {
                        // Causes lag on large lists
                        //SearchTextProvider = columnDefinition.DisplayValue,
                    }
                };

                ColumnDefinitions.Add(column);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }
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

    private static string? GetKey(T item)
    {
        var meta = item?.Metadata;
        if (meta is null)
        {
            return null;
        }

        var ns = meta.NamespaceProperty ?? string.Empty;
        var name = meta.Name ?? string.Empty;

        return ns + "/" + name;
    }

    private void PreserveSelectionByKey()
    {
        if (_view is null)
        {
            return;
        }

        var selection = (SelectionModel<T>)SelectionModel;

        if (_selectedKeys.Count == 0)
        {
            return;
        }

        _suppressSelectionChanged = true;
        selection.Clear();

        for (var i = 0; i < _view.Count; i++)
        {
            var key = GetKey(_view[i]);
            if (key != null && _selectedKeys.Contains(key))
            {
                selection.Select(i);

                if (selection.SingleSelect)
                {
                    break;
                }
            }
        }

        _suppressSelectionChanged = false;
    }

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<T> e)
    {
        if (_isUpdatingCollection)
        {
            return;
        }

        var selection = (SelectionModel<T>)SelectionModel;

        if (_suppressSelectionChanged)
        {
            return;
        }

        _selectedKeys.Clear();

        foreach (var key in selection.SelectedItems.Select(GetKey).Where(k => k != null))
        {
            _selectedKeys.Add(key!);
        }

        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(SelectedItems));
    }

    private void SearchModelOnSearchChanged(object? sender, SearchChangedEventArgs e)
    {
        _searchAdapterFactory.UpdatePredicate(e.NewDescriptors);
        _searchSubject.OnNext(_searchAdapterFactory.SearchPredicate);
    }
}

public sealed class DynamicDataSortingAdapterFactory<T> : IDataGridSortingAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ResourceConfigBase<T> _resourceConfig;

    private static readonly IComparer<T> s_noopComparer = Comparer<T>.Create(static (_, _) => 0);

    public IComparer<T> SortComparer { get; private set; }

    public DynamicDataSortingAdapterFactory(ResourceConfigBase<T> resourceConfig)
    {
        SortComparer = s_noopComparer;
        _resourceConfig = resourceConfig;
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

        foreach (var descriptor in descriptors.Where(static d => d != null))
        {
            var selector = CreateSelector(descriptor);

            if (selector == null)
            {
                continue;
            }

            comparer = comparer == null
                ? descriptor.Direction == ListSortDirection.Ascending
                    ? SortExpressionComparer<T>.Ascending(selector)
                    : SortExpressionComparer<T>.Descending(selector)
                : descriptor.Direction == ListSortDirection.Ascending
                    ? comparer.ThenByAscending(selector)
                    : comparer.ThenByDescending(selector);
        }

        return comparer ?? s_noopComparer;
    }

    private Func<T, IComparable?>? CreateSelector(SortingDescriptor descriptor)
    {
        if (descriptor.ColumnId is not DataGridColumnDefinition gridColumn)
        {
            return null;
        }

        if (gridColumn.Header is not string header || string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        return _resourceConfig.Columns().First(x => x.Name == header).SortKey;
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

    private readonly ResourceConfigBase<T> _resourceConfig;

    public DynamicDataFilteringAdapterFactory(ResourceConfigBase<T> resourceConfig)
    {
        FilterPredicate = s_alwaysTrue;
        _resourceConfig = resourceConfig;
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
            return item => predicate(item);
        }

        var selector = CreateSelector(descriptor);
        if (selector == null)
        {
            return null;
        }

        var culture = descriptor.Culture ?? System.Globalization.CultureInfo.InvariantCulture;
        var stringComparison = descriptor.StringComparisonMode ?? StringComparison.OrdinalIgnoreCase;
        var values = descriptor.Values;
        var value = descriptor.Value;

        return descriptor.Operator switch
        {
            FilteringOperator.Equals => item => Equals(selector(item), value),
            FilteringOperator.NotEquals => item => !Equals(selector(item), value),
            FilteringOperator.Contains => item => Contains(selector(item), value, stringComparison),
            FilteringOperator.StartsWith => item => StartsWith(selector(item), value, stringComparison),
            FilteringOperator.EndsWith => item => EndsWith(selector(item), value, stringComparison),
            FilteringOperator.GreaterThan => item => Compare(selector(item), value, culture) > 0,
            FilteringOperator.GreaterThanOrEqual => item => Compare(selector(item), value, culture) >= 0,
            FilteringOperator.LessThan => item => Compare(selector(item), value, culture) < 0,
            FilteringOperator.LessThanOrEqual => item => Compare(selector(item), value, culture) <= 0,
            FilteringOperator.Between => item => Between(selector(item), values, culture),
            FilteringOperator.In => item => In(selector(item), values),
            _ => s_alwaysTrue
        };
    }

    private Func<T, object?>? CreateSelector(FilteringDescriptor descriptor)
    {
        if (descriptor.ColumnId is not DataGridColumnDefinition gridColumn)
        {
            return null;
        }

        if (gridColumn.Header is not string header || string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        return _resourceConfig.Columns().First(x => x.Name == header).DisplayValue;
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

public sealed class DynamicDataSelectionModelFactory<T> : IDataGridSelectionModelFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ISelectionModel _selectionModel;

    public DynamicDataSelectionModelFactory(ISelectionModel selectionModel)
    {
        _selectionModel = selectionModel;
    }

    public ISelectionModel Create()
    {
        return _selectionModel;
    }
}

/// <summary>
/// Adapter factory that translates SearchModel descriptors into a DynamicData search predicate.
/// It can push search criteria upstream while letting the grid compute match highlighting.
/// </summary>
public sealed class DynamicDataSearchAdapterFactory<T> : IDataGridSearchAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ResourceConfigBase<T> _resourceConfig;

    public DynamicDataSearchAdapterFactory(ResourceConfigBase<T> resourceConfig)
    {
        _resourceConfig = resourceConfig;
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
        if (columns.Count == 0)
        {
            return null;
        }

        return item =>
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var text = columns[i].Getter(item);
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

    private List<ColumnSelector> SelectColumns(SearchDescriptor descriptor)
    {
        if (descriptor.Scope != SearchScope.ExplicitColumns)
        {
            return [.. _resourceConfig.Columns().Select(x => new ColumnSelector(x.Name, x.DisplayValue))];
        }

        if (descriptor.ColumnIds == null || descriptor.ColumnIds.Count == 0)
        {
            return [];
        }

        var selected = new List<ColumnSelector>();
        foreach (var id in descriptor.ColumnIds)
        {
            if (id is not string path)
            {
                continue;
            }

            var column = _resourceConfig.Columns().FirstOrDefault(c => string.Equals(c.Name, path, StringComparison.Ordinal));
            if (column != null)
            {
                selected.Add(new ColumnSelector(column.Name, column.DisplayValue));
            }
        }

        return selected;
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
