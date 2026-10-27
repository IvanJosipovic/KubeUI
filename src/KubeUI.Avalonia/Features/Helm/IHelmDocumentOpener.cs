using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Opens Helm documents for charts, releases, and repositories.</summary>
public interface IHelmDocumentOpener
{
    /// <summary>Opens an install editor, or an upgrade editor when a release is supplied.</summary>
    /// <param name="cluster">Workspace owning the editor.</param>
    /// <param name="connection">Helm connection the editor operates on.</param>
    /// <param name="release">Release to upgrade, or <see langword="null"/> to install a new release.</param>
    void OpenReleaseEditor(ClusterWorkspace cluster, HelmConnection connection, HelmRelease? release);

    /// <summary>Opens an install editor prefilled from a selected indexed chart.</summary>
    /// <param name="cluster">Workspace owning the editor.</param>
    /// <param name="connection">Helm connection the editor operates on.</param>
    /// <param name="chart">Chart selected in the chart catalog.</param>
    Task OpenReleaseEditorForChartAsync(ClusterWorkspace cluster, HelmConnection connection, HelmChartSearchResult chart);

    /// <summary>Opens a repository editor, or an edit form when a repository is supplied.</summary>
    /// <param name="cluster">Workspace owning the editor.</param>
    /// <param name="connection">Helm connection the editor operates on.</param>
    /// <param name="repository">Repository to edit, or <see langword="null"/> to add a new repository.</param>
    void OpenRepositoryEditor(ClusterWorkspace cluster, HelmConnection connection, HelmRepository? repository);

    /// <summary>Opens or focuses the cluster chart list, optionally filtered to a repository.</summary>
    /// <param name="cluster">Workspace owning the chart document.</param>
    /// <param name="repositoryName">Repository to filter by, or <see langword="null"/> to keep the current filter.</param>
    void OpenCharts(ClusterWorkspace cluster, string? repositoryName = null);
}
