using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Settings;

public sealed partial class ClusterSettings : ObservableObject
{
    public const string DefaultDebugContainerImage = "docker.io/library/busybox:latest";

    [ObservableProperty]
    public partial ObservableCollection<string>? Namespaces { get; set; } = [];

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = DefaultDebugContainerImage;

    /// <summary>Gets or sets whether the cluster is shown in navigation.</summary>
    [ObservableProperty]
    public partial bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the KubeUI-only display name. The kubeconfig context name is used when empty.</summary>
    [ObservableProperty]
    public partial string? Alias { get; set; }

    [ObservableProperty]
    public partial ClusterMetricsSettings MetricsSettings { get; set; } = new();
}
