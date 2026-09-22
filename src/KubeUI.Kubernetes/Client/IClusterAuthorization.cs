using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>Provides cached and live Kubernetes authorization checks.</summary>
public interface IClusterAuthorization
{
    /// <summary>Returns cached permission for a resource and optional namespace/subresource.</summary>
    bool CanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null);
    /// <summary>Returns whether permission exists in any namespace, or cluster-wide when <paramref name="namespaced"/> is false.</summary>
    bool CanIAnyNamespace(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null);
    /// <summary>Refreshes cached permission for all namespaces or cluster scope.</summary>
    Task UpdatePermissionsAllNamespaceAsync(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null);
    /// <summary>Performs a live authorization check and updates cache.</summary>
    Task<bool> UpdateCanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null);
    /// <summary>Returns cached permission for typed resource.</summary>
    bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    /// <summary>Returns cached permission for typed resource in any namespace.</summary>
    bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    /// <summary>Performs live authorization check for typed resource.</summary>
    Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
    /// <summary>Refreshes typed-resource permission for all namespaces.</summary>
    Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new();
}
