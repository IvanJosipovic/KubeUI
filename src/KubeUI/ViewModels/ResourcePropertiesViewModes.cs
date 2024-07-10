using k8s;
using k8s.Models;
using KubeUI.Assets;
using KubeUI.Client;

namespace KubeUI.ViewModels;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DemoResourcePropertiesViewModel : ResourcePropertiesViewModel<V1Pod>
{
    public DemoResourcePropertiesViewModel()
    {
        Object = new V1Pod()
        {
            Metadata = new()
            {
                Name = "Demo",
                NamespaceProperty = "NS"
            }
        };
    }
}

public partial class ResourcePropertiesViewModel<T> : ViewModelBase, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    private Cluster? _cluster;

    [ObservableProperty]
    private GroupApiVersionKind? _kind;

    [ObservableProperty]
    private T? _object;

    public string TypeName => Kind.ToString();

    public ResourcePropertiesViewModel()
    {
        Title = Resources.ResourcePropertiesViewModel_Title;
        Id = "Properties";
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Cluster))
        {
            Cluster.OnChange += Cluster_OnChange;
        }
    }

    private async void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Object != null
            && Object.Kind == resource.Kind
            && Object.ApiVersion == resource.ApiVersion
            && Object.Metadata.Name == resource.Metadata.Name
            && Object.Metadata.NamespaceProperty == resource.Metadata.NamespaceProperty)
        {
            await Dispatcher.UIThread.InvokeAsync(() => Object = (T)resource);
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
