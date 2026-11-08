using System.Net;
using System.Net.Http.Headers;
using System.Text;
using KubeUI.Kubernetes.Serialization;

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
            var error = HasConfiguredAuthentication(endpoint)
                ? "<redacted for authenticated request>"
                : await ReadErrorSummaryAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning(
                "Prometheus direct query failed for provider {Provider} with HTTP {StatusCode} ({ReasonPhrase}). Response: {ResponseError}",
                endpoint.ProviderKind,
                (int)response.StatusCode,
                response.ReasonPhrase,
                error);
            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync(
            KubernetesJsonStaticContext.Default.PrometheusClientQueryRangeResponse,
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
        ApplyConfiguredAuthentication(request, endpoint);
        if (endpoint.UseAzureMonitorAuthentication)
        {
            var cachedToken = Volatile.Read(ref _azureMonitorAccessToken);
            if (!string.IsNullOrWhiteSpace(cachedToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken);
            }
        }

        var response = await _httpClient!.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Unauthorized || !endpoint.UseAzureMonitorAuthentication)
        {
            return response;
        }

        response.Dispose();

        var token = await GetAzureMonitorTokenAsync(cancellationToken, forceRefresh: true).ConfigureAwait(false);
        using var retryRequest = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyConfiguredAuthentication(retryRequest, endpoint);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var authorization = new AuthenticationHeaderValue("Bearer", token);
            Volatile.Write(ref _azureMonitorAccessToken, token);
            retryRequest.Headers.Authorization = authorization;
        }

        return await _httpClient!.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateAuthentication(ResolvedPrometheusEndpoint endpoint, Uri baseAddress)
    {
        var hasUsername = !string.IsNullOrEmpty(endpoint.BasicAuthUsername);
        var hasPassword = !string.IsNullOrEmpty(endpoint.BasicAuthPassword);
        if (hasUsername != hasPassword)
        {
            throw new InvalidOperationException("Prometheus Basic Auth requires both a username and a password.");
        }

        if (hasUsername && endpoint.BasicAuthUsername!.Contains(':', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Prometheus Basic Auth usernames cannot contain a colon.");
        }

        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hasAuthorizationHeader = false;
        if (endpoint.CustomHeaders is not null)
        {
            foreach (var header in endpoint.CustomHeaders)
            {
                var hasName = !string.IsNullOrWhiteSpace(header.Name);
                var hasValue = !string.IsNullOrEmpty(header.Value);
                if (!hasName && !hasValue)
                {
                    continue;
                }

                if (!hasName || !IsValidHeaderName(header.Name))
                {
                    throw new InvalidOperationException("A configured Prometheus header has an invalid name.");
                }

                if (!seenNames.Add(header.Name))
                {
                    throw new InvalidOperationException($"Prometheus header '{header.Name}' is configured more than once.");
                }

                if (IsRestrictedHeader(header.Name))
                {
                    throw new InvalidOperationException($"Prometheus header '{header.Name}' is controlled by the HTTP transport and cannot be configured.");
                }

                if (header.Value.IndexOf('\r') >= 0 || header.Value.IndexOf('\n') >= 0)
                {
                    throw new InvalidOperationException($"Prometheus header '{header.Name}' contains an invalid line break.");
                }

                hasAuthorizationHeader |= string.Equals(header.Name, "Authorization", StringComparison.OrdinalIgnoreCase);
            }
        }

        if (hasUsername && hasAuthorizationHeader)
        {
            throw new InvalidOperationException("Prometheus Basic Auth cannot be combined with a custom Authorization header.");
        }

        if (HasConfiguredAuthentication(endpoint)
            && endpoint.RequireHttpsForAuthentication
            && !string.Equals(baseAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Prometheus authentication requires an HTTPS endpoint. Disable the HTTPS requirement only if you accept sending credentials without transport encryption.");
        }
    }

    private static bool IsValidHeaderName(string name)
    {
        foreach (var character in name)
        {
            if (!char.IsAsciiLetterOrDigit(character) && "!#$%&'*+-.^_`|~".IndexOf(character) < 0)
            {
                return false;
            }
        }

        return name.Length > 0;
    }

    private static bool IsRestrictedHeader(string name)
    {
        return name.Equals("Host", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Connection", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
            || name.Equals("TE", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Trailer", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Upgrade", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Proxy-Connection", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasConfiguredAuthentication(ResolvedPrometheusEndpoint endpoint)
    {
        return endpoint.UseAzureMonitorAuthentication
            || !string.IsNullOrEmpty(endpoint.BasicAuthUsername)
            || !string.IsNullOrEmpty(endpoint.BasicAuthPassword)
            || endpoint.CustomHeaders?.Any(static header =>
                !string.IsNullOrWhiteSpace(header.Name) || !string.IsNullOrEmpty(header.Value)) == true;
    }

    private static void ApplyConfiguredAuthentication(HttpRequestMessage request, ResolvedPrometheusEndpoint endpoint)
    {
        if (endpoint.CustomHeaders is not null)
        {
            foreach (var header in endpoint.CustomHeaders)
            {
                if (string.IsNullOrWhiteSpace(header.Name) && string.IsNullOrEmpty(header.Value))
                {
                    continue;
                }

                if (!request.Headers.TryAddWithoutValidation(header.Name, header.Value))
                {
                    throw new InvalidOperationException($"Prometheus header '{header.Name}' cannot be added to a request.");
                }
            }
        }

        if (!string.IsNullOrEmpty(endpoint.BasicAuthUsername))
        {
            var credentials = string.Concat(endpoint.BasicAuthUsername, ":", endpoint.BasicAuthPassword);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
        }
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
        if (handler is not null)
        {
            client = new HttpClient(handler);
        }
        else if (HasConfiguredAuthentication(endpoint))
        {
            // Prevent custom credentials from being forwarded to a redirect target.
#pragma warning disable CA2000 // HttpClient owns and disposes the handler.
            client = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false });
#pragma warning restore CA2000
        }
        else if (factory is not null)
        {
            client = factory.CreateClient();
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
