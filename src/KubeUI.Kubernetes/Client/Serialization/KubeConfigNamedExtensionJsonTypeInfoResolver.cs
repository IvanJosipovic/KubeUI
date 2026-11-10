using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubeConfigNamedExtensionJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private static readonly JsonConverter<NamedExtension> s_converter = new KubeConfigNamedExtensionJsonConverter();

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) =>
        type == typeof(NamedExtension)
            ? JsonMetadataServices.CreateValueInfo<NamedExtension>(options, s_converter)
            : null;
}

internal sealed class KubeConfigNamedExtensionJsonConverter : JsonConverter<NamedExtension>
{
    public override NamedExtension? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("A kubeconfig named extension must be a JSON object.");
        }

        var root = document.RootElement;
        return new NamedExtension
        {
            Name = root.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null,
            Extension = root.TryGetProperty("extension", out var extension)
                ? KubeConfigExtensionValueCodec.Read(extension)
                : null,
        };
    }

    public override void Write(Utf8JsonWriter writer, NamedExtension value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WritePropertyName("extension");
        KubeConfigExtensionValueCodec.Write(writer, (object?)value.Extension);
        writer.WriteEndObject();
    }
}
