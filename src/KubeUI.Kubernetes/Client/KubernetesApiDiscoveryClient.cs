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

    public KubernetesApiDiscoveryClient(k8s.Kubernetes client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public V2beta1APIGroupDiscoveryList? Core { get; private set; }

    public V2beta1APIGroupDiscoveryList? Groups { get; private set; }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Core = await RefreshEndpointAsync(true, cancellationToken).ConfigureAwait(false);
        Groups = await RefreshEndpointAsync(false, cancellationToken).ConfigureAwait(false);
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
