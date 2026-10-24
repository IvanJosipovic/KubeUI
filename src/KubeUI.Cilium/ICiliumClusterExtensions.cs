using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Cilium;

public static class CiliumClusterExtensions
{
    public static async Task<bool> IsCiliumInstalledAsync(this IClusterRuntime cluster, CancellationToken _ = default)
    {
        try
        {
            // Check for Cilium CRDs
            var crds = cluster.GetResourceList<V1CustomResourceDefinition>();
            return crds.Any(crd => crd.Name()?.StartsWith("cilium", StringComparison.OrdinalIgnoreCase) == true);
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
            var crds = cluster.GetResourceList<V1CustomResourceDefinition>();
            return crds.Any(crd => crd.Name()?.StartsWith("cilium", StringComparison.OrdinalIgnoreCase) == true);
        }
        catch
        {
            return false;
        }
    }
}