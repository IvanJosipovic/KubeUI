using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

/// <summary>
/// Identifies a Kubernetes resource by namespace and name without allocating a combined string key.
/// </summary>
/// <param name="Namespace">The resource namespace, or <see langword="null"/> for cluster-scoped resources.</param>
/// <param name="Name">The resource name.</param>
public readonly record struct ResourceCacheKey(string? Namespace, string Name)
{
    /// <summary>
    /// Creates a cache key from a Kubernetes resource's namespace and name.
    /// </summary>
    /// <param name="resource">The resource to identify.</param>
    /// <returns>An allocation-free namespace/name cache key.</returns>
    public static ResourceCacheKey From(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new(resource.Metadata?.NamespaceProperty, resource.Metadata?.Name ?? string.Empty);
    }
}
