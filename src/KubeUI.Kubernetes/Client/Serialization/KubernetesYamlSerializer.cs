using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubernetesYamlSerializer : IKubernetesYamlSerializer, IDisposable
{
    private readonly ThreadLocal<IDeserializer> _deserializer;
    private readonly ThreadLocal<IDeserializer> _strictDeserializer;
    private readonly ThreadLocal<IValueSerializer> _serializer;

    public KubernetesYamlSerializer(bool allowReflection)
    {
        _deserializer = new(() => KubernetesYaml.CreateDeserializer(allowReflection, strict: false));
        _strictDeserializer = new(() => KubernetesYaml.CreateDeserializer(allowReflection, strict: true));
        _serializer = new(() => KubernetesYaml.CreateSerializer(allowReflection));
    }

    public string Serialize(object value) => KubernetesYaml.Serialize(value, _serializer.Value!);

    public T Deserialize<T>(string yaml, bool strict = false)
    {
        using var reader = new StringReader(yaml);
        return GetDeserializer(strict).Deserialize<T>(new MergingParser(new Parser(reader)));
    }

    public object? Deserialize(string yaml, Type type, bool strict = false)
    {
        using var reader = new StringReader(yaml);
        return GetDeserializer(strict).Deserialize(new MergingParser(new Parser(reader)), type);
    }

    public object? Deserialize(IParser parser, Type type, bool strict = false) => GetDeserializer(strict).Deserialize(parser, type);

    public List<object> LoadAllFromString(string yaml, Func<string, Type> resolveType, bool strict = false) =>
        KubernetesYaml.LoadAllFromStringCore(yaml, resolveType, strict, GetDeserializer(strict));

    private IDeserializer GetDeserializer(bool strict) => (strict ? _strictDeserializer.Value : _deserializer.Value)!;

    public void Dispose()
    {
        _deserializer.Dispose();
        _strictDeserializer.Dispose();
        _serializer.Dispose();
    }
}
