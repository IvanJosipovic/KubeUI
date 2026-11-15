using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using KubernetesClient.Informer.Client;
using Microsoft.OpenApi;

namespace KubeUI.Kubernetes.Resources.Editor;

/// <summary>
/// Describes a single node of the Kubernetes OpenAPI schema used to render a resource editor.
/// </summary>
public sealed class ResourceEditorSchemaNode
{
    private static readonly ConditionalWeakTable<KubernetesOpenApiSchemaCatalog, SchemaCache> Caches = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceEditorSchemaNode"/> class.
    /// </summary>
    /// <param name="name">The field name this node represents.</param>
    /// <param name="valueKind">The JSON shape this node represents.</param>
    /// <param name="schema">The OpenAPI schema backing this node.</param>
    /// <param name="properties">The child properties of this node.</param>
    /// <param name="items">The schema of array items or map values.</param>
    /// <param name="enumValues">The allowed values when this node is an enum.</param>
    /// <param name="required">The names of the required child properties.</param>
    /// <param name="description">The schema description for this node.</param>
    /// <param name="isYamlEditor">Whether the value is edited as free-form YAML.</param>
    /// <param name="format">The schema format for this node.</param>
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

    /// <summary>Gets the field name this node represents.</summary>
    public string Name { get; }

    /// <summary>Gets the JSON shape this node represents.</summary>
    public ResourceEditorValueKind ValueKind { get; }

    /// <summary>Gets the OpenAPI schema backing this node.</summary>
    public IOpenApiSchema? Schema { get; }

    /// <summary>Gets the child properties of this node.</summary>
    public IReadOnlyDictionary<string, ResourceEditorSchemaNode> Properties { get; }

    /// <summary>Gets the schema of array items or map values.</summary>
    public ResourceEditorSchemaNode? Items { get; }

    /// <summary>Gets the allowed values when this node is an enum.</summary>
    public IReadOnlyList<string> EnumValues { get; }

    /// <summary>Gets the names of the required child properties.</summary>
    public IReadOnlySet<string> Required { get; }

    /// <summary>Gets the schema description for this node.</summary>
    public string? Description { get; }

    /// <summary>Gets a value indicating whether the value is edited as free-form YAML.</summary>
    public bool IsYamlEditor { get; }

    /// <summary>Gets the schema format for this node.</summary>
    public string? Format { get; }

    /// <summary>
    /// Gets the root schema for a Kubernetes resource kind.
    /// </summary>
    /// <param name="kind">The resource kind to resolve.</param>
    /// <param name="catalog">The catalog holding the published OpenAPI schemas.</param>
    /// <returns>The root schema node.</returns>
    /// <exception cref="InvalidOperationException">The catalog does not contain a schema for the kind.</exception>
    public static ResourceEditorSchemaNode CreateRoot(
        GroupApiVersionKind kind,
        KubernetesOpenApiSchemaCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var cache = Caches.GetOrCreateValue(catalog);
        var key = $"{kind.Group}/{kind.ApiVersion}/{kind.Kind}";
        lock (cache.Gate)
        {
            var source = catalog.GetSchema(kind);
            if (source is null)
                throw new InvalidOperationException(
                    $"OpenAPI schema for {kind.ApiVersion}/{kind.Kind} is unavailable. Reload cluster schemas before opening the resource editor.");

            // The cache is keyed on the resolved schema instance. Registering an unrelated schema
            // leaves this instance untouched, so the cached tree stays valid instead of being
            // rebuilt under the lock for every schema that arrives.
            if (cache.Roots.TryGetValue(key, out var cached) && ReferenceEquals(cached.Source, source))
                return cached.Root;

            var root = Create(kind.Kind, source, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));
            cache.Roots[key] = new CachedRoot(source, root);
            return root;
        }
    }

    private sealed class SchemaCache
    {
        public object Gate { get; } = new();
        public Dictionary<string, CachedRoot> Roots { get; } = new(StringComparer.Ordinal);
    }

    private sealed record CachedRoot(IOpenApiSchema Source, ResourceEditorSchemaNode Root);

    private static ResourceEditorSchemaNode Create(
        string name,
        IOpenApiSchema? source,
        KubernetesOpenApiSchemaCatalog catalog,
        HashSet<IOpenApiSchema> active)
    {
        var schema = catalog.ExpandReferences(source);
        if (schema is null || !active.Add(schema))
        {
            return new(name, ResourceEditorValueKind.UnknownKind, schema,
                new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal), null, [],
                new HashSet<string>(StringComparer.Ordinal), source?.Description, format: source?.Format);
        }

        try
        {
            // `allOf` describes a conjunction: every branch contributes properties and required
            // names. `oneOf` and `anyOf` describe alternatives: their properties are offered as
            // available fields, but their required names are not merged, because only one branch
            // may be satisfied. Merging required names across alternatives would demand
            // mutually exclusive fields at the same time.
            var conjuncts = GetConjuncts(schema, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));
            var alternatives = GetAlternatives(schema, catalog, new HashSet<IOpenApiSchema>(ReferenceEqualityComparer.Instance));

            var properties = new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal);
            var required = new HashSet<string>(StringComparer.Ordinal);
            foreach (var variant in conjuncts)
            {
                if (variant.Required is { } requiredNames)
                    foreach (var requiredName in requiredNames)
                        required.Add(requiredName);
            }

            foreach (var variant in conjuncts.Concat(alternatives))
            {
                foreach (var property in variant.Properties ?? new Dictionary<string, IOpenApiSchema>())
                    properties[property.Key] = Create(property.Key, property.Value, catalog, active);
            }

            var itemSchema = conjuncts.Select(static x => x.Items).FirstOrDefault(static x => x is not null);
            var additionalPropertySchema = conjuncts
                .Where(static x => x.AdditionalPropertiesAllowed)
                .Select(static x => x.AdditionalProperties)
                .FirstOrDefault(static x => x is not null);
            var enumValues = conjuncts.Concat(alternatives)
                .SelectMany(static x => x.Enum ?? new List<JsonNode>())
                .Select(static value => value?.ToString() ?? string.Empty)
                .Where(static value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var isYamlEditor = conjuncts.Concat(alternatives).Any(IsYamlSchema);
            var valueKind = GetValueKind(conjuncts, alternatives, properties.Count, itemSchema, enumValues.Length > 0);
            var childSchema = valueKind == ResourceEditorValueKind.MapKind ? additionalPropertySchema : itemSchema;
            return new(name, valueKind, schema, properties,
                childSchema is null ? null : Create(name, childSchema, catalog, active),
                enumValues, required,
                conjuncts.Select(static x => x.Description).FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x)),
                isYamlEditor,
                conjuncts.Select(static x => x.Format).FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x)));
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
        IReadOnlyList<IOpenApiSchema> conjuncts,
        IReadOnlyList<IOpenApiSchema> alternatives,
        int propertyCount,
        IOpenApiSchema? items,
        bool hasEnum)
    {
        if (propertyCount > 0)
            return ResourceEditorValueKind.ObjectKind;
        if (items is not null)
            return ResourceEditorValueKind.ArrayKind;
        if (hasEnum)
            return ResourceEditorValueKind.EnumKind;

        return conjuncts
            .Concat(alternatives)
            .Select(static x => x.Type)
            .FirstOrDefault(static x => x is not null) switch
        {
            JsonSchemaType.String => ResourceEditorValueKind.StringKind,
            JsonSchemaType.Integer or JsonSchemaType.Number => ResourceEditorValueKind.NumberKind,
            JsonSchemaType.Boolean => ResourceEditorValueKind.BooleanKind,
            JsonSchemaType.Object => ResourceEditorValueKind.MapKind,
            _ => ResourceEditorValueKind.UnknownKind
        };
    }

    /// <summary>
    /// Gets the schema and every <c>allOf</c> branch that must be satisfied together.
    /// </summary>
    private static List<IOpenApiSchema> GetConjuncts(
        IOpenApiSchema schema,
        KubernetesOpenApiSchemaCatalog catalog,
        HashSet<IOpenApiSchema> visited)
    {
        if (!visited.Add(schema))
            return [];

        var result = new List<IOpenApiSchema> { schema };
        foreach (var composed in schema.AllOf ?? [])
        {
            var resolved = catalog.ExpandReferences(composed);
            if (resolved is not null)
                result.AddRange(GetConjuncts(resolved, catalog, visited));
        }

        return result;
    }

    /// <summary>
    /// Gets the <c>oneOf</c> and <c>anyOf</c> branches that describe alternative shapes.
    /// </summary>
    private static List<IOpenApiSchema> GetAlternatives(
        IOpenApiSchema schema,
        KubernetesOpenApiSchemaCatalog catalog,
        HashSet<IOpenApiSchema> visited)
    {
        if (!visited.Add(schema))
            return [];

        var result = new List<IOpenApiSchema>();
        foreach (var composed in (schema.OneOf ?? []).Concat(schema.AnyOf ?? []))
        {
            var resolved = catalog.ExpandReferences(composed);
            if (resolved is not null)
            {
                result.Add(resolved);
                result.AddRange(GetConjuncts(resolved, catalog, visited));
            }
        }

        return result;
    }
}
