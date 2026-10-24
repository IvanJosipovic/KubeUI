using Dock.Model.Core;
using Helm.NET;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Helm.Charts;
using KubeUI.Avalonia.Features.Helm.Releases;
using KubeUI.Avalonia.Features.Helm.Repositories;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Creates Helm documents through dependency injection and adds them to the document dock.</summary>
public sealed class HelmDocumentOpener : IHelmDocumentOpener
{
    private readonly Func<HelmChartsViewModel> _createCharts;
    private readonly Func<HelmReleaseEditorViewModel> _createReleaseEditor;
    private readonly Func<HelmRepositoryEditorViewModel> _createRepositoryEditor;
    private readonly IFactory _factory;

    /// <summary>Creates a Helm document opener.</summary>
    /// <param name="createCharts">Creates the chart list document.</param>
    /// <param name="createReleaseEditor">Creates a release install/upgrade editor.</param>
    /// <param name="createRepositoryEditor">Creates a repository editor.</param>
    /// <param name="factory">Application dock factory.</param>
    public HelmDocumentOpener(
        Func<HelmChartsViewModel> createCharts,
        Func<HelmReleaseEditorViewModel> createReleaseEditor,
        Func<HelmRepositoryEditorViewModel> createRepositoryEditor,
        IFactory factory)
    {
        ArgumentNullException.ThrowIfNull(createCharts);
        ArgumentNullException.ThrowIfNull(createReleaseEditor);
        ArgumentNullException.ThrowIfNull(createRepositoryEditor);
        ArgumentNullException.ThrowIfNull(factory);

        _createCharts = createCharts;
        _createReleaseEditor = createReleaseEditor;
        _createRepositoryEditor = createRepositoryEditor;
        _factory = factory;
    }

    /// <inheritdoc />
    public void OpenReleaseEditor(ClusterWorkspace cluster, HelmConnection connection, HelmRelease? release)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(connection);

        var editor = _createReleaseEditor();
        editor.Initialize(cluster, connection, release);
        _factory.AddToDocuments(editor);
    }

    /// <inheritdoc />
    public async Task OpenReleaseEditorForChartAsync(ClusterWorkspace cluster, HelmConnection connection, HelmChartSearchResult chart)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(chart);

        var editor = _createReleaseEditor();
        editor.Initialize(cluster, connection, null);
        await editor.SelectInstallChartAsync(chart);
        _factory.AddToDocuments(editor);
    }

    /// <inheritdoc />
    public void OpenRepositoryEditor(ClusterWorkspace cluster, HelmConnection connection, HelmRepository? repository)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(connection);

        var editor = _createRepositoryEditor();
        editor.Initialize(cluster, connection, repository);
        _factory.AddToDocuments(editor);
    }

    /// <inheritdoc />
    public void OpenCharts(ClusterWorkspace cluster, string? repositoryName = null)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        var charts = _createCharts();
        charts.Initialize(cluster);
        _factory.AddOrUpdateToDocuments(
            charts,
            repositoryName is null ? null : viewModel => viewModel.SelectRepository(repositoryName));
    }
}
