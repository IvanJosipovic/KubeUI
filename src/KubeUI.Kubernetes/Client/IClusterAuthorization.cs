using System.Diagnostics.CodeAnalysis;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>Provides cached and live Kubernetes authorization checks.</summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public API contract; renaming a parameter is a breaking change. Revisit in next major version.")]
public interface IClusterAuthorization
{
    /// <summary>Raised after the full authorization cache has been discarded.</summary>
    event Action? PermissionsInvalidated;
    /// <summary>Discards cached authorization decisions and rule sets.</summary>
    void InvalidatePermissions();
    /// <summary>Discards namespace-scoped authorization decisions and rule sets while retaining cluster-wide decisions.</summary>
    void InvalidateNamespacePermissions();
    /// <summary>Returns cached permission for a resource and optional namespace/subresource.</summary>
    /// <param name="kind">
    /// The resource kind (e.g., V1Pod) used to build review keys.
    /// </param>
    /// <param name="verb">
    /// The verb (e.g., List, Watch) to check permissions for.
    /// </param>
    /// <param name="namespace">
    /// Optional namespace filter; when empty, checks cluster-wide decisions.
    /// </param>
    /// <param name="subresource">
    /// Optional subresource (e.g., pods/status); defaults to "*" if omitted.
    /// </param>
    /// <param name="skipIncompleteRulesFallback">
    /// When <see langword="true"/>, the incomplete-rules fail-open fallback is skipped and
    /// the method returns <see langword="false"/> if no exact decision is cached. Used by
    /// informer-scope selection so that incomplete rules cannot authorize informer creation.
    /// </param>
    bool CanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null, bool skipIncompleteRulesFallback = false);
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
    /// <summary>
    /// Ensures the connect-time authorization snapshot: one coarse probe and, when needed,
    /// one rules review per known namespace. Implementations that do not own a client
    /// can keep the default no-op.
    /// </summary>
    Task RefreshAuthorizationSnapshotAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
