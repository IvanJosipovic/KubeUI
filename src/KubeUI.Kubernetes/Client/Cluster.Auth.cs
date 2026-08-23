using System.Collections.Concurrent;
using System.Diagnostics;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Runtime authorization engine and permission index for a cluster.
/// Owns Kubernetes authorization calls and cached permission truth.
/// </summary>
public partial class Cluster
{
    private const int MaximumConcurrentNamespaceAuthorizationReviews = 8;
    private readonly ConcurrentDictionary<string, bool> _permissionIndex = new();

    private static string BuildReviewKeyCore(
        string? group,
        string pluralName,
        string version,
        string verbString,
        string? @namespace = null,
        string? subresource = null)
    {
        return $"{verbString}:{(string.IsNullOrEmpty(group) ? "" : group)}:{pluralName}:{(string.IsNullOrEmpty(@namespace) ? "" : @namespace)}:{(string.IsNullOrEmpty(subresource) ? "" : subresource)}:{version}";
    }

    private static string BuildReviewKey(GroupApiVersionKind kind, string verbString, string? @namespace = null, string? subresource = null)
    {
        return BuildReviewKeyCore(
            kind.Group,
            kind.PluralName,
            kind.ApiVersion,
            verbString,
            @namespace,
            subresource);
    }

    [ObservableProperty]
    public partial bool ListNamespaces { get; set; }

    private async Task UpdateNamespacePermission()
    {
        using var activity = StartClusterActivity(nameof(UpdateNamespacePermission));
        await UpdateCanI<V1Namespace>(Verb.List).ConfigureAwait(false);
        await UpdateCanI<V1Namespace>(Verb.Watch).ConfigureAwait(false);

        ListNamespaces = CanI<V1Namespace>(Verb.List) && CanI<V1Namespace>(Verb.Watch);
    }

    private void SetPermissionResult(GroupApiVersionKind kind, string verbString, string? @namespace, string? subresource, bool allowed)
    {
        var key = BuildReviewKey(kind, verbString, @namespace, subresource);
        _permissionIndex[key] = allowed;
    }

    private void ResetAuthorizationIndex()
    {
        _permissionIndex.Clear();
    }

    private static void SetAuthorizationActivityTags(Activity? activity, GroupApiVersionKind kind, Verb verb, string? @namespace, string? subresource)
    {
        activity?.SetTag("kubernetes.resource.group", kind.Group);
        activity?.SetTag("kubernetes.resource.version", kind.ApiVersion);
        activity?.SetTag("kubernetes.resource.kind", kind.Kind);
        activity?.SetTag("kubernetes.authorization.verb", verb.ToString());
        activity?.SetTag("kubernetes.namespace", @namespace);
        activity?.SetTag("kubernetes.subresource", subresource);
    }

    private IEnumerable<string> GetKnownNamespaceNames()
    {
        if (Namespaces.Count > 0)
        {
            return Namespaces.Select(static item => item.Name()).Where(static name => !string.IsNullOrWhiteSpace(name));
        }

        return _settings.GetClusterNamespaces(this).Where(static name => !string.IsNullOrWhiteSpace(name));
    }

    private async Task<bool> BuildPermissionAsync(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(BuildPermissionAsync));

        var verbString = verb.ToString().ToLowerInvariant();
        var keyCheck = BuildReviewKey(kind, verbString, @namespace, subresource);
        if (_permissionIndex.TryGetValue(keyCheck, out var cached))
        {
            return cached;
        }

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = (string.IsNullOrEmpty(kind.Group) ? "" : kind.Group),
                    NamespaceProperty = (string.IsNullOrEmpty(@namespace) ? "" : @namespace),
                    Resource = kind.PluralName,
                    Subresource = (string.IsNullOrEmpty(subresource) ? "" : subresource),
                    Verb = verb.ToString().ToLowerInvariant(),
                    Version = kind.ApiVersion
                }
            }
        };

        var resp = await Client.AuthorizationV1.CreateSelfSubjectAccessReviewAsync(model);
        var allowed = resp.Status?.Allowed == true;
        SetPermissionResult(kind, verbString, @namespace, subresource, allowed);
        return allowed;
    }

    public bool CanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        var verbString = verb.ToString().ToLowerInvariant();
        if (!string.IsNullOrEmpty(@namespace)
            && _permissionIndex.TryGetValue(BuildReviewKey(kind, verbString, null, subresource), out var globalAllowed)
            && globalAllowed)
        {
            return true;
        }

        return _permissionIndex.TryGetValue(BuildReviewKey(kind, verbString, @namespace, subresource), out var allowed) && allowed;
    }

    public bool CanIAnyNamespace(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null)
    {
        return CanI(kind, verb, subresource: subresource)
            || (namespaced && GetKnownNamespaceNames().Any(@namespace => CanI(kind, verb, @namespace, subresource)));
    }

    public async Task UpdatePermissionsAllNamespaceAsync(GroupApiVersionKind kind, bool namespaced, Verb verb, string? subresource = null)
    {
        await BuildPermissionAsync(kind, verb, subresource: subresource).ConfigureAwait(false);
        if (!namespaced || CanI(kind, verb, subresource: subresource))
        {
            return;
        }

        await Parallel.ForEachAsync(GetKnownNamespaceNames(),
            new ParallelOptions { MaxDegreeOfParallelism = MaximumConcurrentNamespaceAuthorizationReviews },
            async (namespaceName, _) => await BuildPermissionAsync(kind, verb, namespaceName, subresource).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(CanI));
        var kind = GroupApiVersionKind.From<T>();
        SetAuthorizationActivityTags(activity, kind, verb, @namespace, subresource);

        return CanI(kind, verb, @namespace, subresource);
    }

    public async Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(UpdateCanI));
        var kind = GroupApiVersionKind.From<T>();
        SetAuthorizationActivityTags(activity, kind, verb, @namespace, subresource);

        return await BuildPermissionAsync(kind, verb, @namespace, subresource).ConfigureAwait(false);
    }

    public async Task<bool> UpdateCanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(UpdateCanI));
        SetAuthorizationActivityTags(activity, kind, verb, @namespace, subresource);
        return await BuildPermissionAsync(kind, verb, @namespace, subresource).ConfigureAwait(false);
    }

    public async Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(UpdatePermissionsAllNamespaceAsync));
        var kind = GroupApiVersionKind.From<T>();
        SetAuthorizationActivityTags(activity, kind, verb, null, subresource);

        var globallyAllowed = await UpdateCanI<T>(verb, subresource: subresource).ConfigureAwait(false);
        if (globallyAllowed || !IsResourceNamespaced(kind))
        {
            return;
        }

        await Parallel.ForEachAsync(
            GetKnownNamespaceNames(),
            new ParallelOptions { MaxDegreeOfParallelism = MaximumConcurrentNamespaceAuthorizationReviews },
            async (@namespace, _) => await UpdateCanI<T>(verb, @namespace, subresource).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(CanIAnyNamespace));
        var kind = GroupApiVersionKind.From<T>();
        SetAuthorizationActivityTags(activity, kind, verb, null, subresource);

        return CanIAnyNamespace(kind, IsResourceNamespaced(kind), verb, subresource);
    }
}
