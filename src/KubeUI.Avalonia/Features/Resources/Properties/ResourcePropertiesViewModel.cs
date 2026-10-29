using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties;

public sealed partial class ResourcePropertiesViewModel<T> : ViewModelBase, IViewModelViewFactory, IClusterWorkspaceContext, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    public GroupApiVersionKind Kind { get; private set; }

    private T? _object;
    private Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? _changeHandler;
    private object? _subscription;
    private long _selectionGeneration;

    public T? Object
    {
        get => _object;
        set
        {
            _object = value;
            OnPropertyChanged();
            RefreshActions();
        }
    }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    public IReadOnlyList<MenuItemViewModel> Actions { get; private set; } = [];

    public ResourcePropertiesViewModel()
    {
        Title = Assets.Resources.ResourcePropertiesView_Title;
        Id = nameof(ResourcePropertiesViewModel<>);
    }

    Control IViewModelViewFactory.CreateView(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<ResourcePropertiesView<T>>(serviceProvider);

    public void Initialize(ClusterWorkspace cluster, T resource)
    {
        ShowResource(cluster, resource);
    }

    /// <summary>
    /// Shows <paramref name="resource"/> in this view, moving the live-update subscription when the cluster changes.
    /// </summary>
    /// <param name="cluster">The cluster workspace that owns the resource.</param>
    /// <param name="resource">The resource to show.</param>
    public void ShowResource(ClusterWorkspace cluster, T resource)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(resource);

        Interlocked.Increment(ref _selectionGeneration);
        if (!ReferenceEquals(Cluster, cluster))
        {
            if (Cluster is not null && _changeHandler is not null)
                Cluster.Runtime.OnChange -= _changeHandler;
            Cluster = cluster;
            var subscription = new object();
            _subscription = subscription;
            _changeHandler = (eventType, kind, changed) => OnClusterChange(cluster, changed, subscription);
            cluster.Runtime.OnChange += _changeHandler;
        }

        Kind = resource.GetResourceKind(cluster.Runtime.ModelCatalog);
        ResourceConfig = cluster.GetResourceConfig<T>(Kind);
        Object = resource;
    }

    partial void OnResourceConfigChanged(ResourceConfigBase<T> value)
    {
        RefreshActions();
    }

    private void RefreshActions()
    {
        Actions = Object == null || ResourceConfig == null
            ? []
            : ResourceActionPresenter.Compose(ResourceConfig, new[] { Object }).ToList();
        OnPropertyChanged(nameof(Actions));
    }

    internal void OnClusterChange(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource)
        => OnClusterChange(cluster, resource, _subscription);

    private void OnClusterChange(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, object? subscription)
    {
        var generation = Interlocked.Read(ref _selectionGeneration);
        if (!ReferenceEquals(_subscription, subscription) || !ReferenceEquals(Cluster, cluster) || !IsShown(resource))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            // The view may have been retargeted (or moved to another cluster) before this runs.
            if (generation == Interlocked.Read(ref _selectionGeneration)
                && ReferenceEquals(_subscription, subscription)
                && ReferenceEquals(Cluster, cluster) && IsShown(resource))
            {
                Object = (T)resource;
            }
        });
    }

    private bool IsShown(IKubernetesObject<V1ObjectMeta> resource)
    {
        return Object != null
            && Object.Kind == resource.Kind
            && Object.ApiVersion == resource.ApiVersion
            && Object.Metadata.Name == resource.Metadata.Name
            && Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty
            && Object.Metadata.Uid == resource.Metadata.Uid;
    }

    public void Dispose()
    {
        if (Cluster is not null && _changeHandler is not null)
            Cluster.Runtime.OnChange -= _changeHandler;
        _changeHandler = null;
        _subscription = null;
        Interlocked.Increment(ref _selectionGeneration);
        Cluster = null;
    }
}
