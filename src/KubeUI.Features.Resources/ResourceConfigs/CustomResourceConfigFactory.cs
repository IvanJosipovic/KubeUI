using k8s.Models;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Resources;

internal sealed class CustomResourceConfigFactory(IServiceProvider serviceProvider) : IClusterCustomResourceConfigFactory
{
    public IClusterCustomResourceConfig Create(V1CustomResourceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return CustomResourceConfig.Select(definition, serviceProvider.GetServices<CustomResourceConfig>());
    }
}
