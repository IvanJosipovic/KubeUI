using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Opens live Helm release visualization or renders charts into isolated resource previews.</summary>
public interface IHelmReleasePreviewService
{
    /// <summary>Renders the selected chart version with edited values in an isolated visualization.</summary>
    Task PreviewVersionAsync(ClusterWorkspace cluster, HelmConnection connection, string releaseName,
        string chart, string version, IReadOnlyDictionary<string, object?>? values = null,
        CancellationToken cancellationToken = default);

    /// <summary>Opens live cluster visualization scoped to the release ownership annotations.</summary>
    /// <param name="cluster">Cluster containing the release.</param>
    /// <param name="releaseName">Release name.</param>
    /// <param name="releaseNamespace">Release namespace.</param>
    void VisualizeRelease(ClusterWorkspace cluster, string releaseName, string releaseNamespace);

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
