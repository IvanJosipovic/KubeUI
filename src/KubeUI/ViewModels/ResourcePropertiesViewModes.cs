using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public partial class ResourcePropertiesViewModel<T> : ViewModelBase, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    private ICluster? _cluster;

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

    [ObservableProperty]
    private T? _object;

    public ResourcePropertiesViewModel()
    {
        Title = Resources.ResourcePropertiesViewModel_Title;
        Id = nameof(ResourcePropertiesViewModel<T>);
    }

    public void Initialize(ICluster cluster, T resource)
    {
        Cluster = cluster;
        Object = resource;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Cluster))
        {
            Cluster.OnChange += Cluster_OnChange;
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
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
        if (Cluster != null)
        {
            Cluster.OnChange -= Cluster_OnChange;
        }
    }
}
