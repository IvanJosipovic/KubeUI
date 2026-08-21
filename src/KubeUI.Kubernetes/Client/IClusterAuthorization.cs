using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public interface IClusterAuthorization
{
    bool CanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null);
    bool CanIAnyNamespace(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null);
    Task UpdatePermissionsAllNamespaceAsync(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null);
    Task<bool> UpdateCanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null);
    bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
}
