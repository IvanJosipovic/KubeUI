using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.Contracts;

namespace KubeUI.Features.Resources.Visualization;

/// <summary>Coordinates visualization documents for consuming features.</summary>
internal sealed class ResourceVisualizationService : IResourceVisualizationService
{
    private readonly Func<VisualizationViewModel> _createViewModel;
    private readonly IFactory _factory;

    public ResourceVisualizationService(Func<VisualizationViewModel> createViewModel, IFactory factory)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public void OpenScope(ClusterWorkspace cluster, VisualizationScope scope)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(scope);
        var viewModel = _createViewModel();
        viewModel.InitializeScope(cluster, scope);
        _factory.AddToDocuments(viewModel);
    }

    public void OpenPreview(ClusterWorkspace cluster, string releaseName, IEnumerable<IKubernetesObject<V1ObjectMeta>> resources)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentNullException.ThrowIfNull(resources);
        var viewModel = _createViewModel();
        viewModel.InitializePreview(cluster, releaseName, resources);
        _factory.AddToDocuments(viewModel);
    }
}
