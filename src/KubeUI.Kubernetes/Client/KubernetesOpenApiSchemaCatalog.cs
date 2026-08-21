using System.Collections.Concurrent;
using System.Text.Json;
using KubeUI.Kubernetes.Client;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Stores OpenAPI schemas published by a Kubernetes API server.
/// </summary>
public sealed class KubernetesOpenApiSchemaCatalog
{
    private readonly ConcurrentDictionary<string, IOpenApiSchema> _schemas = new(StringComparer.Ordinal);
    private readonly Dictionary<string, OpenApiDocument> _activeDocuments = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _activeHashes = new(StringComparer.Ordinal);
    private readonly Dictionary<OpenApiCacheKey, OpenApiDocument> _documentCache = [];
    private readonly Lock _gate = new();
    private long _version;

    public int Count => _schemas.Count;

    public long Version => Interlocked.Read(ref _version);

    /// <summary>
    /// Loads all OpenAPI v3 documents advertised by a Kubernetes server.
    /// </summary>
    /// <param name="client">The authenticated Kubernetes client.</param>
    /// <param name="cancellationToken">Token used to cancel the load.</param>
    public async Task LoadAsync(k8s.Kubernetes client, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

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

        Dictionary<string, (string Hash, Uri Uri)> references = [];
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

            var hash = ParseHash(schemaUri);
            if (hash is null)
            {
                continue;
            }

            references[groupVersion] = (hash, schemaUri);
        }

        Dictionary<string, OpenApiDocument> nextDocuments = [];
        Dictionary<string, string> nextHashes = [];
        lock (_gate)
        {
            foreach (var reference in references)
            {
                if (_activeHashes.TryGetValue(reference.Key, out var activeHash)
                    && string.Equals(activeHash, reference.Value.Hash, StringComparison.Ordinal)
                    && _activeDocuments.TryGetValue(reference.Key, out var activeDocument))
                {
                    nextHashes[reference.Key] = activeHash;
                    nextDocuments[reference.Key] = activeDocument;
                    continue;
                }

                if (_documentCache.TryGetValue(
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
                lock (_gate)
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
                lock (_gate)
                {
                    _documentCache[new OpenApiCacheKey(reference.Key, reference.Value.Hash)] = readResult.Document;
                }
            }
        }

        lock (_gate)
        {
            _activeDocuments.Clear();
            _activeHashes.Clear();
            foreach (var document in nextDocuments)
            {
                _activeDocuments[document.Key] = document.Value;
                _activeHashes[document.Key] = nextHashes[document.Key];
            }

            _schemas.Clear();
            foreach (var document in _activeDocuments.Values)
            {
                AddSchemas(document);
            }

            Interlocked.Increment(ref _version);
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

    private readonly record struct OpenApiCacheKey(string GroupVersion, string Hash);

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

    public void Register(OpenApiDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.RegisterComponents();

        if (document.Components?.Schemas is null)
        {
            return;
        }

        lock (_gate)
        {
            AddSchemas(document);
            Interlocked.Increment(ref _version);
        }
    }

    private void AddSchemas(OpenApiDocument document)
    {
        if (document.Components?.Schemas is not { } schemas)
        {
            return;
        }

        foreach (var definition in schemas)
        {
            _schemas[definition.Key] = definition.Value;
        }
    }

    public IOpenApiSchema? GetSchema(GroupApiVersionKind kind)
    {
        foreach (var name in GetSchemaNames(kind))
        {
            if (_schemas.TryGetValue(name, out var schema))
            {
                return ExpandReferences(schema);
            }
        }

        return null;
    }

    public IOpenApiSchema? GetSchema(string name)
    {
        return ExpandReferences(FindSchema(name));
    }

    /// <summary>
    /// Gets the resolved target for an OpenAPI schema reference.
    /// </summary>
    /// <param name="schema">The schema or schema reference.</param>
    /// <returns>The recursively resolved schema, or the original schema when its reference cannot be resolved.</returns>
    public IOpenApiSchema? ExpandReferences(IOpenApiSchema? schema)
    {
        var current = schema;
        HashSet<IOpenApiSchema>? visited = null;
        while (current is OpenApiSchemaReference reference)
        {
            visited ??= new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance);
            if (!visited.Add(current))
            {
                break;
            }

            var target = reference.RecursiveTarget
                ?? reference.Target
                ?? ResolveReference(reference);
            if (target is null || ReferenceEquals(target, current))
            {
                break;
            }

            current = target;
        }

        return current;
    }

    private IOpenApiSchema? ResolveReference(OpenApiSchemaReference reference)
    {
        var referenceName = reference.Reference?.SchemaId;
        if (string.IsNullOrWhiteSpace(referenceName))
        {
            referenceName = reference.Reference?.ReferenceV3;
        }

        if (string.IsNullOrWhiteSpace(referenceName))
        {
            referenceName = reference.Id;
        }
        if (string.IsNullOrWhiteSpace(referenceName))
        {
            return null;
        }

        var schemaName = referenceName[(referenceName.LastIndexOf('/') + 1)..];
        return FindSchema(schemaName);
    }

    public string? GetDescription(GroupApiVersionKind kind, string? propertyName = null)
    {
        var schema = GetSchema(kind);
        if (schema is null)
        {
            return null;
        }

        return propertyName is not null && schema.Properties.TryGetValue(propertyName, out var property)
            ? property.Description
            : schema.Description;
    }

    private IOpenApiSchema? FindSchema(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return null;
        }

        if (_schemas.TryGetValue(typeName, out var exactSchema))
        {
            return exactSchema;
        }

        foreach (var pair in _schemas)
        {
            if (pair.Key.EndsWith($".{typeName}", StringComparison.Ordinal))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSchemaNames(GroupApiVersionKind kind)
    {
        if (string.IsNullOrEmpty(kind.Group))
        {
            yield return $"io.k8s.api.core.{kind.ApiVersion}.{kind.Kind}";
            yield break;
        }

        var group = kind.Group.EndsWith(".k8s.io", StringComparison.Ordinal)
            ? kind.Group[..^".k8s.io".Length]
            : kind.Group;
        yield return $"io.k8s.api.{group}.{kind.ApiVersion}.{kind.Kind}";

        if (kind.Group.Contains('.', StringComparison.Ordinal))
        {
            var reversedGroup = string.Join('.', kind.Group.Split('.').Reverse());
            yield return $"{reversedGroup}.{kind.ApiVersion}.{kind.Kind}";
        }

        // Keep compatibility with older/non-Kubernetes OpenAPI documents.
        yield return $"io.{kind.Group}.{kind.ApiVersion}.{kind.Kind}";
    }
}
