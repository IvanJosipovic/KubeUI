using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using k8s;
using k8s.Models;
using SVCSystems.Text.Json.Yaml;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubernetesYamlSerializer : IKubernetesYamlSerializer
{
    private static readonly JsonTypeInfo<JsonElement> s_jsonElementTypeInfo = KubernetesJsonStaticContext.Default.JsonElement;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly JsonSerializerOptions _strictJsonOptions;
    private readonly YamlSerializerOptions _lenientYamlOptions = new()
    {
        ResolveMergeKeys = true,
        DuplicateKeyHandling = YamlDuplicateKeyHandling.LastWins,
    };
    private readonly YamlSerializerOptions _strictYamlOptions = new()
    {
        ResolveMergeKeys = true,
        DuplicateKeyHandling = YamlDuplicateKeyHandling.Reject,
    };

    public KubernetesYamlSerializer(JsonSerializerOptions jsonOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonOptions);
        _jsonOptions = jsonOptions;
        _strictJsonOptions = new JsonSerializerOptions(jsonOptions)
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        _strictJsonOptions.MakeReadOnly();
    }

    public string Serialize(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var document = GetSerializableDocument(value, _jsonOptions);
        return YamlSerializer.Serialize(document.Value, document.TypeInfo);
    }

    public string SerializeAll(IEnumerable<object> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        List<(object? Value, JsonTypeInfo TypeInfo)> documents = [];
        foreach (var value in values)
        {
            if (value is null)
            {
                continue;
            }

            documents.Add(GetSerializableDocument(value, _jsonOptions));
        }

        return YamlSerializer.SerializeDocuments(documents);
    }

    public T Deserialize<T>(string yaml, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        var typeInfo = GetJsonOptions(strict).GetTypeInfo<T>() ?? throw new JsonException($"Unable to get JsonTypeInfo for type '{typeof(T)}'.");

        return YamlSerializer.Deserialize(yaml, typeInfo, GetYamlOptions(strict))!;
    }

    public object? Deserialize(string yaml, Type type, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(type);

        return YamlSerializer.Deserialize(yaml, GetJsonOptions(strict).GetTypeInfo(type), GetYamlOptions(strict));
    }

    public List<IKubernetesObject<V1ObjectMeta>> LoadAllFromString(string yaml, Func<string, Type> resolveType, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(resolveType);

        var jsonOptions = GetJsonOptions(strict);
        List<IKubernetesObject<V1ObjectMeta>> resources = [];
        foreach (var document in YamlSerializer.DeserializeDocuments(yaml, s_jsonElementTypeInfo, GetYamlOptions(strict)))
        {
            if (document.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            if (document.ValueKind != JsonValueKind.Object
                || !document.TryGetProperty("apiVersion", out var apiVersion)
                || apiVersion.ValueKind != JsonValueKind.String
                || !document.TryGetProperty("kind", out var kind)
                || kind.ValueKind != JsonValueKind.String)
            {
                throw new FormatException("A Kubernetes document must include apiVersion and kind.");
            }

            var type = resolveType($"{apiVersion.GetString()}/{kind.GetString()}");
            
            resources.Add(JsonSerializer.Deserialize(document, jsonOptions.GetTypeInfo(type)) as IKubernetesObject<V1ObjectMeta>
                ?? throw new JsonException($"The resolved type '{type}' is not a Kubernetes resource."));
        }

        return resources;
    }

    public List<GenericKubernetesObject> LoadGenericResourcesFromString(string yaml, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        List<GenericKubernetesObject> resources = [];
        foreach (var document in YamlSerializer.DeserializeDocuments(
                     yaml,
                     KubernetesJsonStaticContext.Default.GenericKubernetesObject,
                     GetYamlOptions(strict)))
        {
            if (document is not null)
            {
                resources.Add(document);
            }
        }

        return resources;
    }

    private YamlSerializerOptions GetYamlOptions(bool strict) => strict ? _strictYamlOptions : _lenientYamlOptions;

    private static (object? Value, JsonTypeInfo TypeInfo) GetSerializableDocument(object value, JsonSerializerOptions options) => (value, options.GetTypeInfo(value.GetType()));

    private JsonSerializerOptions GetJsonOptions(bool strict) => strict ? _strictJsonOptions : _jsonOptions;
}
