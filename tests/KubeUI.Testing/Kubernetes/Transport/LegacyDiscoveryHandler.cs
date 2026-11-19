namespace KubeUI.Testing.Kubernetes.Transport;

/// <summary>Forces legacy discovery responses from the fake Kubernetes API.</summary>
public sealed class LegacyDiscoveryHandler : DelegatingHandler
{
    /// <summary>Removes aggregated-discovery media types from top-level discovery requests.</summary>
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath is "/api" or "/apis")
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.ParseAdd("application/json");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
