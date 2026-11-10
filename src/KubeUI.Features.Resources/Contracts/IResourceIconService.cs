using KubernetesClient.Informer.Client;

namespace KubeUI.Features.Resources.Contracts;

/// <summary>Provides icons for Kubernetes resource kinds to consuming features.</summary>
public interface IResourceIconService
{
    /// <summary>Gets the icon for a resource kind.</summary>
    /// <param name="resourceKind">Kubernetes resource kind.</param>
    /// <returns>The matching icon.</returns>
    IImage GetIcon(GroupApiVersionKind resourceKind);
}
