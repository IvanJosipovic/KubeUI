using System.Diagnostics;
using System.Net;
using System.Text.Json;
using KubeUI.Kubernetes.Client;
using KubeUI.Kubernetes.Serialization;
namespace KubeUI.Kubernetes;

/// <summary>
/// Retrieves and caches Kubernetes aggregated API discovery responses.
/// </summary>
public sealed class KubernetesApiDiscoveryClient
{
    private readonly k8s.Kubernetes _client;
    private string? _coreETag;
    private string? _groupedETag;
    private V2beta1APIGroupDiscoveryList? _cachedCore;
    private V2beta1APIGroupDiscoveryList? _cachedGroups;
    private int _refreshInProgress;
    private int _refreshRequested;

    /// <summary>Creates a discovery client for an authenticated Kubernetes client.</summary>
    /// <param name="client">Kubernetes client used for discovery requests.</param>
    public KubernetesApiDiscoveryClient(k8s.Kubernetes client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>Gets core API discovery data, or null before a successful refresh.</summary>
    public V2beta1APIGroupDiscoveryList? Core { get; private set; }

    /// <summary>Gets grouped API discovery data, or null before a successful refresh.</summary>
    public V2beta1APIGroupDiscoveryList? Groups { get; private set; }

    /// <summary>Gets the last complete discovery snapshot, or null before a successful refresh.</summary>
    public ApiDiscoverySnapshot? Snapshot { get; private set; }

    /// <summary>Raised after a complete discovery refresh, including an unchanged ETag response.</summary>
    public event Action<ApiDiscoverySnapshot>? SnapshotRefreshed;

    /// <summary>Refreshes core and grouped API discovery data, skipping concurrent requests.</summary>
    /// <param name="cancellationToken">Token used to cancel the refresh.</param>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _refreshInProgress, 1) != 0)
        {
            Volatile.Write(ref _refreshRequested, 1);
            return;
        }

        try
        {
            do
            {
                Volatile.Write(ref _refreshRequested, 0);
                var core = await RefreshEndpointAsync(true, cancellationToken).ConfigureAwait(false);
                var groups = await RefreshEndpointAsync(false, cancellationToken).ConfigureAwait(false);
                ApiDiscoverySnapshot snapshot;
                try
                {
                    snapshot = ApiDiscoverySnapshot.Create(core, groups, Snapshot);
                }
                catch
                {
                    _coreETag = null;
                    _groupedETag = null;
                    throw;
                }

                Core = core;
                Groups = groups;
                Snapshot = snapshot;
                SnapshotRefreshed?.Invoke(snapshot);
            }
            while (Interlocked.Exchange(ref _refreshRequested, 0) != 0);
        }
        finally
        {
            Volatile.Write(ref _refreshInProgress, 0);
        }
    }

    /// <summary>Fetches one discovery endpoint using aggregated or legacy Kubernetes API responses.</summary>
    private async Task<V2beta1APIGroupDiscoveryList> RefreshEndpointAsync(
        bool core,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(_client.BaseUri, (core ? "api" : "apis") + "?timeout=32s"));
        request.Headers.TryAddWithoutValidation(
            "Accept",
            "application/json;g=apidiscovery.k8s.io;v=v2;as=APIGroupDiscoveryList,application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList,application/json");

        var etag = core ? _coreETag : _groupedETag;
        if (!string.IsNullOrWhiteSpace(etag))
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        }

        using var activity = KubeInstrumentation.Source.StartActivity(
            "kubernetes.discovery",
            ActivityKind.Client);
        activity?.SetTag("http.request.method", request.Method.Method);
        activity?.SetTag("url.full", request.RequestUri?.ToString());
        activity?.SetTag("url.path", request.RequestUri?.AbsolutePath);

        try
        {
            using var response = await _client.SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("http.response.status_code", (int)response.StatusCode);
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                return core
                    ? _cachedCore ?? throw new InvalidOperationException("Core API discovery cache is empty.")
                    : _cachedGroups ?? throw new InvalidOperationException("Grouped API discovery cache is empty.");
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(content);
            var responseKind = document.RootElement.TryGetProperty("kind", out var kindElement)
                ? kindElement.GetString()
                : null;
            V2beta1APIGroupDiscoveryList result;
            var isAggregatedDiscovery = string.Equals(responseKind, "APIGroupDiscoveryList", StringComparison.Ordinal);
            if (isAggregatedDiscovery)
            {
                result = JsonSerializer.Deserialize(
                        document.RootElement,
                        KubernetesJsonStaticContext.Default.V2beta1APIGroupDiscoveryList)
                    ?? throw new InvalidOperationException("API group discovery response was empty.");
                if (result.items is null)
                {
                    throw new InvalidOperationException("API group discovery response was incomplete.");
                }
            }
            else if (core && string.Equals(responseKind, "APIVersions", StringComparison.Ordinal))
            {
                result = await ReadLegacyCoreDiscoveryAsync(document.RootElement, cancellationToken).ConfigureAwait(false);
            }
            else if (!core && string.Equals(responseKind, "APIGroupList", StringComparison.Ordinal))
            {
                result = await ReadLegacyGroupedDiscoveryAsync(document.RootElement, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException("API group discovery response was incomplete or unsupported.");
            }

            if (core)
            {
                _cachedCore = result;
                _coreETag = isAggregatedDiscovery ? response.Headers.ETag?.Tag : null;
            }
            else
            {
                _cachedGroups = result;
                _groupedETag = isAggregatedDiscovery ? response.Headers.ETag?.Tag : null;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    /// <summary>Loads each legacy core API resource list and maps it to an aggregated discovery response.</summary>
    private async Task<V2beta1APIGroupDiscoveryList> ReadLegacyCoreDiscoveryAsync(
        JsonElement response,
        CancellationToken cancellationToken)
    {
        var versions = new List<V2beta1APIGroupDiscoveryListItemVersion>();
        if (response.TryGetProperty("versions", out var versionElements) && versionElements.ValueKind == JsonValueKind.Array)
        {
            foreach (var versionElement in versionElements.EnumerateArray())
            {
                var version = versionElement.GetString();
                if (!string.IsNullOrWhiteSpace(version))
                {
                    versions.Add(await ReadLegacyResourceVersionAsync(
                        $"api/{Uri.EscapeDataString(version)}",
                        string.Empty,
                        version,
                        cancellationToken).ConfigureAwait(false));
                }
            }
        }

        return CreateDiscoveryList([CreateDiscoveryGroup(string.Empty, versions)]);
    }

    /// <summary>Loads legacy group versions and maps their resource lists to an aggregated discovery response.</summary>
    private async Task<V2beta1APIGroupDiscoveryList> ReadLegacyGroupedDiscoveryAsync(
        JsonElement response,
        CancellationToken cancellationToken)
    {
        var groups = new List<V2beta1APIGroupDiscoveryListItem>();
        if (response.TryGetProperty("groups", out var groupElements) && groupElements.ValueKind == JsonValueKind.Array)
        {
            foreach (var groupElement in groupElements.EnumerateArray())
            {
                var group = groupElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
                if (string.IsNullOrWhiteSpace(group)
                    || !groupElement.TryGetProperty("versions", out var versionElements)
                    || versionElements.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var versions = new List<V2beta1APIGroupDiscoveryListItemVersion>();
                foreach (var versionElement in versionElements.EnumerateArray())
                {
                    var version = versionElement.TryGetProperty("version", out var versionNameElement)
                        ? versionNameElement.GetString()
                        : null;
                    if (string.IsNullOrWhiteSpace(version))
                    {
                        continue;
                    }

                    versions.Add(await ReadLegacyResourceVersionAsync(
                        $"apis/{Uri.EscapeDataString(group)}/{Uri.EscapeDataString(version)}",
                        group,
                        version,
                        cancellationToken).ConfigureAwait(false));
                }

                groups.Add(CreateDiscoveryGroup(group, versions));
            }
        }

        return CreateDiscoveryList(groups);
    }

    /// <summary>Reads one legacy API resource list and converts its root resources to discovery entries.</summary>
    private async Task<V2beta1APIGroupDiscoveryListItemVersion> ReadLegacyResourceVersionAsync(
        string path,
        string group,
        string version,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_client.BaseUri, path));
        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        using var response = await _client.SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
        var root = document.RootElement;
        if (!root.TryGetProperty("kind", out var kindElement)
            || !string.Equals(kindElement.GetString(), "APIResourceList", StringComparison.Ordinal)
            || !root.TryGetProperty("resources", out var resourceElements)
            || resourceElements.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"Legacy discovery response for '{path}' was incomplete.");
        }

        var resources = new List<V2beta1APIGroupDiscoveryListItemVersionResource>();
        foreach (var resourceElement in resourceElements.EnumerateArray())
        {
            var name = resourceElement.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;
            var kind = resourceElement.TryGetProperty("kind", out var resourceKindElement)
                ? resourceKindElement.GetString()
                : null;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(kind))
            {
                continue;
            }

            var namespaced = resourceElement.TryGetProperty("namespaced", out var namespacedElement)
                && namespacedElement.ValueKind == JsonValueKind.True;
            var verbs = resourceElement.TryGetProperty("verbs", out var verbElements)
                && verbElements.ValueKind == JsonValueKind.Array
                ? verbElements.EnumerateArray()
                    .Select(static verbElement => verbElement.GetString())
                    .Where(static verb => !string.IsNullOrWhiteSpace(verb))
                    .Select(static verb => verb!)
                    .ToArray()
                : [];

            resources.Add(new V2beta1APIGroupDiscoveryListItemVersionResource
            {
                resource = name,
                responseKind = new V2beta1APIGroupDiscoveryListItemVersionResourceResponsekind
                {
                    group = group,
                    version = version,
                    kind = kind,
                },
                scope = namespaced ? "Namespaced" : "Cluster",
                verbs = verbs,
            });
        }

        return new V2beta1APIGroupDiscoveryListItemVersion
        {
            version = version,
            freshness = "Current",
            resources = resources,
        };
    }

    private static V2beta1APIGroupDiscoveryList CreateDiscoveryList(
        IList<V2beta1APIGroupDiscoveryListItem> items)
        => new()
        {
            kind = "APIGroupDiscoveryList",
            apiVersion = "apidiscovery.k8s.io/v2beta1",
            items = items,
        };

    private static V2beta1APIGroupDiscoveryListItem CreateDiscoveryGroup(
        string name,
        IList<V2beta1APIGroupDiscoveryListItemVersion> versions)
        => new()
        {
            metadata = new V2beta1APIGroupDiscoveryListItemMetadata { name = name },
            versions = versions,
        };
}
