using KubeUI.Kubernetes.Client;
using KubernetesClient.Informer.Client;
using Microsoft.OpenApi;

namespace KubeUI.Kubernetes;

/// <summary>
/// Stores OpenAPI schemas published by a Kubernetes API server.
/// </summary>
public sealed class KubernetesOpenApiSchemaCatalog
{
    private IReadOnlyDictionary<string, IOpenApiSchema> _schemas =
        new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
    private readonly Lock _gate = new();
    private long _version;

    public int Count => Volatile.Read(ref _schemas).Count;

    public long Version => Interlocked.Read(ref _version);

    internal void Replace(IEnumerable<OpenApiDocument> documents)
    {
        var schemas = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            AddSchemas(document, schemas);
        }

        lock (_gate)
        {
            Volatile.Write(ref _schemas, schemas);
            Interlocked.Increment(ref _version);
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
            var schemas = new Dictionary<string, IOpenApiSchema>(Volatile.Read(ref _schemas), StringComparer.Ordinal);
            AddSchemas(document, schemas);
            Volatile.Write(ref _schemas, schemas);
            Interlocked.Increment(ref _version);
        }
    }

    private static void AddSchemas(OpenApiDocument document, IDictionary<string, IOpenApiSchema> targetSchemas)
    {
        if (document.Components?.Schemas is not { } definitions)
        {
            return;
        }

        foreach (var definition in definitions)
        {
            targetSchemas[definition.Key] = definition.Value;
        }
    }

    public IOpenApiSchema? GetSchema(GroupApiVersionKind kind)
    {
        var schemas = Volatile.Read(ref _schemas);
        foreach (var name in GetSchemaNames(kind))
        {
            if (schemas.TryGetValue(name, out var schema))
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

        var schemas = Volatile.Read(ref _schemas);
        if (schemas.TryGetValue(typeName, out var exactSchema))
        {
            return exactSchema;
        }

        foreach (var pair in schemas)
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
