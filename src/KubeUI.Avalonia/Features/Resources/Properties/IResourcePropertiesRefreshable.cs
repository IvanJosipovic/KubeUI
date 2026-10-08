using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Features.Resources.Properties;

/// <summary>Updates a resource properties control without replacing the control.</summary>
/// <typeparam name="T">The Kubernetes resource represented by the properties control.</typeparam>
public interface IResourcePropertiesRefreshable<T>
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    /// <summary>Refreshes displayed resource values while retaining the control instance.</summary>
    /// <param name="resource">The latest resource snapshot.</param>
    void Refresh(T resource);
}
