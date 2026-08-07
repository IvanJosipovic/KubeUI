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

    private static void SetAuthorizationActivityTags(Activity? activity, Type type, Verb verb, string? @namespace, string? subresource)
    {
        activity?.SetTag("kubernetes.resource.type", type.Name);
        activity?.SetTag("kubernetes.authorization.verb", verb.ToString());
        activity?.SetTag("kubernetes.namespace", @namespace);
        activity?.SetTag("kubernetes.subresource", subresource);
    }

    private async Task<bool> BuildPermissionAsync(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(BuildPermissionAsync));
        SetAuthorizationActivityTags(activity, type, verb, @namespace, subresource);

        var kind = GroupApiVersionKind.From(type);
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

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(CanI));
        SetAuthorizationActivityTags(activity, type, verb, @namespace, subresource);

        var kind = GroupApiVersionKind.From(type);
        var verbString = verb.ToString().ToLowerInvariant();

        // If checking namespace permissions, check index for cluster-level first
        if (!string.IsNullOrEmpty(@namespace))
        {
            var globalKey = BuildReviewKey(kind, verbString, null, subresource);
            if (_permissionIndex.TryGetValue(globalKey, out var globalAllowed) && globalAllowed)
            {
                return true;
            }
        }

        var key = BuildReviewKey(kind, verbString, @namespace, subresource);
        if (!_permissionIndex.TryGetValue(key, out var allowed))
        {
            _logger.LogDebug(
                "Authorization key was not indexed for {Verb} {Group}/{Resource}/{Subresource} namespace '{Namespace}'. Returning false.",
                verb,
                kind.Group,
                kind.PluralName,
                subresource,
                @namespace);
            return false;
        }

        return allowed;
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(CanI));
        SetAuthorizationActivityTags(activity, typeof(T), verb, @namespace, subresource);

        return CanI(typeof(T), verb, @namespace, subresource);
    }

    public async Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(UpdateCanI));
        SetAuthorizationActivityTags(activity, type, verb, @namespace, subresource);

        return await BuildPermissionAsync(type, verb, @namespace, subresource).ConfigureAwait(false);
    }

    public async Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(UpdateCanI));
        SetAuthorizationActivityTags(activity, typeof(T), verb, @namespace, subresource);

        return await UpdateCanI(typeof(T), verb, @namespace, subresource).ConfigureAwait(false);
    }

    public async Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(UpdatePermissionsAllNamespaceAsync));
        SetAuthorizationActivityTags(activity, type, verb, null, subresource);

        ArgumentNullException.ThrowIfNull(type);

        var globallyAllowed = await UpdateCanI(type, verb, subresource: subresource).ConfigureAwait(false);
        if (globallyAllowed || !IsResourceNamespaced(type))
        {
            return;
        }

        await Parallel.ForEachAsync(
            Namespaces
                .Select(static item => item.Name())
                .Where(static name => !string.IsNullOrWhiteSpace(name))
                .ToArray(),
            new ParallelOptions { MaxDegreeOfParallelism = MaximumConcurrentNamespaceAuthorizationReviews },
            async (@namespace, _) => await UpdateCanI(type, verb, @namespace, subresource).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public async Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(UpdatePermissionsAllNamespaceAsync));
        SetAuthorizationActivityTags(activity, typeof(T), verb, null, subresource);

        await UpdatePermissionsAllNamespaceAsync(typeof(T), verb, subresource).ConfigureAwait(false);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(CanIAnyNamespace));
        SetAuthorizationActivityTags(activity, type, verb, null, subresource);

        if (CanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (!IsResourceNamespaced(type))
        {
            return false;
        }

        if (Namespaces is null)
        {
            return false;
        }

        foreach (var item in Namespaces)
        {
            var @namespace = item.Name();
            if (!string.IsNullOrWhiteSpace(@namespace)
                && CanI(type, verb, @namespace, subresource))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(CanIAnyNamespace));
        SetAuthorizationActivityTags(activity, typeof(T), verb, null, subresource);

        return CanIAnyNamespace(typeof(T), verb, subresource);
    }
}
