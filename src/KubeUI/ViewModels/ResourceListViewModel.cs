using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data.Core;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using Yarp.Kubernetes.Controller.Client;
using SortDirection = KubeUI.Resources.SortDirection;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    private static readonly IComparer s_noopSortComparer = Comparer<object>.Create(static (_, _) => 0);

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

    [ObservableProperty]
    public partial ISourceCache<T, string> Objects { get; set; }

    [ObservableProperty]
    public partial T SelectedItem { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<T> SelectedItems { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    private IDisposable? _subscription;

    // new

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

        _subscription = Objects.Connect()
                                        .Filter(_filterSubject)
                                        .ObserveOn(AvaloniaScheduler.Instance)
                                        .SortAndBind(out _view, _sortSubject)
                                        .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Setting Resource List Filter: {ns} ", typeof(T)));
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchQuery))
        {
            SetFilter();
        }
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        //SetFilter();
    }

    private void SetFilter()
    {
        if (FilteringModel == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteringModel.Clear();
            return;
        }

        var query = SearchQuery.Trim();
        FilteringModel.Clear();

        foreach (var col in ResourceConfig.Columns())
        {
            var descriptor = new FilteringDescriptor(
                col,
                FilteringOperator.Contains,
                null,
                query);

            FilteringModel.SetOrUpdate(descriptor);
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        SortingModel.SortingChanged -= SortingModelOnSortingChanged;
        FilteringModel.FilteringChanged -= FilteringModelOnFilteringChanged;
    }

    private void GenerateColumnDefinitions()
    {
        ColumnDefinitions.Clear();

        var converter = new DataGridLengthConverter();

        var builder = DataGridColumnDefinitionBuilder.For<T>();

        foreach (var columnDefinition in ResourceConfig.Columns())
        {
            try
            {
                DataGridColumnDefinition column = null;

                if (columnDefinition.CustomControl != null)
                {
                    column = new DataGridControlTemplateColumnDefinition()
                    {
                        Header = columnDefinition.Name,
                        CellTemplate = new FuncDataTemplate<T>((item, _) =>
                            {
                                var control = Application.Current.GetRequiredService(columnDefinition.CustomControl) as Control;
                                control.DataContext = item;

                                if (control is IInitializeCluster init)
                                {
                                    init.Initialize(Cluster);
                                }

                                return control;
                            }),
                        CanUserSort = true,
                        CustomSortComparer = s_noopSortComparer,
                        SortDirection = columnDefinition.Sort == SortDirection.None ? null : columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending,
                        Width = columnDefinition.Width != null ? converter.ConvertFromString(columnDefinition.Width) as DataGridLength? : null,
                        ValueType = columnDefinition.ValueType,
                    };
                }
                else
                {
                    column = builder.Text(
                        header: columnDefinition.Name,
                        property: CreateProperty(columnDefinition.Name, p => columnDefinition.DisplayValue(p)),
                        getter: p => columnDefinition.DisplayValue(p),
                        setter : null,
                        configure: ct =>
                        {
                            ct.CanUserSort = true;
                            ct.CustomSortComparer = s_noopSortComparer;
                            ct.SortDirection = columnDefinition.Sort == SortDirection.None ? null : columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                            ct.Width = columnDefinition.Width != null ? converter.ConvertFromString(columnDefinition.Width) as DataGridLength? : null;
                            ct.ValueType = columnDefinition.ValueType;
                        }
                    );
                }

                ColumnDefinitions.Add(column);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate column for: ");
            }
        }
    }

    private static IPropertyInfo CreateProperty<TValue>(
    string name,
    Func<T, TValue> getter,
    Action<T, TValue>? setter = null)
    {
        return new ClrPropertyInfo(
            name,
            target => getter((T)target),
            setter == null
                ? null
                : (target, value) => setter((T)target, value is null ? default! : (TValue)value),
            typeof(TValue));
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
