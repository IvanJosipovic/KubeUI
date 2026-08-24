using System.Net;
using System.Net.Http.Json;
namespace KubeUI.Kubernetes;

/// <summary>
/// Retrieves and caches Kubernetes aggregated API discovery responses.
/// </summary>
public sealed class KubernetesApiDiscoveryClient
{
    private readonly k8s.Kubernetes _client;
    private string? _coreETag;
    private string? _groupedETag;
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
                Core = await RefreshEndpointAsync(true, cancellationToken).ConfigureAwait(false);
                Groups = await RefreshEndpointAsync(false, cancellationToken).ConfigureAwait(false);
            }
            while (Interlocked.Exchange(ref _refreshRequested, 0) != 0);
        }
        finally
        {
            Volatile.Write(ref _refreshInProgress, 0);
        }
    }

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

        using var response = await _client.SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return core
                ? Core ?? throw new InvalidOperationException("Core API discovery cache is empty.")
                : Groups ?? throw new InvalidOperationException("Grouped API discovery cache is empty.");
        }

        response.EnsureSuccessStatusCode();
        if (response.Headers.ETag is { } responseETag)
        {
            if (core)
            {
                _coreETag = responseETag.Tag;
            }
            else
            {
                _groupedETag = responseETag.Tag;
            }
        }

        return await response.Content.ReadFromJsonAsync(
                CustomSourceGenerationContext.Default.V2beta1APIGroupDiscoveryList,
                cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("API group discovery response was empty.");
    }
}
