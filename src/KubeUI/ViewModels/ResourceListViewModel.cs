using System.Collections.Specialized;
using System.Linq.Expressions;
using Avalonia.Collections;
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
    public partial AvaloniaDictionary<NamespacedName, T> Objects { get; set; }

    [ObservableProperty]
    public partial KeyValuePair<NamespacedName, T> SelectedItem { get; set; }

    [ObservableProperty]
    public partial ReadOnlyObservableCollection<KeyValuePair<NamespacedName, T>>? DataGridObjects { get; private set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    [ObservableProperty]
    public partial string SortColumnName { get; set; }

    [ObservableProperty]
    public partial Resources.SortDirection SortDirection { get; set; }

    private IDisposable? _filter;

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

        Objects = Cluster.GetObjectDictionary<T>();

        Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;

        SetFilter();
    }

    private void SetFilter()
    {
        _filter?.Dispose();

        _filter = Objects
            .ToObservableChangeSet<AvaloniaDictionary<NamespacedName, T>, KeyValuePair<NamespacedName, T>>()
            .Filter(GenerateFilter())
            .Bind(out var filteredObjects)
            .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Setting Resource List Filter: {ns}", typeof(T)));

        DataGridObjects = filteredObjects;
    }

    private Func<KeyValuePair<NamespacedName, T>, bool> GenerateFilter()
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

            return Expression.Lambda<Func<KeyValuePair<NamespacedName, T>, bool>>(body, param).Compile();
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
        _filter?.Dispose();

        Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }
}
