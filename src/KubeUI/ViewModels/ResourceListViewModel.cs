using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using KubeUI.Views;
using Yarp.Kubernetes.Controller.Client;
using SortDirection = KubeUI.Resources.SortDirection;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    private static readonly IComparer sNoopSortComparer = Comparer<object>.Create(static (_, _) => 0);

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial GroupApiVersionKind Kind { get; set; }

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

    [ObservableProperty]
    public partial ObservableCollection<DataGridColumn> Columns { get; set; } = [];

    private IDisposable? _subscription;

    // new

    public ReadOnlyObservableCollection<T> View => _view;
    private ReadOnlyObservableCollection<T> _view;

    private BehaviorSubject<IComparer<T>> _sortSubject;

    public IDataGridSortingAdapterFactory SortingAdapterFactory => _sortingAdapterFactory;

    private SortingAdapterFactory<T> _sortingAdapterFactory { get; set; }

    [ObservableProperty]
    public partial ISortingModel SortingModel { get; set; }


    public SelectionModel<T> SelectionModel { get; } = new SelectionModel<T>()
    {
        SingleSelect = false
    };

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Kind = GroupApiVersionKind.From<T>();
        Title = Kind.Kind.Humanize(LetterCasing.Title).Pluralize();
        Id = Cluster.Name + "-" + Kind;
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);

        Cluster.SeedResource<T>().GetAwaiter().GetResult();

        Objects = Cluster.GetResourceSourceCache<T>();
        Dispatcher.UIThread.Invoke(() => GenerateGrid<T>());
        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        _subscription?.Dispose();

        _sortingAdapterFactory = new SortingAdapterFactory<T>(ResourceConfig, Columns);
        _sortSubject = new BehaviorSubject<IComparer<T>>(_sortingAdapterFactory.SortComparer);

        _subscription = Objects.Connect()
                                        //.Filter(GenerateFilter())
                                        .ObserveOn(AvaloniaScheduler.Instance)
                                        .SortAndBind(out _view, _sortSubject)
                                        .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Setting Resource List Filter: {ns} ", typeof(T)));

        SortingModel = new SortingModel
        {
            MultiSort = true,
            CycleMode = SortCycleMode.AscendingDescendingNone,
            OwnsViewSorts = true
        };

        SortingModel.SortingChanged += SortingModelOnSortingChanged;
    }

    private Func<T, bool> GenerateFilter()
    {
        try
        {
            var param = Expression.Parameter(typeof(T), "p");

            Expression? body = null;

            BinaryExpression? namespaceFilter = null;

            BinaryExpression? searchFilter = null;

            if (Cluster.SelectedNamespaces != null && ResourceConfig.IsNamespaced)
            {
                var nsExpr = Expression.Property(
                                Expression.Property(param, nameof(IMetadata<>.Metadata)),
                                nameof(V1ObjectMeta.NamespaceProperty)
                             );

                foreach (var item in Cluster.SelectedNamespaces)
                {
                    var expression = Expression.Equal(nsExpr, Expression.Constant(item.Name()));

                    namespaceFilter = namespaceFilter == null ? expression : Expression.OrElse(namespaceFilter, expression);
                }
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var method = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(string), typeof(StringComparison)])!;

                foreach (var query in SearchQuery.Split(' '))
                {
                    if (string.IsNullOrEmpty(query))
                    {
                        continue;
                    }

                    BinaryExpression? wordFilter = null;

                    foreach (var column in ResourceConfig.Columns())
                    {
                        var someValue = Expression.Constant(query, typeof(string));

                        var colType = column.GetType();

                        var columnDisplay = colType.GetProperty(nameof(ResourceListColumn<,>.Display))!.GetValue(column) as Func<T, string>;

                        columnDisplay ??= colType.GetProperty(nameof(ResourceListColumn<,>.Field))!.GetValue(column) as Func<T, string>;

                        if (columnDisplay != null)
                        {
                            var funcCall = Expression.Call(Expression.Constant(columnDisplay), columnDisplay.GetType()!.GetMethod("Invoke")!, param);

                            var expression = Expression.GreaterThanOrEqual(Expression.Call(funcCall, method, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase)), Expression.Constant(0));

                            wordFilter = wordFilter == null ? expression : Expression.OrElse(wordFilter, expression);
                        }
                        else
                        {
                            throw new Exception($"No string based columnDisplay, {typeof(T)} {column.Name}");
                        }
                    }

                    searchFilter = searchFilter == null ? wordFilter : Expression.AndAlso(searchFilter, wordFilter);
                }
            }

            if (namespaceFilter != null && searchFilter == null)
            {
                body = namespaceFilter;
            }
            else if (namespaceFilter == null && searchFilter != null)
            {
                body = searchFilter;
            }
            else if (namespaceFilter != null && searchFilter != null)
            {
                body = Expression.AndAlso(namespaceFilter, searchFilter);
            }
            else
            {
                body = Expression.Constant(true);
            }

            return Expression.Lambda<Func<T, bool>>(body, param).Compile();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Generating Namespace Filter Expression");

            throw new Exception("Error Generating Namespace Filter Expression", ex);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SearchQuery))
        {
            //SetFilter();
        }
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        //SetFilter();
    }

    public void Dispose()
    {
        _subscription?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

    private void GenerateGrid<T>()
    {
        Columns.Clear();

        var converter = new DataGridLengthConverter();

        foreach (var columnDefinition in ResourceConfig.Columns())
        {
            try
            {
                DataGridColumn column = null;

                if (columnDefinition.CustomControl != null)
                {
                    column = new DataGridTemplateColumn()
                    {
                        Header = columnDefinition.Name,
                        CustomSortComparer = sNoopSortComparer,
                        CanUserSort = true,
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
                        SortDirection = columnDefinition.Sort == SortDirection.None ? null : columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending
                    };
                }
                else
                {
                    // Create Display FuncValueConverter
                    column = new MyDataGridTextColumn
                    {
                        Binding = new Binding()
                        {
                            Converter = new FuncValueConverter<T, string>(x =>
                            {
                                if (x == null)
                                {
                                    return "";
                                }

                                return columnDefinition.DisplayValue(x);
                            }),
                            Mode = BindingMode.OneWay
                        },
                        Header = columnDefinition.Name,
                        CanUserSort = true,
                        CustomSortComparer = sNoopSortComparer,
                        SortDirection = columnDefinition.Sort == SortDirection.None ? null : columnDefinition.Sort == SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending
                    };
                }

                if (!string.IsNullOrEmpty(columnDefinition.Width) && converter.ConvertFromString(columnDefinition.Width) is DataGridLength width)
                {
                    column.Width = width;
                }

                Columns.Add(column);
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
}

public sealed class SortingAdapterFactory<T> : IDataGridSortingAdapterFactory where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ResourceConfigBase<T> _resourceConfig;

    private readonly ObservableCollection<DataGridColumn> _columns;

    private static readonly IComparer<T> s_sNoopComparer = Comparer<T>.Create(static (_, _) => 0);

    private readonly IReadOnlyDictionary<string, IResourceListColumn> _columnsByName;

    private readonly Dictionary<DataGridColumn, Func<T, IComparable?>?> _selectorCache = [];

    public IComparer<T> SortComparer { get; private set; }

    public SortingAdapterFactory(ResourceConfigBase<T> resourceConfig, ObservableCollection<DataGridColumn> columns)
    {
        SortComparer = s_sNoopComparer;
        _resourceConfig = resourceConfig;
        _columns = columns;

        _columnsByName = _resourceConfig
            .Columns()
            .GroupBy(static c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public DataGridSortingAdapter Create(DataGrid grid, ISortingModel model)
    {
        return new DynamicDataSortingAdapter(model, () => _columns, UpdateComparer);
    }

    public void UpdateComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        SortComparer = BuildComparer(descriptors);
    }

    private IComparer<T> BuildComparer(IReadOnlyList<SortingDescriptor> descriptors)
    {
        if (descriptors == null || descriptors.Count == 0)
        {
            return s_sNoopComparer;
        }

        SortExpressionComparer<T>? comparer = null;

        foreach (var descriptor in descriptors.Where(static d => d != null))
        {
            // Depending on SortCycleMode, Avalonia may include descriptors representing "None".
            // Treat default enum value as unsorted and ignore.
            if ((int)descriptor.Direction == 0)
            {
                continue;
            }

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

        return (IComparer<T>?)comparer ?? s_sNoopComparer;
    }

    private Func<T, IComparable?>? CreateSelector(SortingDescriptor descriptor)
    {
        if (descriptor.ColumnId is not DataGridColumn gridColumn)
        {
            return null;
        }

        if (_selectorCache.TryGetValue(gridColumn, out var cached))
        {
            return cached;
        }

        if (gridColumn.Header is not string header || string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        if (!_columnsByName.TryGetValue(header, out var columnDefinition))
        {
            _selectorCache[gridColumn] = null;
            return null;
        }

        // SortExpressionComparer expects a selector; IResourceListColumn exposes SortKey as Func<object, IComparable?>.
        cached = x => columnDefinition.SortKey(x);
        _selectorCache[gridColumn] = cached;
        return cached;
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
