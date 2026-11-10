using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace KubeUI.Testing.Kubernetes.Transport;

/// <summary>Mutates successful API group discovery responses in deterministic Kubernetes tests.</summary>
public sealed class DiscoveryResponseHandler : DelegatingHandler
{
    private readonly Action<JsonObject> _mutateDiscovery;

    /// <summary>Creates a handler that applies a mutation to each successful <c>/apis</c> response.</summary>
    /// <param name="mutateDiscovery">Mutation applied to the parsed discovery document.</param>
    /// <exception cref="ArgumentNullException"><paramref name="mutateDiscovery"/> is null.</exception>
    public DiscoveryResponseHandler(Action<JsonObject> mutateDiscovery)
    {
        _mutateDiscovery = mutateDiscovery ?? throw new ArgumentNullException(nameof(mutateDiscovery));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (request.RequestUri?.AbsolutePath != "/apis" || response.StatusCode != HttpStatusCode.OK)
        {
            return response;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var discovery = JsonNode.Parse(content)!.AsObject();
        _mutateDiscovery(discovery);

        response.Content.Dispose();
        response.Content = new StringContent(discovery.ToJsonString(), Encoding.UTF8, "application/json");
        response.Headers.Remove("ETag");
        return response;
    }
}
