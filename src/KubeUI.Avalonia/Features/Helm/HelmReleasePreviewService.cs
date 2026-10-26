using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Helm;

/// <summary>Opens live release graphs or isolated Helm template previews without applying resources.</summary>
public sealed class HelmReleasePreviewService : IHelmReleasePreviewService
{
    private readonly IHelmService _helmService;
    private readonly Func<VisualizationViewModel> _createVisualization;
    private readonly IFactory _factory;

    /// <summary>Creates the Helm release preview service.</summary>
    /// <param name="helmService">The Helm client abstraction used for chart rendering.</param>
    /// <param name="createVisualization">Creates the visualization document model.</param>
    /// <param name="factory">Application dock factory used to open visualization documents.</param>
    public HelmReleasePreviewService(
        IHelmService helmService,
        Func<VisualizationViewModel> createVisualization,
        IFactory factory)
    {
        ArgumentNullException.ThrowIfNull(helmService);
        ArgumentNullException.ThrowIfNull(createVisualization);
        ArgumentNullException.ThrowIfNull(factory);
        _helmService = helmService;
        _createVisualization = createVisualization;
        _factory = factory;
    }

    /// <inheritdoc />
    public async Task PreviewVersionAsync(ClusterWorkspace cluster, HelmConnection connection, string releaseName,
        string chart, string version, IReadOnlyDictionary<string, object?>? values = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        var manifest = await _helmService.TemplateVersionAsync(connection, releaseName, chart, version, values, cancellationToken);
        OpenPreview(cluster, releaseName, manifest);
    }

    /// <inheritdoc />
    public void VisualizeRelease(ClusterWorkspace cluster, string releaseName, string releaseNamespace)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseNamespace);
        var viewModel = _createVisualization();
        viewModel.InitializeScope(cluster, new VisualizationScope(
            releaseNamespace,
            string.Format(Assets.Resources.HelmReleasePreviewService_ReleaseTitle!, releaseName, releaseNamespace),
            new ResourceAnnotationSelector(new Dictionary<string, string>
            {
                ["meta.helm.sh/release-name"] = releaseName,
                ["meta.helm.sh/release-namespace"] = releaseNamespace
            }),
            IsNamespaceReadOnly: true));
        _factory.AddToDocuments(viewModel);
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
        OpenPreview(cluster, releaseName, manifest);
    }

    private void OpenPreview(ClusterWorkspace cluster, string releaseName, string manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest))
        {
            throw new InvalidOperationException(Assets.Resources.HelmReleasePreviewService_NoResources!);
        }

        var parsed = Kubernetes.Serialization.KubernetesYaml.LoadAllFromString(
            manifest,
            key =>
            {
                var separator = key.LastIndexOf('/');
                var apiVersion = separator < 0 ? key : key[..separator];
                var kind = separator < 0 ? string.Empty : key[(separator + 1)..];
                return cluster.Runtime.ModelCatalog.TryGetResourceKind(apiVersion, kind, out var resourceKind)
                    && cluster.Runtime.ModelCatalog.TryGetResourceType(resourceKind, out var resourceType)
                    ? resourceType
                    : typeof(GenericKubernetesObject);
            });
        var resources = parsed
            .Cast<IKubernetesObject<V1ObjectMeta>>()
            .ToArray();
        if (resources.Length == 0)
        {
            throw new InvalidOperationException(Assets.Resources.HelmReleasePreviewService_NoResources!);
        }

        var viewModel = _createVisualization();
        viewModel.InitializePreview(cluster, releaseName, resources);
        _factory.AddToDocuments(viewModel);
    }
}
