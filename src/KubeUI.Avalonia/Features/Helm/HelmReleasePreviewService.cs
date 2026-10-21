using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Renders and parses Helm template output without sending it to a Kubernetes cluster.</summary>
public sealed class HelmReleasePreviewService : IHelmReleasePreviewService
{
    private readonly IHelmService _helmService;
    private readonly Func<VisualizationViewModel> _createVisualization;
    private readonly IHelmVisualizationDocumentOpener _documentOpener;

    /// <summary>Creates the Helm release preview service.</summary>
    /// <param name="helmService">The Helm client abstraction used for chart rendering.</param>
    public HelmReleasePreviewService(
        IHelmService helmService,
        Func<VisualizationViewModel> createVisualization,
        IHelmVisualizationDocumentOpener documentOpener)
    {
        ArgumentNullException.ThrowIfNull(helmService);
        ArgumentNullException.ThrowIfNull(createVisualization);
        ArgumentNullException.ThrowIfNull(documentOpener);
        _helmService = helmService;
        _createVisualization = createVisualization;
        _documentOpener = documentOpener;
    }

    /// <inheritdoc />
    public async Task PreviewAsync(
        ClusterWorkspace cluster,
        HelmConnection connection,
        string releaseName,
        string chart,
        IReadOnlyDictionary<string, object?>? values = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        var manifest = await _helmService.TemplateAsync(connection, releaseName, chart, values, cancellationToken);
        if (string.IsNullOrWhiteSpace(manifest))
        {
            throw new InvalidOperationException(Assets.Resources.HelmReleasePreviewService_NoResources!);
        }

        var parsed = KubeUI.Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(
            manifest,
            static _ => typeof(GenericKubernetesObject));
        var resources = parsed.OfType<GenericKubernetesObject>()
            .Cast<IKubernetesObject<V1ObjectMeta>>()
            .ToArray();
        if (resources.Length == 0)
        {
            throw new InvalidOperationException(Assets.Resources.HelmReleasePreviewService_NoResources!);
        }

        var viewModel = _createVisualization();
        viewModel.InitializePreview(cluster, releaseName, resources);
        _documentOpener.Open(viewModel);
    }
}
