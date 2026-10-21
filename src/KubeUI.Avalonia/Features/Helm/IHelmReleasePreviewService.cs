using KubeUI.Kubernetes;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Renders Helm charts into Kubernetes resources for read-only visualization.</summary>
public interface IHelmReleasePreviewService
{
    /// <summary>Renders a Helm chart and opens its Kubernetes resources in the read-only visualization view.</summary>
    /// <param name="cluster">Cluster context for the visualization document.</param>
    /// <param name="connection">Kubernetes connection and namespace used for rendering.</param>
    /// <param name="releaseName">Release name used in Helm templates.</param>
    /// <param name="chart">Helm chart reference.</param>
    /// <param name="values">Optional Helm values.</param>
    /// <param name="cancellationToken">Cancels waiting for the native Helm operation.</param>
    /// <returns>A task that completes after the preview document is opened. Resources are not applied or seeded.</returns>
    Task PreviewAsync(
        ClusterWorkspace cluster,
        HelmConnection connection,
        string releaseName,
        string chart,
        IReadOnlyDictionary<string, object?>? values = null,
        CancellationToken cancellationToken = default);
}
