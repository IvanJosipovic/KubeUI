using System.Net;
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
        var url = BuildDirectUrl(query, start, end, stepSeconds);

        using var response = await SendWithAuthenticationAsync(endpoint, url, cancellationToken).ConfigureAwait(false);
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

        return await response.Content.ReadFromJsonAsync(
            CustomSourceGenerationContext.Default.PrometheusClientQueryRangeResponse,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the query, attaching the transport-level authorization header when one is configured.
    /// </summary>
    /// <remarks>
    /// The Azure Monitor token is attached to the transport when it is prepared. If the token has
    /// expired in the meantime the request is rejected with 401, so a fresh token is resolved and the
    /// request is retried once.
    /// </remarks>
    private async Task<HttpResponseMessage> SendWithAuthenticationAsync(
        ResolvedPrometheusEndpoint endpoint,
        string url,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _httpClient!.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Unauthorized || !endpoint.UseAzureMonitorAuthentication)
        {
            return response;
        }

        response.Dispose();

        var token = await GetAzureMonitorTokenAsync(cancellationToken, forceRefresh: true).ConfigureAwait(false);
        using var retryRequest = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(token))
        {
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient!.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Resolves an Azure Monitor Prometheus access token.</summary>
    /// <param name="cancellationToken">Token used to cancel the acquisition.</param>
    /// <param name="forceRefresh">Whether to bypass the cached token and acquire a new one.</param>
    /// <returns>The access token, or <see langword="null"/> when Azure Monitor authentication is not configured.</returns>
    private async Task<string?> GetAzureMonitorTokenAsync(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        if (_azureMonitorWorkspaceService is null)
        {
            throw new InvalidOperationException("Azure Monitor authentication is not configured.");
        }

        if (forceRefresh)
        {
            await _azureMonitorWorkspaceService.RefreshPrometheusAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        var token = await _azureMonitorWorkspaceService.GetPrometheusAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        return token.Token;
    }

    private static async Task<string> ReadErrorSummaryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        const int maxLength = 1024;
        return body.Length <= maxLength ? body : body[..maxLength] + "…";
    }

    private string BuildDirectUrl(string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        var relativeUrl = $"api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
        return new Uri(_httpClient!.BaseAddress!, relativeUrl).AbsoluteUri;
    }

    private static HttpClient CreateDirectHttpClient(ResolvedPrometheusEndpoint endpoint, HttpMessageHandler? handler, IHttpClientFactory? factory)
    {
        HttpClient client;
        if (factory is not null)
        {
            client = factory.CreateClient();
        }
        else if (handler is not null)
        {
            client = new HttpClient(handler);
        }
        else
        {
            client = new HttpClient();
        }
        client.BaseAddress = BuildBaseAddress(endpoint);
        return client;
    }

    /// <summary>Builds the base address for a direct Prometheus endpoint.</summary>
    /// <param name="endpoint">The resolved endpoint.</param>
    /// <returns>The base address including the configured path prefix.</returns>
    private static Uri BuildBaseAddress(ResolvedPrometheusEndpoint endpoint)
    {
        var builder = new UriBuilder(endpoint.DirectUrl!)
        {
            Path = string.IsNullOrEmpty(endpoint.PathPrefix) ? "/" : endpoint.PathPrefix.TrimEnd('/') + "/",
        };

        return builder.Uri;
    }
}
