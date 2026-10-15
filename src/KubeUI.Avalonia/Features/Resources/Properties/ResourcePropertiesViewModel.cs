using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Properties;

public sealed partial class ResourcePropertiesViewModel<T> : ViewModelBase, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    public GroupApiVersionKind Kind { get; private set; }

    private T? _object;

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

        if (!ReferenceEquals(Cluster, cluster))
        {
            Cluster?.Runtime.OnChange -= Cluster_OnChange;
            Cluster = cluster;
            Cluster.Runtime.OnChange += Cluster_OnChange;
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

    public void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Object != null
            && Object.Kind == resource.Kind
            && Object.ApiVersion == resource.ApiVersion
            && Object.Metadata.Name == resource.Metadata.Name
            && Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty)
        {
            Dispatcher.UIThread.Post(() => Object = (T)resource);
        }
    }

    public void Dispose()
    {
        Cluster?.Runtime.OnChange -= Cluster_OnChange;
        Cluster = null;
    }
}
