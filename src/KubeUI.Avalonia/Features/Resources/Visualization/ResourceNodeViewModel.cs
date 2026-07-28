using k8s;
using k8s.Models;
using Avalonia.Media;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;

namespace KubeUI.Avalonia.Features.Resources.Visualization;


public sealed partial class ResourceNodeViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

    [ObservableProperty]
    public partial IImage Icon { get; set; }

    internal void UpdateResource(IKubernetesObject<V1ObjectMeta> resource)
    {
        Resource = resource;
        OnPropertyChanged(nameof(ContextMenuItems));
    }

    public IEnumerable<MenuItemViewModel> ContextMenuItems
    {
        get
        {
            if (Cluster == null || Resource == null)
            {
                return [];
            }

            var config = Cluster.GetResourceConfig(Resource.GetType());
            return config == null
                ? []
                : ResourceActionPresenter.Compose(config, new[] { Resource });
        }
    }
}
