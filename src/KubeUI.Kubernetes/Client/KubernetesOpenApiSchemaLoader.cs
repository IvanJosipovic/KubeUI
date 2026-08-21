using System.Diagnostics;
using System.Text.Json;
using KubeUI.Kubernetes.Client;
using KubernetesClient.Informer.Client;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace KubeUI.Kubernetes;

/// <summary>
/// Coordinates one-time, retryable OpenAPI schema loading for a cluster.
/// </summary>
public sealed class KubernetesOpenApiSchemaLoader : IDisposable
{
    private readonly KubernetesOpenApiSchemaCatalog _catalog;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<string, OpenApiDocument> _activeDocuments = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string?> _activeHashes = new(StringComparer.Ordinal);
    private readonly Dictionary<OpenApiCacheKey, OpenApiDocument> _documentCache = [];
    private readonly Lock _cacheGate = new();
    private int _loadAttempted;
    private int _disposed;

    /// <summary>Creates a retryable loader for a schema catalog.</summary>
    /// <param name="catalog">Catalog populated by successful loads.</param>
    /// <param name="logger">Logger for transient load failures.</param>
    public KubernetesOpenApiSchemaLoader(
        KubernetesOpenApiSchemaCatalog catalog,
        ILogger logger)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task LoadAsync(k8s.Kubernetes client, CancellationToken cancellationToken = default)
    {
        using var credentialsHandler = new KubernetesCredentialsHandler(client);
        using var authenticatedClient = new HttpClient(credentialsHandler)
        {
            BaseAddress = client.BaseUri,
            Timeout = Timeout.InfiniteTimeSpan,
        };

        using var indexResponse = await authenticatedClient.GetAsync(
            new Uri(client.BaseUri, "openapi/v3"),
            cancellationToken).ConfigureAwait(false);
        indexResponse.EnsureSuccessStatusCode();

        using var index = JsonDocument.Parse(
            await indexResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        if (!index.RootElement.TryGetProperty("paths", out var paths)
            || paths.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        Dictionary<string, (string? Hash, Uri Uri)> references = [];
        foreach (var path in paths.EnumerateObject())
        {
            if (!path.Value.TryGetProperty("serverRelativeURL", out var url)
                || url.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var schemaUri = new Uri(client.BaseUri, url.GetString()!.TrimStart('/'));
            var groupVersion = ParseGroupVersion(path.Name);
            if (groupVersion is null)
            {
                continue;
            }

            references[groupVersion] = (ParseHash(schemaUri), schemaUri);
        }

        Dictionary<string, OpenApiDocument> nextDocuments = [];
        Dictionary<string, string?> nextHashes = [];
        lock (_cacheGate)
        {
            foreach (var reference in references)
            {
                if (reference.Value.Hash is not null
                    && _activeHashes.TryGetValue(reference.Key, out var activeHash)
                    && string.Equals(activeHash, reference.Value.Hash, StringComparison.Ordinal)
                    && _activeDocuments.TryGetValue(reference.Key, out var activeDocument))
                {
                    nextHashes[reference.Key] = activeHash;
                    nextDocuments[reference.Key] = activeDocument;
                    continue;
                }

                if (reference.Value.Hash is not null
                    && _documentCache.TryGetValue(
                        new OpenApiCacheKey(reference.Key, reference.Value.Hash),
                        out var cachedDocument))
                {
                    nextHashes[reference.Key] = reference.Value.Hash;
                    nextDocuments[reference.Key] = cachedDocument;
                }
            }
        }

        foreach (var reference in references)
        {
            if (nextDocuments.ContainsKey(reference.Key))
            {
                continue;
            }

            using var schemaResponse = await authenticatedClient.GetAsync(reference.Value.Uri, cancellationToken).ConfigureAwait(false);
            if (!schemaResponse.IsSuccessStatusCode)
            {
                lock (_cacheGate)
                {
                    if (_activeDocuments.TryGetValue(reference.Key, out var previousDocument)
                        && _activeHashes.TryGetValue(reference.Key, out var previousHash))
                    {
                        nextDocuments[reference.Key] = previousDocument;
                        nextHashes[reference.Key] = previousHash;
                    }
                }

                continue;
            }

            var readerSettings = new OpenApiReaderSettings
            {
                HttpClient = authenticatedClient,
                LoadExternalRefs = true,
                BaseUrl = reference.Value.Uri,
            };
            await using var schemaStream = await schemaResponse.Content
                .ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var readResult = await OpenApiDocument.LoadAsync(
                schemaStream,
                "json",
                readerSettings,
                cancellationToken).ConfigureAwait(false);

            if (readResult.Document is not null)
            {
                nextDocuments[reference.Key] = readResult.Document;
                nextHashes[reference.Key] = reference.Value.Hash;
                lock (_cacheGate)
                {
                    if (reference.Value.Hash is not null)
                    {
                        _documentCache[new OpenApiCacheKey(reference.Key, reference.Value.Hash)] = readResult.Document;
                    }
                }
            }
        }

        lock (_cacheGate)
        {
            _activeDocuments.Clear();
            _activeHashes.Clear();
            foreach (var document in nextDocuments)
            {
                _activeDocuments[document.Key] = document.Value;
                _activeHashes[document.Key] = nextHashes[document.Key];
            }

            foreach (var key in _documentCache.Keys.ToList())
            {
                if (!_activeHashes.TryGetValue(key.GroupVersion, out var activeHash)
                    || !string.Equals(activeHash, key.Hash, StringComparison.Ordinal))
                {
                    _documentCache.Remove(key);
                }
            }

            _catalog.Replace(_activeDocuments.Values);
        }
    }

    private static string? ParseGroupVersion(string path)
    {
        path = path.TrimStart('/');
        if (path.StartsWith("api/", StringComparison.Ordinal))
        {
            return $"/{path[4..]}";
        }

        return path.StartsWith("apis/", StringComparison.Ordinal) ? path[5..] : null;
    }

    private static string? ParseHash(Uri uri)
    {
        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = parameter.IndexOf('=');
            if (separator > 0 && string.Equals(parameter[..separator], "hash", StringComparison.Ordinal))
            {
                return Uri.UnescapeDataString(parameter[(separator + 1)..]);
            }
        }

        return null;
    }

    private readonly record struct OpenApiCacheKey(string GroupVersion, string? Hash);

    private sealed class KubernetesCredentialsHandler : DelegatingHandler
    {
        private readonly k8s.Kubernetes _client;

        public KubernetesCredentialsHandler(k8s.Kubernetes client)
        {
            _client = client;
            InnerHandler = new ForwardingHandler(client.HttpClient);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_client.Credentials is not null)
            {
                await _client.Credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class ForwardingHandler : HttpMessageHandler
    {
        private readonly HttpClient _client;

        public ForwardingHandler(HttpClient client)
        {
            _client = client;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var forwardedRequest = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy,
            };

            foreach (var header in request.Headers)
            {
                forwardedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return SendAsyncCore(forwardedRequest, cancellationToken);
        }

        private async Task<HttpResponseMessage> SendAsyncCore(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using (request)
            {
                return await _client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>Loads schemas once, retrying transient failures, and returns quietly when all attempts fail.</summary>
    /// <param name="client">Authenticated Kubernetes client.</param>
    /// <param name="clusterName">Cluster name used for diagnostics.</param>
    /// <param name="cancellationToken">Token used to cancel loading.</param>
    public async Task EnsureAsync(
        k8s.Kubernetes client,
        string clusterName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        using var activity = KubeInstrumentation.Source.StartActivity(
            nameof(KubernetesOpenApiSchemaLoader),
            ActivityKind.Internal);
        activity?.SetTag("kubernetes.cluster.name", clusterName);
        activity?.SetTag("kubernetes.openapi.schema.count", _catalog.Count);

        if (Volatile.Read(ref _loadAttempted) != 0 && _catalog.Count > 0)
        {
            activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref _loadAttempted) != 0 && _catalog.Count > 0)
            {
                activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
                return;
            }

            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await LoadAsync(client, cancellationToken).ConfigureAwait(false);
                    Volatile.Write(ref _loadAttempted, 1);
                    activity?.SetTag("kubernetes.openapi.schema.count", _catalog.Count);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    _logger.LogDebug(
                        ex,
                        "Unable to load Kubernetes OpenAPI v3 schemas for {name} on attempt {attempt} of {maxAttempts}.",
                        clusterName,
                        attempt,
                        maxAttempts);

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Allows a later <see cref="EnsureAsync"/> call to load schemas again.</summary>
    public void Reset()
    {
        Volatile.Write(ref _loadAttempted, 0);
    }

    /// <summary>Releases the loader synchronization resources.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            _gate.Dispose();
        }
    }
}
