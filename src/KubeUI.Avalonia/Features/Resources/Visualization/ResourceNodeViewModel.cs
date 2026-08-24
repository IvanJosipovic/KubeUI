using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Visualization;


public sealed partial class ResourceNodeViewModel : ViewModelBase
{
    private static readonly TimeSpan UpdateFlashDuration = TimeSpan.FromMilliseconds(300);
    private long _updateFlashVersion;
    private string? _resourceVersion;

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

    [ObservableProperty]
    public partial IImage Icon { get; set; }

    [ObservableProperty]
    internal partial bool IsUpdated { get; set; }

    public bool IsNotReady => ResourceReadiness.IsNotReady(Resource);

    partial void OnResourceChanged(IKubernetesObject<V1ObjectMeta> value)
    {
        _resourceVersion = value.Metadata?.ResourceVersion;
        OnPropertyChanged(nameof(IsNotReady));
    }

    internal bool HasResourceChanged(IKubernetesObject<V1ObjectMeta> resource)
        => !ReferenceEquals(Resource, resource)
            || !string.Equals(_resourceVersion, resource.Metadata?.ResourceVersion, StringComparison.Ordinal);

    internal void UpdateResource(IKubernetesObject<V1ObjectMeta> resource)
    {
        Resource = resource;
        ShowUpdateFlash();
        OnPropertyChanged(nameof(ContextMenuItems));
    }

    private void ShowUpdateFlash()
    {
        var version = Interlocked.Increment(ref _updateFlashVersion);
        IsUpdated = true;
        DispatcherTimer.RunOnce(
            () =>
            {
                if (version == _updateFlashVersion)
                {
                    IsUpdated = false;
                }
            },
            UpdateFlashDuration,
            DispatcherPriority.Background);
    }

    public IEnumerable<MenuItemViewModel> ContextMenuItems
    {
        get
        {
            if (Cluster == null || Resource == null)
            {
                return [];
            }

            var config = Cluster.GetResourceConfig(Resource);
            return config == null
                ? []
                : ResourceActionPresenter.Compose(config, new[] { Resource });
        }
    }
}
