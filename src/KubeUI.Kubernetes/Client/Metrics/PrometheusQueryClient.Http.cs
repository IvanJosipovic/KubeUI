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
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static string BuildDirectUrl(ResolvedPrometheusEndpoint endpoint, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        return $"{endpoint.DirectUrl!.TrimEnd('/')}{endpoint.PathPrefix}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private static HttpClient CreateDirectHttpClient(ResolvedPrometheusEndpoint endpoint, HttpMessageHandler? handler)
    {
        var client = handler is null ? new HttpClient() : new HttpClient(handler);
        client.BaseAddress = new Uri(endpoint.DirectUrl!.TrimEnd('/') + endpoint.PathPrefix, UriKind.Absolute);
        client.Timeout = Timeout.InfiniteTimeSpan;
        return client;
    }
}
