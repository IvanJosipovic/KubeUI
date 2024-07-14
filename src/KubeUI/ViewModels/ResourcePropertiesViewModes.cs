using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DemoResourcePropertiesViewModel : ResourcePropertiesViewModel<V1Deployment>
{
    public DemoResourcePropertiesViewModel()
    {
        Object = new V1Deployment()
        {
            Metadata = new()
            {
                Name = "Demo",
                NamespaceProperty = "NS",
                Labels = new Dictionary<string,string>()
                {
                    { "app", "demo" },
                },
                Annotations = new Dictionary<string, string>()
                {
                    { "ann1", "demo" },
                },
                CreationTimestamp =  DateTime.Now,
            },
        };
    }
}

public partial class ResourcePropertiesViewModel<T> : ViewModelBase, IDisposable where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    private Cluster? _cluster;

    public GroupApiVersionKind Kind { get; } = GroupApiVersionKind.From<T>();

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
