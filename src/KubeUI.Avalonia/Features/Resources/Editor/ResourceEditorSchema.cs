using System.Text.Json;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using Microsoft.OpenApi;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public enum ResourceEditorValueKind
{
    Object,
    Array,
    Map,
    String,
    Number,
    Boolean,
    Enum,
    Unknown
}

public sealed class ResourceEditorSchemaNode
{
    private static readonly ConditionalWeakTable<KubernetesOpenApiSchemaCatalog, SchemaCache> Caches = [];
    public ResourceEditorSchemaNode(
        string name,
        ResourceEditorValueKind valueKind,
        IOpenApiSchema? schema,
        IReadOnlyDictionary<string, ResourceEditorSchemaNode> properties,
        ResourceEditorSchemaNode? items,
        IReadOnlyList<string> enumValues,
        IReadOnlySet<string> required,
        string? description,
        bool isYamlEditor = false,
        string? format = null)
    {
        Name = name;
        ValueKind = valueKind;
        Schema = schema;
        Properties = properties;
        Items = items;
        EnumValues = enumValues;
        Required = required;
        Description = description;
        IsYamlEditor = isYamlEditor;
        Format = format;
    }

    public string Name { get; }
    public ResourceEditorValueKind ValueKind { get; }
    public IOpenApiSchema? Schema { get; }
    public IReadOnlyDictionary<string, ResourceEditorSchemaNode> Properties { get; }
    public ResourceEditorSchemaNode? Items { get; }
    public IReadOnlyList<string> EnumValues { get; }
    public IReadOnlySet<string> Required { get; }
    public string? Description { get; }
    public bool IsYamlEditor { get; }
    public string? Format { get; }
    public bool IsRequired => Required.Contains(Name);

    public static ResourceEditorSchemaNode CreateRoot(
        GroupApiVersionKind kind,
        KubernetesOpenApiSchemaCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var cache = Caches.GetOrCreateValue(catalog);
        var key = $"{kind.Group}/{kind.ApiVersion}/{kind.Kind}";
        lock (cache.Gate)
        {
            if (cache.Version != catalog.Version)
            {
                cache.Version = catalog.Version;
                cache.Roots.Clear();
            }

            if (cache.Roots.TryGetValue(key, out var root))
                return root;

            root = Create(kind.Kind, catalog.GetSchema(kind), catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));
            cache.Roots[key] = root;
            return root;
        }
    }

    private sealed class SchemaCache
    {
        public object Gate { get; } = new();
        public long Version { get; set; } = -1;
        public Dictionary<string, ResourceEditorSchemaNode> Roots { get; } = new(StringComparer.Ordinal);
    }

    private static ResourceEditorSchemaNode Create(
        string name,
        IOpenApiSchema? source,
        KubernetesOpenApiSchemaCatalog catalog,
        HashSet<IOpenApiSchema> active)
    {
        var schema = catalog.ExpandReferences(source);
        if (schema is null || !active.Add(schema))
        {
            return new(name, ResourceEditorValueKind.Unknown, schema,
                new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal), null, [],
                new HashSet<string>(StringComparer.Ordinal), source?.Description, format: source?.Format);
        }

        try
        {
            var variants = GetVariants(schema, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance)).ToArray();
            var properties = new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal);
            var required = new HashSet<string>(StringComparer.Ordinal);
            foreach (var variant in variants)
            {
                if (variant.Required is { } requiredNames)
                    foreach (var requiredName in requiredNames)
                        required.Add(requiredName);

                foreach (var property in variant.Properties ?? new Dictionary<string, IOpenApiSchema>())
                    properties[property.Key] = Create(property.Key, property.Value, catalog, active);
            }

            var itemSchema = variants.Select(static x => x.Items).FirstOrDefault(static x => x is not null);
            var additionalPropertySchema = variants
                .Where(static x => x.AdditionalPropertiesAllowed)
                .Select(static x => x.AdditionalProperties)
                .FirstOrDefault(static x => x is not null);
            var enumValues = variants.SelectMany(static x => x.Enum ?? new List<System.Text.Json.Nodes.JsonNode>())
                .Select(static value => value?.ToString() ?? string.Empty)
                .Where(static value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var isYamlEditor = variants.Any(IsYamlSchema);
            var valueKind = GetValueKind(variants, properties.Count, itemSchema, enumValues.Length > 0);
            var childSchema = valueKind == ResourceEditorValueKind.Map ? additionalPropertySchema : itemSchema;
            return new(name, valueKind, schema, properties,
                childSchema is null ? null : Create(name, childSchema, catalog, active),
                enumValues, required,
                variants.Select(static x => x.Description).FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x)),
                isYamlEditor,
                variants.Select(static x => x.Format).FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x)));
        }
        finally
        {
            active.Remove(schema);
        }
    }

    private static bool IsYamlSchema(IOpenApiSchema schema)
    {
        if (schema.Extensions?.ContainsKey("x-kubernetes-preserve-unknown-fields") == true)
            return true;

        if (schema.UnrecognizedKeywords?.ContainsKey("x-kubernetes-preserve-unknown-fields") == true)
            return true;

        return schema.Type == JsonSchemaType.Object
            && (schema.Properties is null or { Count: 0 })
            && schema.AdditionalPropertiesAllowed
            && schema.AdditionalProperties is null;
    }

    private static ResourceEditorValueKind GetValueKind(
        IReadOnlyList<IOpenApiSchema> variants,
        int propertyCount,
        IOpenApiSchema? items,
        bool hasEnum)
    {
        if (propertyCount > 0)
            return ResourceEditorValueKind.Object;
        if (items is not null)
            return ResourceEditorValueKind.Array;
        if (hasEnum)
            return ResourceEditorValueKind.Enum;

        return variants.Select(static x => x.Type).FirstOrDefault(static x => x is not null) switch
        {
            JsonSchemaType.String => ResourceEditorValueKind.String,
            JsonSchemaType.Integer or JsonSchemaType.Number => ResourceEditorValueKind.Number,
            JsonSchemaType.Boolean => ResourceEditorValueKind.Boolean,
            JsonSchemaType.Object => ResourceEditorValueKind.Map,
            _ => ResourceEditorValueKind.Unknown
        };
    }

    private static IEnumerable<IOpenApiSchema> GetVariants(
        IOpenApiSchema schema,
        KubernetesOpenApiSchemaCatalog catalog,
        HashSet<IOpenApiSchema> visited)
    {
        if (!visited.Add(schema))
            yield break;

        yield return schema;
        foreach (var composed in schema.AllOf ?? [])
        {
            var resolved = catalog.ExpandReferences(composed);
            if (resolved is not null)
                foreach (var variant in GetVariants(resolved, catalog, visited))
                    yield return variant;
        }
        foreach (var composed in schema.OneOf ?? [])
        {
            var resolved = catalog.ExpandReferences(composed);
            if (resolved is not null)
                foreach (var variant in GetVariants(resolved, catalog, visited))
                    yield return variant;
        }
        foreach (var composed in schema.AnyOf ?? [])
        {
            var resolved = catalog.ExpandReferences(composed);
            if (resolved is not null)
                foreach (var variant in GetVariants(resolved, catalog, visited))
                    yield return variant;
        }
    }
}

public sealed record ResourceEditorValidationError(string Path, string Message);
public sealed record ResourceEditorEnumOption(string? Value, string DisplayName);

public sealed class ResourceEditorDocument
{
    public ResourceEditorDocument(JsonObject root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
        OriginalJson = root.ToJsonString();
    }

    public JsonObject Root { get; }
    public string OriginalJson { get; }
    public bool IsDirty => !string.Equals(OriginalJson, Root.ToJsonString(), StringComparison.Ordinal);

    public void Reset()
    {
        var original = JsonNode.Parse(OriginalJson)?.AsObject()
            ?? throw new InvalidOperationException("Original editor document is invalid.");
        Root.Clear();
        foreach (var property in original)
            Root[property.Key] = property.Value?.DeepClone();
    }

    public static ResourceEditorDocument Parse(string json)
    {
        var node = JsonNode.Parse(json);
        return node is JsonObject root
            ? new(root)
            : throw new JsonException("Resource JSON must be an object.");
    }
}
