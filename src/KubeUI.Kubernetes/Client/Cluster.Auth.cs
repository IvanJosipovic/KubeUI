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
    private static readonly NamespaceRuleSet IncompleteNamespaceRules = new([], IsComplete: false);
    private AuthorizationCacheState _authorizationCache = new();

    /// <inheritdoc />
    public event Action? PermissionsInvalidated;

    private readonly record struct PermissionCacheKey(
        string Verb,
        string? Group,
        string Resource,
        string? Namespace,
        string? Subresource);

    private sealed record NamespaceRuleSet(IReadOnlyList<V1ResourceRule> ResourceRules, bool IsComplete);

    /// <summary>
    /// One generation of authorization caches. Invalidation replaces the whole state rather than
    /// clearing dictionaries, so in-flight loads can only publish into the generation they started in.
    /// </summary>
    private sealed class AuthorizationCacheState
    {
        public ConcurrentDictionary<PermissionCacheKey, bool> PermissionIndex { get; } = new();

        public ConcurrentDictionary<string, NamespaceRuleSet> NamespaceRules { get; } = new(StringComparer.Ordinal);

        public ConcurrentDictionary<string, Lazy<Task<NamespaceRuleSet>>> NamespaceRuleLoads { get; } = new(StringComparer.Ordinal);
    }

    private static PermissionCacheKey BuildReviewKey(
        GroupApiVersionKind kind,
        string verbString,
        string? @namespace = null,
        string? subresource = null)
    {
        return new PermissionCacheKey(
            verbString,
            string.IsNullOrEmpty(kind.Group) ? null : kind.Group,
            kind.PluralName,
            string.IsNullOrEmpty(@namespace) ? null : @namespace,
            string.IsNullOrEmpty(subresource) ? null : subresource);
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

    private static void SetPermissionResult(
        AuthorizationCacheState cache,
        GroupApiVersionKind kind,
        string verbString,
        string? @namespace,
        string? subresource,
        bool allowed)
    {
        var key = BuildReviewKey(kind, verbString, @namespace, subresource);
        cache.PermissionIndex[key] = allowed;
    }

    private void ResetAuthorizationIndex()
    {
        Interlocked.Exchange(ref _authorizationCache, new AuthorizationCacheState());
    }

    /// <inheritdoc />
    public void InvalidatePermissions()
    {
        ResetAuthorizationIndex();
        PermissionsInvalidated?.Invoke();
    }

    /// <inheritdoc />
    public void InvalidateNamespacePermissions()
    {
        while (true)
        {
            var current = Volatile.Read(ref _authorizationCache);
            var replacement = new AuthorizationCacheState();
            foreach (var pair in current.PermissionIndex)
            {
                if (string.IsNullOrEmpty(pair.Key.Namespace))
                {
                    replacement.PermissionIndex[pair.Key] = pair.Value;
                }
            }

            if (ReferenceEquals(
                    Interlocked.CompareExchange(ref _authorizationCache, replacement, current),
                    current))
            {
                return;
            }
        }
    }

    private void InvalidatePermissionsAfterForbiddenResponse()
    {
        if (!Connected || Client is null)
        {
            return;
        }

        InvalidatePermissions();
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

    private async Task<bool> BuildPermissionAsync(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(BuildPermissionAsync));

        var verbString = verb.ToString().ToLowerInvariant();
        var cache = Volatile.Read(ref _authorizationCache);
        var keyCheck = BuildReviewKey(kind, verbString, @namespace, subresource);
        if (cache.PermissionIndex.TryGetValue(keyCheck, out var cached))
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
        SetPermissionResult(cache, kind, verbString, @namespace, subresource, allowed);

        return allowed;
    }

    private Task<NamespaceRuleSet> GetNamespaceRulesAsync(string @namespace, CancellationToken cancellationToken)
    {
        var cache = Volatile.Read(ref _authorizationCache);
        if (cache.NamespaceRules.TryGetValue(@namespace, out var cached))
        {
            return Task.FromResult(cached);
        }

        var lazy = cache.NamespaceRuleLoads.GetOrAdd(
            @namespace,
            (namespaceName) => new Lazy<Task<NamespaceRuleSet>>(
                () => LoadNamespaceRulesAsync(namespaceName, cache),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return AwaitNamespaceRulesAsync(cache, @namespace, lazy, cancellationToken);
    }

    private static async Task<NamespaceRuleSet> AwaitNamespaceRulesAsync(
        AuthorizationCacheState cache,
        string @namespace,
        Lazy<Task<NamespaceRuleSet>> lazy,
        CancellationToken cancellationToken)
    {
        var rules = await lazy.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        cache.NamespaceRules.TryAdd(@namespace, rules);
        return rules;
    }

    private async Task<NamespaceRuleSet> LoadNamespaceRulesAsync(string @namespace, AuthorizationCacheState cache)
    {
        using var activity = StartClusterActivity(nameof(LoadNamespaceRulesAsync));
        activity?.SetTag("kubernetes.namespace", @namespace);

        try
        {
            var client = Client;
            if (client is null)
            {
                return IncompleteNamespaceRules;
            }

            var review = new V1SelfSubjectRulesReview
            {
                ApiVersion = V1SelfSubjectRulesReview.KubeGroup + "/" + V1SelfSubjectRulesReview.KubeApiVersion,
                Kind = V1SelfSubjectRulesReview.KubeKind,
                Spec = new V1SelfSubjectRulesReviewSpec
                {
                    NamespaceProperty = @namespace,
                },
            };

            var response = await client.AuthorizationV1
                .CreateSelfSubjectRulesReviewAsync(review)
                .ConfigureAwait(false);
            var status = response.Status;
            var isComplete = status is not null
                && status.Incomplete != true
                && string.IsNullOrEmpty(status.EvaluationError);
            var result = new NamespaceRuleSet(
                status?.ResourceRules?.ToArray() ?? [],
                isComplete);

            cache.NamespaceRules.TryAdd(@namespace, result);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to load Kubernetes authorization rules for namespace {Namespace}", @namespace);
            return IncompleteNamespaceRules;
        }
    }

    private static bool RuleSetAllows(
        NamespaceRuleSet ruleSet,
        GroupApiVersionKind kind,
        string verbString,
        string? subresource)
    {
        foreach (var rule in ruleSet.ResourceRules)
        {
            if (RuleAllows(rule, kind, verbString, subresource))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RuleAllows(
        V1ResourceRule rule,
        GroupApiVersionKind kind,
        string verbString,
        string? subresource)
    {
        if (!RuleValueMatches(rule.Verbs, verbString)
            || !RuleValueMatches(rule.ApiGroups, kind.Group ?? string.Empty))
        {
            return false;
        }

        var requestedResource = string.IsNullOrEmpty(subresource)
            ? kind.PluralName
            : kind.PluralName + "/" + subresource;
        if (!ResourceMatches(rule.Resources, requestedResource))
        {
            return false;
        }

        if (rule.ResourceNames is { Count: > 0 } && IsNameSpecificVerb(verbString))
        {
            // resourceNames cannot be evaluated without a concrete object name. Kubernetes
            // ignores them for list, watch, create, and deletecollection, so only
            // name-specific verbs are denied when no object name is supplied.
            return false;
        }

        return true;
    }

    private static bool IsNameSpecificVerb(string verbString)
        => verbString is "delete" or "get" or "patch" or "update";

    private static bool RuleValueMatches(IList<string>? values, string value)
    {
        if (values is null)
        {
            return false;
        }

        foreach (var item in values)
        {
            if (item == "*" || string.Equals(item, value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ResourceMatches(IList<string>? ruleResources, string requestedResource)
    {
        if (ruleResources is null)
        {
            return false;
        }

        foreach (var ruleResource in ruleResources)
        {
            if (ruleResource == "*"
                || string.Equals(ruleResource, requestedResource, StringComparison.Ordinal)
                || ResourceSegmentsMatch(ruleResource, requestedResource))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ResourceSegmentsMatch(string ruleResource, string requestedResource)
    {
        var ruleSeparator = ruleResource.IndexOf('/');
        var requestedSeparator = requestedResource.IndexOf('/');

        if (ruleSeparator < 0 || requestedSeparator < 0)
        {
            return false;
        }

        if (!string.Equals(ruleResource[..ruleSeparator], requestedResource[..requestedSeparator], StringComparison.Ordinal)
            && ruleResource[..ruleSeparator] != "*")
        {
            return false;
        }

        return string.Equals(
            ruleResource[(ruleSeparator + 1)..],
            requestedResource[(requestedSeparator + 1)..],
            StringComparison.Ordinal);
    }

    public bool CanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        var verbString = verb.ToString().ToLowerInvariant();
        var cache = Volatile.Read(ref _authorizationCache);
        if (!string.IsNullOrEmpty(@namespace)
            && cache.PermissionIndex.TryGetValue(BuildReviewKey(kind, verbString, null, subresource), out var globalAllowed)
            && globalAllowed)
        {
            return true;
        }

        if (cache.PermissionIndex.TryGetValue(BuildReviewKey(kind, verbString, @namespace, subresource), out var allowed))
        {
            return allowed;
        }

        if (!string.IsNullOrEmpty(@namespace)
            && cache.NamespaceRules.TryGetValue(@namespace, out var namespaceRules)
            && namespaceRules.IsComplete)
        {
            var ruleAllowed = RuleSetAllows(namespaceRules, kind, verbString, subresource);
            SetPermissionResult(cache, kind, verbString, @namespace, subresource, ruleAllowed);
            return ruleAllowed;
        }

        return false;
    }

    private IReadOnlyCollection<string> GetKnownNamespaceNames()
    {
        if (Objects.TryGetValue(GroupApiVersionKind.From<V1Namespace>(), out var container)
            && container is ContainerClass<V1Namespace> namespaceContainer)
        {
            var cachedNamespaces = namespaceContainer.Items.Items
                .Select(static item => item.Name())
                .Where(static name => !string.IsNullOrWhiteSpace(name))
                .ToArray();
            if (cachedNamespaces.Length > 0)
            {
                return cachedNamespaces;
            }
        }

        return _settings.GetClusterNamespaces(this);
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
            async (namespaceName, cancellationToken) =>
            {
                var rules = await GetNamespaceRulesAsync(namespaceName, cancellationToken).ConfigureAwait(false);
                if (!rules.IsComplete)
                {
                    // Incomplete rule reviews cannot answer synchronously, so keep the exact
                    // resource/verb decision available in the permission index.
                    await BuildPermissionAsync(kind, verb, namespaceName, subresource).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
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

        return await UpdateCanI(kind, verb, @namespace, subresource).ConfigureAwait(false);
    }

    public async Task<bool> UpdateCanI(GroupApiVersionKind kind, Verb verb, string? @namespace = null, string? subresource = null)
    {
        using var activity = StartClusterActivity(nameof(UpdateCanI));
        SetAuthorizationActivityTags(activity, kind, verb, @namespace, subresource);

        if (string.IsNullOrEmpty(@namespace))
        {
            return await BuildPermissionAsync(kind, verb, subresource: subresource).ConfigureAwait(false);
        }

        var cache = Volatile.Read(ref _authorizationCache);
        var rules = await GetNamespaceRulesAsync(@namespace, CancellationToken.None).ConfigureAwait(false);
        if (rules.IsComplete)
        {
            var verbString = verb.ToString().ToLowerInvariant();
            var allowed = RuleSetAllows(rules, kind, verbString, subresource);
            SetPermissionResult(cache, kind, verbString, @namespace, subresource, allowed);
            return allowed;
        }

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
