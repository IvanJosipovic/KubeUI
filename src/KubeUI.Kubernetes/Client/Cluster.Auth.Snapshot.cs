using System.Diagnostics;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Connect-time authorization snapshot: one coarse probe for wildcard administrators,
/// otherwise one rules review per known namespace. Those rules answer every verb and
/// subresource locally, so navigation and action checks add no Kubernetes requests.
/// </summary>
public partial class Cluster
{
    private const int MaximumConcurrentNamespaceRulesReviews = 5;

    /// <summary>
    /// Ensures the connect-time authorization snapshot is available. For a wildcard
    /// administrator no namespace rules are needed; otherwise one rules review per
    /// known namespace is loaded with bounded concurrency.
    /// </summary>
    /// <inheritdoc />
    public async Task RefreshAuthorizationSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (await EnsureWildcardAdminAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        // Two coarse probes replace one global access review per kind for list and watch.
        await Task.WhenAll(
            EnsureWildcardVerbAsync(Verb.List, cancellationToken),
            EnsureWildcardVerbAsync(Verb.Watch, cancellationToken)).ConfigureAwait(false);

        var namespaces = GetKnownNamespaceNames();
        if (namespaces.Count == 0)
        {
            return;
        }

        await Parallel.ForEachAsync(
            namespaces,
            new ParallelOptions { MaxDegreeOfParallelism = MaximumConcurrentNamespaceRulesReviews, CancellationToken = cancellationToken },
            async (@namespace, token) => await GetNamespaceRulesAsync(@namespace, token).ConfigureAwait(false)).ConfigureAwait(false);
    }

    private async Task<bool> EnsureWildcardAdminAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var cache = Volatile.Read(ref _authorizationCache);
            if (cache.IsWildcardAdmin is bool known)
            {
                return known;
            }

            var existing = Volatile.Read(ref cache.WildcardAdminLoad);
            if (existing is null)
            {
                var candidate = new Lazy<Task<bool>>(
                    () => LoadWildcardAdminAsync(cache),
                    LazyThreadSafetyMode.ExecutionAndPublication);
                if (!ReferenceEquals(Interlocked.CompareExchange(ref cache.WildcardAdminLoad, candidate, null), null))
                {
                    continue;
                }

                existing = candidate;
            }

            return await existing.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<bool> LoadWildcardAdminAsync(AuthorizationCacheState cache)
    {
        using var activity = StartClusterActivity(nameof(LoadWildcardAdminAsync));

        var client = Client;
        if (client is null)
        {
            return false;
        }

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "*",
                    Version = "*",
                    Resource = "*",
                    Verb = "*",
                },
            },
        };

        try
        {
            var response = await client.AuthorizationV1
                .CreateSelfSubjectAccessReviewAsync(model, cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
            var allowed = response.Status?.Allowed == true;

            // A review started before an invalidation belongs to the old generation.
            // Re-run against the current generation so an admin flag cannot outlive it.
            if (!ReferenceEquals(Volatile.Read(ref _authorizationCache), cache))
            {
                return await EnsureWildcardAdminAsync(CancellationToken.None).ConfigureAwait(false);
            }

            cache.IsWildcardAdmin = allowed;
            return allowed;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "Unable to probe wildcard administrator permission for {ClusterName}", Name);
            }

            if (ReferenceEquals(Volatile.Read(ref _authorizationCache), cache))
            {
                cache.IsWildcardAdmin = false;
            }

            return false;
        }
    }

    internal bool IsWildcardAdmin()
        => Volatile.Read(ref _authorizationCache).IsWildcardAdmin == true;

    private bool HasWildcard(Verb verb)
    {
        if (verb is not (Verb.List or Verb.Watch))
        {
            return false;
        }

        var cache = Volatile.Read(ref _authorizationCache);
        return cache.WildcardDecisions.TryGetValue(verb.ToString().ToLowerInvariant(), out var allowed) && allowed;
    }

    private async Task EnsureWildcardVerbAsync(Verb verb, CancellationToken cancellationToken)
    {
        var key = verb.ToString().ToLowerInvariant();
        while (true)
        {
            var cache = Volatile.Read(ref _authorizationCache);
            if (cache.WildcardDecisions.TryGetValue(key, out _))
            {
                return;
            }

            var load = cache.WildcardLoads.GetOrAdd(
                key,
                static (_, state) => new Lazy<Task<bool>>(
                    () => state.Cluster.LoadWildcardVerbAsync(state.Cache, state.Verb),
                    LazyThreadSafetyMode.ExecutionAndPublication),
                (Cluster: this, Cache: cache, Verb: verb));

            await load.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
    }

    private async Task<bool> LoadWildcardVerbAsync(AuthorizationCacheState cache, Verb verb)
    {
        using var activity = StartClusterActivity(nameof(LoadWildcardVerbAsync));
        activity?.SetTag("kubernetes.authorization.verb", verb.ToString());

        var client = Client;
        if (client is null)
        {
            return false;
        }

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "*",
                    Version = "*",
                    Resource = "*",
                    Verb = verb.ToString().ToLowerInvariant(),
                },
            },
        };

        try
        {
            var response = await client.AuthorizationV1
                .CreateSelfSubjectAccessReviewAsync(model, cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
            var allowed = response.Status?.Allowed == true;

            if (ReferenceEquals(Volatile.Read(ref _authorizationCache), cache))
            {
                cache.WildcardDecisions[verb.ToString().ToLowerInvariant()] = allowed;
            }

            return allowed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "Unable to probe wildcard {Verb} permission for {ClusterName}", verb, Name);
            }

            if (ReferenceEquals(Volatile.Read(ref _authorizationCache), cache))
            {
                cache.WildcardDecisions[verb.ToString().ToLowerInvariant()] = false;
            }

            return false;
        }
    }

    /// <summary>
    /// True when a namespace's rules review could not be completed. Used only to keep
    /// navigation visibility Lens-like; destructive commands still fail closed.
    /// </summary>
    private bool HasIncompleteNamespaceRules(Verb verb, string? @namespace)
    {
        if (verb is not (Verb.List or Verb.Watch))
        {
            return false;
        }

        var cache = Volatile.Read(ref _authorizationCache);
        if (!string.IsNullOrEmpty(@namespace))
        {
            return cache.NamespaceRules.TryGetValue(@namespace, out var rules) && !rules.IsComplete;
        }

        foreach (var name in GetKnownNamespaceNames())
        {
            if (cache.NamespaceRules.TryGetValue(name, out var rules) && !rules.IsComplete)
            {
                return true;
            }
        }

        return false;
    }
}
