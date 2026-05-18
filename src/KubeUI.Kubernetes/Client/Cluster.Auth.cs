using System.Collections.Concurrent;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private readonly ConcurrentDictionary<string, bool> _permissionIndex = new();
    private readonly ConcurrentDictionary<AuthorizationRequest, byte> _authorizationManifest = new();
    private readonly ConcurrentDictionary<string, long> _authorizationNamespaceVersions = new(StringComparer.Ordinal);
    private long _authorizationManifestVersion;
    private long _authorizationManifestSequence;

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

    [ObservableProperty]
    public partial bool AuthorizationIndexReady { get; set; }

    [ObservableProperty]
    public partial long AuthorizationIndexVersion { get; set; }

    private async Task UpdateNamespacePermission()
    {
        await UpdatePermissionsAllNamespaceAsync<V1Namespace>(Verb.List);
        await UpdatePermissionsAllNamespaceAsync<V1Namespace>(Verb.Watch);

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
        _authorizationManifest.Clear();
        _authorizationNamespaceVersions.Clear();
        Interlocked.Exchange(ref _authorizationManifestVersion, 0);
        AuthorizationIndexReady = false;
    }

    private bool SynchronizeAuthorizationManifest(IReadOnlyCollection<AuthorizationRequest> manifest)
    {
        var changed = false;
        foreach (var request in manifest)
        {
            if (_authorizationManifest.TryAdd(request, 0))
            {
                changed = true;
            }
        }

        if (changed)
        {
            Interlocked.Exchange(ref _authorizationManifestVersion, Interlocked.Increment(ref _authorizationManifestSequence));
        }

        return changed;
    }

    private async Task<bool> GetSelfSubjectAccessReview(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
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

    private async Task RefreshNamespaceAuthorizationIndexAsync(string @namespace, IReadOnlyCollection<AuthorizationRequest> requests, long manifestVersion)
    {
        var namespacedRequests = requests
            .Where(static request => request.ResourceType != null)
            .Where(request => IsResourceNamespaced(request.ResourceType))
            .Distinct()
            .ToArray();

        if (namespacedRequests.Length == 0 || string.IsNullOrWhiteSpace(@namespace))
        {
            return;
        }

        if (_authorizationNamespaceVersions.TryGetValue(@namespace, out var indexedVersion) && indexedVersion == manifestVersion)
        {
            return;
        }

        var model = new V1SelfSubjectRulesReview()
        {
            ApiVersion = $"{V1SelfSubjectRulesReview.KubeGroup}/{V1SelfSubjectRulesReview.KubeApiVersion}",
            Kind = V1SelfSubjectRulesReview.KubeKind,
            Spec = new V1SelfSubjectRulesReviewSpec
            {
                NamespaceProperty = @namespace
            }
        };

        var review = await Client.AuthorizationV1.CreateSelfSubjectRulesReviewAsync(model);
        if (review.Status?.Incomplete == true || !string.IsNullOrWhiteSpace(review.Status?.EvaluationError))
        {
            _logger.LogWarning(
                "SelfSubjectRulesReview for namespace '{Namespace}' was incomplete. Falling back to SelfSubjectAccessReview.",
                @namespace);

            foreach (var resourceType in namespacedRequests)
            {
                await GetSelfSubjectAccessReview(resourceType.ResourceType, resourceType.Verb, @namespace, resourceType.Subresource).ConfigureAwait(false);
            }

            _authorizationNamespaceVersions[@namespace] = manifestVersion;
            return;
        }

        var resourceRules = review.Status?.ResourceRules ?? [];
        foreach (var request in namespacedRequests)
        {
            var kind = GroupApiVersionKind.From(request.ResourceType);
            var verbString = request.Verb.ToString().ToLowerInvariant();
            SetPermissionResult(kind, verbString, @namespace, request.Subresource, IsRuleAllowed(resourceRules, kind, verbString, request.Subresource));
        }

        _authorizationNamespaceVersions[@namespace] = manifestVersion;
    }

    private static bool IsRuleAllowed(IEnumerable<V1ResourceRule> resourceRules, GroupApiVersionKind kind, string verbString, string? subresource = null)
    {
        var group = kind.Group ?? string.Empty;
        var resource = kind.PluralName;
        var resourceWithSubresource = string.IsNullOrWhiteSpace(subresource)
            ? resource
            : $"{resource}/{subresource}";

        foreach (var rule in resourceRules)
        {
            if (!Matches(rule.Verbs, verbString) || !Matches(rule.ApiGroups, group))
            {
                continue;
            }

            if (Matches(rule.Resources, resourceWithSubresource))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Matches(IEnumerable<string>? values, string value)
    {
        if (values == null)
        {
            return false;
        }

        foreach (var item in values)
        {
            if (string.Equals(item, "*", StringComparison.Ordinal) || string.Equals(item, value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public async Task RefreshAuthorizationIndexAsync(IEnumerable<AuthorizationRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var manifest = requests
            .Where(static request => request.ResourceType != null)
            .Distinct()
            .ToArray();

        AuthorizationIndexReady = false;
        SynchronizeAuthorizationManifest(manifest);

        if (manifest.Length == 0)
        {
            AuthorizationIndexReady = true;
            AuthorizationIndexVersion++;
            return;
        }

        foreach (var request in manifest)
        {
            var kind = GroupApiVersionKind.From(request.ResourceType);
            var verbString = request.Verb.ToString().ToLowerInvariant();
            var key = BuildReviewKey(kind, verbString, subresource: request.Subresource);
            if (_permissionIndex.ContainsKey(key))
            {
                continue;
            }

            await GetSelfSubjectAccessReview(request.ResourceType, request.Verb, subresource: request.Subresource).ConfigureAwait(false);
        }

        var namespaces = GetEffectivePermissionNamespaces();
        var manifestVersion = Volatile.Read(ref _authorizationManifestVersion);
        var namespaceRefreshTasks = namespaces
            .Select(@namespace => RefreshNamespaceAuthorizationIndexAsync(@namespace, manifest, manifestVersion))
            .ToArray();

        await Task.WhenAll(namespaceRefreshTasks).ConfigureAwait(false);
        AuthorizationIndexVersion++;
        AuthorizationIndexReady = true;
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
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
            if (!AuthorizationIndexReady)
            {
                _logger.LogDebug(
                    "Authorization key is not indexed yet for {Verb} {Group}/{Resource}/{Subresource} namespace '{Namespace}'. Returning false while authorization index is still building.",
                    verb,
                    kind.Group,
                    kind.PluralName,
                    subresource,
                    @namespace);
                return false;
            }

            _logger.LogError(
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
        return CanI(typeof(T), verb, @namespace, subresource);
    }

    public async Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return await GetSelfSubjectAccessReview(type, verb, @namespace, subresource);
    }

    public async Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await UpdateCanI(typeof(T), verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        if (CanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (IsResourceNamespaced(type))
        {
            foreach (var @namespace in GetEffectivePermissionNamespaces())
            {
                if (CanI(type, verb, @namespace, subresource))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanIAnyNamespace(typeof(T), verb, subresource);
    }

    public async Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        if (await UpdateCanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (IsResourceNamespaced(type))
        {
            foreach (var @namespace in GetEffectivePermissionNamespaces())
            {
                if (await UpdateCanI(type, verb, @namespace, subresource))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await UpdateCanIAnyNamespaceAsync(typeof(T), verb, subresource);
    }

    public async Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        await GetSelfSubjectAccessReview(type, verb, subresource: subresource);

        if (IsResourceNamespaced(type))
        {
            var tasks = new List<Task>();

            foreach (var @namespace in GetEffectivePermissionNamespaces())
            {
                tasks.Add(GetSelfSubjectAccessReview(type, verb, @namespace, subresource));
            }

            await Task.WhenAll(tasks);
        }
    }

    public async Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await UpdatePermissionsAllNamespaceAsync(typeof(T), verb, subresource);
    }

    private IReadOnlyCollection<string> GetEffectivePermissionNamespaces()
    {
        var namespaces = new HashSet<string>(StringComparer.Ordinal);

        if (Objects.ContainsKey(GroupApiVersionKind.From<V1Namespace>()))
        {
            foreach (var item in GetResourceList<V1Namespace>())
            {
                var name = item.Name();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    namespaces.Add(name);
                }
            }
        }

        if (!ListNamespaces)
        {
            foreach (var configuredNamespace in _settings.GetClusterNamespaces(this))
            {
                if (!string.IsNullOrWhiteSpace(configuredNamespace))
                {
                    namespaces.Add(configuredNamespace);
                }
            }
        }

        return namespaces.ToArray();
    }
}
