using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class JsonBackedYamlTypeConverter<T>(JsonTypeInfo<T> jsonTypeInfo) : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(T);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var json = (JsonElement)rootDeserializer(typeof(JsonElement))!;
        return json.Deserialize(jsonTypeInfo);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var json = JsonSerializer.SerializeToElement((T)value!, jsonTypeInfo);
        serializer(json, typeof(JsonElement));
    }
}
