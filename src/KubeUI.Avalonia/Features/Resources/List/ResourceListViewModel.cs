using System.Collections.Specialized;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using DynamicData;
using Humanizer;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using SortDirection = KubeUI.Avalonia.Resources.SortDirection;

namespace KubeUI.Avalonia.Features.Resources.List;

public partial class ResourceListViewModel<T> : ViewModelBase, IInitializeCluster, IDisposable, IResourceListViewModel where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    internal const string NamespaceScopeFilterId = "__namespace_scope__";
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResourceListViewModel<T>> _logger;
    private readonly IAgentContextService _agentContextService;
    private GroupApiVersionKind _kind;


    [ObservableProperty]
    public partial ClusterWorkspace Cluster { get; set; }

    public GroupApiVersionKind Kind => _kind;

    [ObservableProperty]
    public partial ISourceCache<T, ResourceCacheKey> Objects { get; set; }

    public T? SelectedItem => _tableSource?.SelectionModel.SelectedItem as T;

    public IReadOnlyList<T>? SelectedItems => _tableSource?.SelectionModel.SelectedItems.Cast<T>().ToArray();

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    IResourceConfig IResourceListViewModel.ResourceConfig => ResourceConfig;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial Exception? LoadError { get; set; }

    private DynamicTableViewSource<T, ResourceCacheKey>? _tableSource;
    private INotifyCollectionChanged? _itemsNotifications;
    private ResourceCacheKey[]? _selectionRuntimeState;

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new InvalidOperationException("Resource list source has not been initialized.");

    public ISelectionModel SelectionModel => TableSource.SelectionModel;

    public DynamicTableViewState? TableViewRuntimeState { get; set; }

    /// <summary>Stores selected resource keys so a recreated list view can restore visible selections.</summary>
    public void CaptureSelectionState()
    {
        if (_tableSource is null)
            return;

        var selectedItems = SelectionModel.SelectedItems;
        var selectedKeys = new ResourceCacheKey[selectedItems.Count];
        for (var index = 0; index < selectedItems.Count; index++)
            selectedKeys[index] = ResourceCacheKey.From((T)selectedItems[index]!);
        _selectionRuntimeState = selectedKeys;
    }

    /// <summary>Restores captured selections for resources that remain visible in the table source.</summary>
    public void RestoreSelectionState()
    {
        if (_tableSource is null || _selectionRuntimeState is not { } savedKeys)
            return;

        var selectedKeys = new HashSet<ResourceCacheKey>(savedKeys);
        var selection = _tableSource.SelectionModel;
        var desiredIndexes = new List<int>();
        var index = 0;
        foreach (var item in _tableSource.Items)
        {
            if (item is T resource && selectedKeys.Contains(ResourceCacheKey.From(resource)))
                desiredIndexes.Add(index);
            index++;
        }

        if (selection.SelectedIndexes.Count == desiredIndexes.Count &&
            selection.SelectedIndexes.SequenceEqual(desiredIndexes))
            return;

        using (selection.BatchUpdate())
        {
            selection.Clear();
            foreach (var selectedIndex in desiredIndexes)
                selection.Select(selectedIndex);
        }
    }

    [ObservableProperty]
    public partial int ItemCount { get; set; }

    [ObservableProperty]
    public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    private IList<IResourceListColumn> _resourceColumns = [];
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public ResourceListViewModel(
        IServiceProvider serviceProvider,
        ILogger<ResourceListViewModel<T>> logger,
        IAgentContextService agentContextService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _agentContextService = agentContextService;

    }

    public void Initialize(ClusterWorkspace cluster)
    {
        if (typeof(T) == typeof(GenericKubernetesObject))
        {
            throw new InvalidOperationException("Generic resource lists must be initialized with a resource GVK.");
        }

        InitializeResource(cluster, GroupApiVersionKind.From<T>());
    }

    public void InitializeResource(ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        UnsubscribeFromSelectedNamespaces();
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged -= ItemsOnCollectionChanged;
            _itemsNotifications = null;
        }
        if (_tableSource is not null)
        {
            _tableSource.SourceError -= TableSourceOnError;
            _tableSource.SelectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
            _tableSource.Dispose();
            _tableSource = null;
        }

        Cluster = cluster;
        _kind = kind;
        ResourceConfig = Cluster.GetResourceConfig<T>(kind);
        Title = kind.Kind.Humanize(LetterCasing.Title).Pluralize();
        Id = Cluster.Runtime.Name + "-" + kind;
        var seedTask = ResourceConfig.SeedResource();
        _resourceColumns = ResourceConfig.Columns();
        var columns = new List<DynamicTableViewColumn<T>>(_resourceColumns.Count);
        var sorts = new List<DynamicTableViewSortDescriptor>();
        foreach (var column in _resourceColumns)
        {
            try
            {
                var cellTemplate = column.CustomControl is null ? null : CreateCellTemplate(column);
                var dynamicColumn = column.CreateDynamicTableViewColumn(cellTemplate);
                if (dynamicColumn is not DynamicTableViewColumn<T> typedColumn)
                    throw new InvalidOperationException($"Column '{column.Key}' has an incompatible row type.");
                columns.Add(typedColumn);
                if (column.Sort != SortDirection.None)
                {
                    sorts.Add(new(column.Key, column.Sort == SortDirection.Ascending
                        ? ListSortDirection.Ascending
                        : ListSortDirection.Descending));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to generate resource column {ColumnKey}", column.Key);
            }
        }

        Objects = Cluster.Runtime.GetResourceSourceCache<T>(ResourceConfig.Kind);
        _tableSource = new DynamicTableViewSource<T, ResourceCacheKey>(
            Objects,
            ResourceCacheKey.From,
            columns,
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
        _tableSource.SourceError += TableSourceOnError;
        _tableSource.SelectionModel.SelectionChanged += SelectionModelOnSelectionChanged;
        _tableSource.SetSort(sorts.ToArray());
        _tableSource.SearchText = SearchQuery;
        SetNamespaceFilter();
        SubscribeToItems();
        OnPropertyChanged(nameof(TableSource));
        OnPropertyChanged(nameof(SelectionModel));

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

        if (e.PropertyName == nameof(SearchQuery) && _tableSource is not null)
            _tableSource.SearchText = SearchQuery;
    }

    public IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? selectedItems)
    {
        if (ResourceConfig == null)
        {
            return [];
        }

        return ResourceActionPresenter.Compose(ResourceConfig, selectedItems);
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

        if (_tableSource is null)
            return;
        if (!ResourceConfig.IsNamespaced || SelectedNamespaces.Count == 0)
        {
            _tableSource.SetScopeFilter(NamespaceScopeFilterId, null);
            return;
        }

        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var selectedNamespace in SelectedNamespaces)
        {
            if (selectedNamespace.Name() is { Length: > 0 } name)
                namespaces.Add(name);
        }

        _tableSource.SetScopeFilter(NamespaceScopeFilterId,
            item => item is T resource && namespaces.Contains(resource.Namespace() ?? string.Empty));
    }

    public void Dispose()
    {
        _agentContextService.ClearContext(this);
        if (_tableSource is not null)
        {
            _tableSource.SourceError -= TableSourceOnError;
            _tableSource.SelectionModel.SelectionChanged -= SelectionModelOnSelectionChanged;
            _tableSource.Dispose();
            _tableSource = null;
        }
        if (_itemsNotifications is not null)
        {
            _itemsNotifications.CollectionChanged -= ItemsOnCollectionChanged;
            _itemsNotifications = null;
        }
        UnsubscribeFromSelectedNamespaces();
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
            _logger.LogError(ex, "Error loading resource list for {Kind}", Kind);

            await Dispatcher.UIThread.InvokeAsync(() => LoadError = ex);
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = false);
        }
    }

    private void BindObjects()
    {
        Objects = Cluster.Runtime.GetResourceSourceCache<T>(ResourceConfig.Kind);
        SubscribeToSelectedNamespaces();
        SetNamespaceFilter();
        SubscribeToItems();
    }

    private void SubscribeToSelectedNamespaces()
    {
        UnsubscribeFromSelectedNamespaces();
        SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
    }

    private void UnsubscribeFromSelectedNamespaces()
    {
        Cluster?.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;

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

    private FuncDataTemplate<T> CreateCellTemplate(IResourceListColumn columnDefinition)
    {
        return new FuncDataTemplate<T>((_, _) =>
        {
            try
            {
                var control = _serviceProvider.GetRequiredService(columnDefinition.CustomControl!) as Control
                    ?? throw new InvalidOperationException($"Unable to resolve control type {columnDefinition.CustomControl!.FullName}");
                if (control is IInitializeCluster initializeCluster)
                    initializeCluster.Initialize(Cluster);
                return control;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resource cell control for {ColumnKey}", columnDefinition.Key);
                return new TextBlock { Text = ex.Message };
            }
        }, supportsRecycling: true);
    }

    private void SubscribeToItems()
    {
        if (_itemsNotifications is not null)
            _itemsNotifications.CollectionChanged -= ItemsOnCollectionChanged;
        _itemsNotifications = _tableSource?.Items as INotifyCollectionChanged;
        if (_itemsNotifications is not null)
            _itemsNotifications.CollectionChanged += ItemsOnCollectionChanged;
        UpdateItemCount();
    }

    private void ItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateItemCount();

    private void UpdateItemCount()
    {
        if (_tableSource?.Items is ICollection items)
            ItemCount = items.Count;
    }

    private void TableSourceOnError(object? sender, Exception exception)
        => _logger.LogError(exception, "Error updating resource list for {Kind}", Kind);

    private void SelectionModelOnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedItem));
        OnPropertyChanged(nameof(SelectedItems));
        var selectedItems = SelectedItems?.Where(static selected => selected is not null).ToArray();
        _agentContextService.SetContext(this, selectedItems is not { Length: > 0 }
            ? null
            : new AgentContext
            {
                Namespace = selectedItems[0].Metadata?.NamespaceProperty,
                SelectedResources = selectedItems
                    .Select(selected => new KubernetesResourceReference(
                        Kind.GroupApiVersion,
                        Kind.Kind,
                        selected.Metadata?.Name ?? string.Empty,
                        selected.Metadata?.NamespaceProperty))
                    .ToArray()
            });
    }


}
