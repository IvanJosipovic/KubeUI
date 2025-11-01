using k8s;
using k8s.Models;
using KubeUI.Client;
using KubeUI.Resources;
using Yarp.Kubernetes.Controller.Client;

namespace KubeUI.ViewModels;

public partial class ResourcePropertiesViewModel<T> : ViewModelBase, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    public partial ICluster? Cluster { get; set; }

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

    [ObservableProperty]
    public partial T? Object { get; set; }

    [ObservableProperty]
    public partial ResourceConfigBase<T> ResourceConfig { get; set; }

    public ResourcePropertiesViewModel()
    {
        Title = Assets.Resources.ResourcePropertiesViewModel_Title;
        Id = nameof(ResourcePropertiesViewModel<>);
    }

    public void Initialize(ICluster cluster, T resource)
    {
        Cluster = cluster;
        Object = resource;
        ResourceConfig = (ResourceConfigBase<T>)Cluster.GetResourceConfig(Kind);
        Cluster.OnChange += Cluster_OnChange;

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
        Cluster?.OnChange -= Cluster_OnChange;
    }
}
