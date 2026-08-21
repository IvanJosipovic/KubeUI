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

        foreach (var path in paths.EnumerateObject())
        {
            if (!path.Value.TryGetProperty("serverRelativeURL", out var url)
                || url.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var schemaUri = new Uri(client.BaseUri, url.GetString()!.TrimStart('/'));
            using var schemaResponse = await authenticatedClient.GetAsync(schemaUri, cancellationToken).ConfigureAwait(false);
            if (!schemaResponse.IsSuccessStatusCode)
            {
                continue;
            }

            var readerSettings = new OpenApiReaderSettings
            {
                HttpClient = authenticatedClient,
                LoadExternalRefs = true,
                BaseUrl = schemaUri,
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
                Register(readResult.Document);
            }
        }
    }

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

        if (document.Components?.Schemas is not { } schemas)
        {
            return;
        }

        foreach (var definition in schemas)
        {
            _schemas[definition.Key] = definition.Value;
        }

        Interlocked.Increment(ref _version);
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
