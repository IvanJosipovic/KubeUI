using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources;

internal sealed class CustomResourceConfigFactory(IServiceProvider serviceProvider) : IClusterCustomResourceConfigFactory
{
    public IClusterCustomResourceConfig Create(ApiDiscoveryResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return CustomResourceConfig.Select(resource, serviceProvider.GetServices<CustomResourceConfig>());
    }
}
