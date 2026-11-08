using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Helm.Helm;
using KubeUI.Features.Resources.Contracts;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Features.Helm;

/// <summary>Opens live release graphs or isolated Helm template previews without applying resources.</summary>
public sealed class HelmReleasePreviewService : IHelmReleasePreviewService
{
    private readonly IKubernetesYamlSerializer _yamlSerializer;
    private readonly IHelmService _helmService;
    private readonly IResourceVisualizationService _visualizationService;

    /// <summary>Creates the Helm release preview service.</summary>
    /// <param name="yamlSerializer">Application serializer for preview manifests.</param>
    /// <param name="helmService">The Helm client abstraction used for chart rendering.</param>
    /// <param name="visualizationService">Resources feature service that opens visualization documents.</param>
    public HelmReleasePreviewService(
        IHelmService helmService,
        IKubernetesYamlSerializer yamlSerializer,
        IResourceVisualizationService visualizationService)
    {
        ArgumentNullException.ThrowIfNull(helmService);
        ArgumentNullException.ThrowIfNull(visualizationService);
        _helmService = helmService;
        _yamlSerializer = yamlSerializer;
        _visualizationService = visualizationService;
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
        _visualizationService.OpenScope(cluster, new VisualizationScope(
            releaseNamespace,
            string.Format(HelmLanguageManager.Instance.HelmReleasePreviewService_ReleaseTitle.CurrentValue()!, releaseName, releaseNamespace),
            new ResourceAnnotationSelector(new Dictionary<string, string>
            {
                ["meta.helm.sh/release-name"] = releaseName,
                ["meta.helm.sh/release-namespace"] = releaseNamespace
            }),
            IsNamespaceReadOnly: true));
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
            throw new InvalidOperationException(HelmLanguageManager.Instance.HelmReleasePreviewService_NoResources.CurrentValue()!);
        }

        var parsed = _yamlSerializer.LoadAllFromString(
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
            throw new InvalidOperationException(HelmLanguageManager.Instance.HelmReleasePreviewService_NoResources.CurrentValue()!);
        }

        _visualizationService.OpenPreview(cluster, releaseName, resources);
    }
}
