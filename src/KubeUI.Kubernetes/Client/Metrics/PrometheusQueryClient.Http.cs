using System.Net.Http.Json;

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
        using var response = await _httpClient!.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static string BuildDirectUrl(ResolvedPrometheusEndpoint endpoint, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        return $"{endpoint.DirectUrl!.TrimEnd('/')}{endpoint.PathPrefix}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private static HttpClient CreateDirectHttpClient(ResolvedPrometheusEndpoint endpoint)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(endpoint.DirectUrl!.TrimEnd('/') + endpoint.PathPrefix, UriKind.Absolute),
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }
}
