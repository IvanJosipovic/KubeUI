using System.Net.Http.Headers;

namespace KubeUI.Kubernetes;

public sealed partial class PrometheusQueryClient
{
    private async Task<PrometheusClientQueryRangeResponse?> QueryDirectAsync(
        ResolvedPrometheusEndpoint endpoint,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken)
    {
        var url = BuildDirectUrl(endpoint, query, start, end, stepSeconds);
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        if (endpoint.UseAzureMonitorAuthentication)
        {
            if (_azureMonitorWorkspaceService is null)
            {
                throw new InvalidOperationException("Azure Monitor authentication is not configured.");
            }

            var token = await _azureMonitorWorkspaceService.GetPrometheusAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        using var response = await _httpClient!.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorSummaryAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(
                "Prometheus direct query failed for provider {Provider} with HTTP {StatusCode} ({ReasonPhrase}). Response: {ResponseError}",
                endpoint.ProviderKind,
                (int)response.StatusCode,
                response.ReasonPhrase,
                error);
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ReadErrorSummaryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        const int maxLength = 1024;
        return body.Length <= maxLength ? body : body[..maxLength] + "…";
    }

    private static string BuildDirectUrl(ResolvedPrometheusEndpoint endpoint, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        return $"{endpoint.DirectUrl!.TrimEnd('/')}{endpoint.PathPrefix}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private static HttpClient CreateDirectHttpClient(ResolvedPrometheusEndpoint endpoint, HttpMessageHandler? handler)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler);
        client.BaseAddress = new Uri(endpoint.DirectUrl!.TrimEnd('/') + endpoint.PathPrefix, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }
}
