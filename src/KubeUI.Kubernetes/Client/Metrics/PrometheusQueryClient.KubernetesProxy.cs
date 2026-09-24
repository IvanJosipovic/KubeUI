namespace KubeUI.Kubernetes;

public sealed partial class PrometheusQueryClient
{
    private async Task<PrometheusClientQueryRangeResponse?> QueryViaKubernetesProxyAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken)
    {
        var proxyUrl = BuildServiceProxyUrl(endpoint, query, start, end, stepSeconds);
        return await SendServiceProxyRequestAsync(cluster, endpoint, proxyUrl, cancellationToken).ConfigureAwait(false);
    }

    private static string BuildServiceProxyUrl(ResolvedPrometheusEndpoint endpoint, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Namespace)
            || string.IsNullOrWhiteSpace(endpoint.ServiceName)
            || endpoint.ServicePort is not > 0)
        {
            throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
        }

        var scheme = endpoint.UseHttps ? "https" : "http";
        var queryString = $"query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
        return $"/api/v1/namespaces/{endpoint.Namespace}/services/{scheme}:{endpoint.ServiceName}:{endpoint.ServicePort}/proxy{endpoint.PathPrefix}/api/v1/query_range?{queryString}";
    }

    private async Task<PrometheusClientQueryRangeResponse?> SendServiceProxyRequestAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        string relativeUri,
        CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["accept"] = ["application/json"],
        };

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(((k8s.Kubernetes)cluster.Client!).BaseUri, relativeUri));
        foreach (var (name, values) in headers)
        {
            request.Headers.TryAddWithoutValidation(name, values);
        }

        using var response = await ((k8s.Kubernetes)cluster.Client!).SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            const int maxLength = 1024;
            if (error.Length > maxLength)
            {
                error = error[..maxLength] + "…";
            }

            _logger.LogWarning(
                "Prometheus Kubernetes-proxy query failed for provider {Provider} with HTTP {StatusCode} ({ReasonPhrase}). Response: {ResponseError}",
                endpoint.ProviderKind,
                (int)response.StatusCode,
                response.ReasonPhrase,
                error);
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
