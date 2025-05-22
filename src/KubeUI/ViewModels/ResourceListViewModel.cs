using System.Collections.Specialized;
using System.Linq.Expressions;
using Avalonia.Collections;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Experimental.Data;
using Avalonia.Styling;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Client.Informer;
using KubeUI.Resources;

namespace KubeUI.ViewModels;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly ILogger<ResourceListViewModel<T>> _logger;

    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial GroupApiVersionKind Kind { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<T> Objects { get; set; }

    //[ObservableProperty]
    //public partial ReadOnlyObservableCollection<T>? DataGridObjects { get; private set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    [ObservableProperty]
    public partial FlatTreeDataGridSource<T> Source { get; set; }

    //private IDisposable? _filter;

    public ResourceListViewModel()
    {
        _logger = Application.Current.GetRequiredService<ILogger<ResourceListViewModel<T>>>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Kind = GroupApiVersionKind.From<T>();
        Title = Kind.Kind.Humanize(LetterCasing.Title).Pluralize();
        Id = Cluster.Name + "-" + Kind;
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);

        Objects = Cluster.GetObjects<T>();

        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        SetFilter();

        Source = new FlatTreeDataGridSource<T>(Objects);

        foreach (var column in ResourceConfig.Columns())
        {
            // Expression<Func<T, TValue>>
            var columnField = column.GetType().GetProperty(nameof(ResourceListColumn<T, string>.FieldExpression)).GetValue(column);
            var col = typeof(TextColumn<,>).MakeGenericType(typeof(T), column.Type);

            var gridLength = GridLength.Parse(column.Width);

            var opts = new TextColumnOptions<T>
            {
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                IsTextSearchEnabled = false
            };

            var colDefinition = Activator.CreateInstance(col, [column.Name, columnField, gridLength, opts]) as IColumn<T>;

            Source.Columns.Add(colDefinition);

            if (column.Sort != Resources.SortDirection.None)
            {
                ((ITreeDataGridSource)Source).SortBy(colDefinition, column.Sort == Resources.SortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            }
        }
    }

    private void SetFilter()
    {
        //_filter?.Dispose();

        //_filter = Objects
        //    .ToObservableChangeSet<ObservableCollection<T>, T>()
        //    //.Filter(GenerateFilter())
        //    .Bind(out var filteredObjects)
        //    .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Setting Resource List Filter: {ns}", typeof(T)));

        //DataGridObjects = filteredObjects;
    }

    private Func<T, bool> GenerateFilter()
    {
        try
        {
            var param = Expression.Parameter(typeof(KeyValuePair<NamespacedName, T>), "p");

            var key = Expression.PropertyOrField(param, "Key");

            var value = Expression.PropertyOrField(param, "Value");

            BinaryExpression? body = null;

            BinaryExpression? namespaceFilter = null;

            BinaryExpression? searchFilter = null;

            if (Cluster.SelectedNamespaces != null && ResourceConfig.IsNamespaced)
            {
                foreach (var item in Cluster.SelectedNamespaces)
                {
                    var expression = Expression.Equal(
                            Expression.PropertyOrField(key, "Namespace"),
                            Expression.Constant(item.Name())
                       );

                    namespaceFilter = namespaceFilter == null ? expression : Expression.OrElse(namespaceFilter, expression);
                }
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var method = typeof(string).GetMethod(nameof(string.IndexOf), [typeof(string), typeof(StringComparison)]);

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

                        var columnDisplay = (Func<T, string>)colType.GetProperty(nameof(ResourceListColumn<V1Pod, string>.Display)).GetValue(column);

                        columnDisplay ??= (Func<T, string>)colType.GetProperty(nameof(ResourceListColumn<V1Pod, string>.Field)).GetValue(column);

                        var funcCall = Expression.Call(Expression.Constant(columnDisplay), columnDisplay.GetType().GetMethod("Invoke"), value);

                        var expression = Expression.GreaterThanOrEqual(Expression.Call(funcCall, method, someValue, Expression.Constant(StringComparison.OrdinalIgnoreCase)), Expression.Constant(0));

                        wordFilter = wordFilter == null ? expression : Expression.OrElse(wordFilter, expression);
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
                body = Expression.Equal(Expression.Constant(true), Expression.Constant(true));
            }

            return Expression.Lambda<Func<T, bool>>(body).Compile();
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
            SetFilter();
        }
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetFilter();
    }

    public void Dispose()
    {
        //_filter?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }
}
