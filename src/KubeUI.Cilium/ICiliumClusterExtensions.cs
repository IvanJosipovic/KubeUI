using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Cilium;

public static class CiliumClusterExtensions
{
    public static async Task<bool> IsCiliumInstalledAsync(this IClusterRuntime cluster, CancellationToken cancellationToken = default)
    {
        try
        {
            await cluster.SeedResource<V1CustomResourceDefinition>(waitForReady: true, cancellationToken)
                .ConfigureAwait(false);
            return HasCiliumCustomResourceDefinition(cluster.GetResourceList<V1CustomResourceDefinition>());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsCiliumInstalled(this IClusterRuntime cluster)
    {
        try
        {
            return HasCiliumCustomResourceDefinition(cluster.GetResourceList<V1CustomResourceDefinition>());
        }
        catch
        {
            return false;
        }
    }

    private static bool HasCiliumCustomResourceDefinition(IEnumerable<V1CustomResourceDefinition> crds)
        => crds.Any(crd => string.Equals(crd.Spec?.Group, "cilium.io", StringComparison.OrdinalIgnoreCase));
}
