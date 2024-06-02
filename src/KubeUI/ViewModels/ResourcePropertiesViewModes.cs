using System.ComponentModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using k8s;
using k8s.Models;
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

public partial class ResourcePropertiesViewModel<T> : ViewModelBase where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    [ObservableProperty]
    private Cluster _cluster;

    [ObservableProperty]
    private GroupApiVersionKind _kind;

    [ObservableProperty]
    private T _object;

    public string TypeName => Kind.ToString();

    public ResourcePropertiesViewModel()
    {
        Title = "Properties";
        Id = "Properties";
    }
}
