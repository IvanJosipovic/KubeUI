using System.Reflection;
using System.Text;
using k8s;
using k8s.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.System.Text.Json;

namespace KubeUI.Kubernetes.Serialization;

/// <summary>
/// This is a utility class that helps you load objects from YAML files.
/// </summary>
public static class KubernetesYaml
{
    private static readonly object s_deserializerLockObject = new();
    private static readonly object s_serializerLockObject = new();

    private static DeserializerBuilder CommonDeserializerBuilder =>
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new IntOrStringYamlConverter())
            .WithTypeConverter(new ByteArrayStringYamlConverter())
            .WithTypeConverter(new ResourceQuantityYamlConverter())
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeConverter(new KubernetesDateTimeYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeOffsetYamlConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .WithAttemptingUnquotedStringTypeDeserialization();

    public static readonly IDeserializer StrictDeserializer =
        CommonDeserializerBuilder
            .WithDuplicateKeyChecking()
            .Build();

    public static readonly IDeserializer Deserializer =
        CommonDeserializerBuilder
            .IgnoreUnmatchedProperties()
            .Build();

    public static IDeserializer GetDeserializer(bool strict = false)
    {
        return strict ? StrictDeserializer : Deserializer;
    }

    public static readonly IValueSerializer Serializer =
        new SerializerBuilder()
            .DisableAliases()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)

            .WithTypeConverter(new IntOrStringYamlConverter())
            .WithTypeConverter(new ByteArrayStringYamlConverter())
            .WithTypeConverter(new ResourceQuantityYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeOffsetYamlConverter())

            .WithTypeConverter(new SystemTextJsonYamlTypeConverter(true))

            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .WithTypeInspector(x => new SortedTypeInspector(x))

            .WithEventEmitter(e => new StringQuotingEmitter(e))
            .WithEventEmitter(e => new FloatEmitter(e))
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .BuildValueSerializer();

    private static readonly IDictionary<string, Type> ModelTypeMap = typeof(KubernetesEntityAttribute).Assembly
        .GetTypes()
        .Where(t => t.GetCustomAttributes(typeof(KubernetesEntityAttribute), true).Any())
        .ToDictionary(
            t =>
            {
                var attr = t.GetCustomAttribute<KubernetesEntityAttribute>(true);
                var groupPrefix = string.IsNullOrEmpty(attr.Group) ? "" : $"{attr.Group}/";
                return $"{groupPrefix}{attr.ApiVersion}/{attr.Kind}";
            },
            t => t);

    private class ByteArrayStringYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (parser?.Current is Scalar scalar)
            {
                try
                {
                    if (string.IsNullOrEmpty(scalar.Value))
                    {
                        return null;
                    }

                    try
                    {
                        return Convert.FromBase64String(scalar.Value);
                    }
                    catch (FormatException ex)
                    {
                        throw new YamlException(scalar.Start, scalar.End, $"Invalid Base64 string: '{scalar.Value}'", ex);
                    }
                }
                finally
                {
                    parser.MoveNext();
                }
            }

            throw new InvalidOperationException(parser.Current?.ToString());
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value == null)
            {
                emitter.Emit(new Scalar(string.Empty));
                return;
            }

            var obj = (byte[])value;
            var encoded = Convert.ToBase64String(obj);
            emitter.Emit(new Scalar(encoded));
        }
    }

    /// <summary>
    /// Load a collection of objects from a stream asynchronously
    ///
    /// caller is responsible for closing the stream
    /// </summary>
    /// <param name="stream">
    /// The stream to load the objects from.
    /// </param>
    /// <param name="typeMap">
    /// A map from apiVersion/kind to Type. For example "v1/Pod" -> typeof(V1Pod). If null, a default mapping will
    /// be used.
    /// </param>
    /// <returns>collection of objects</returns>
    public static async Task<List<object>> LoadAllFromStreamAsync(Stream stream, IDictionary<string, Type> typeMap = null)
    {
        var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return LoadAllFromString(content, typeMap);
    }

    public static async Task<List<object>> LoadAllFromStreamAsync(Stream stream, IDictionary<string, Type> typeMap, bool strict)
    {
        var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return LoadAllFromString(content, typeMap, strict);
    }

    /// <summary>
    /// Load a collection of objects from a file asynchronously
    /// </summary>
    /// <param name="fileName">The name of the file to load from.</param>
    /// <param name="typeMap">
    /// A map from apiVersion/kind to Type. For example "v1/Pod" -> typeof(V1Pod). If null, a default mapping will
    /// be used.
    /// </param>
    /// <returns>collection of objects</returns>
    public static async Task<List<object>> LoadAllFromFileAsync(string fileName, IDictionary<string, Type> typeMap = null)
    {
        await using var fileStream = File.OpenRead(fileName);
        return await LoadAllFromStreamAsync(fileStream, typeMap).ConfigureAwait(false);
    }

    public static async Task<List<object>> LoadAllFromFileAsync(string fileName, IDictionary<string, Type> typeMap, bool strict)
    {
        await using var fileStream = File.OpenRead(fileName);
        return await LoadAllFromStreamAsync(fileStream, typeMap, strict).ConfigureAwait(false);
    }

    /// <summary>
    /// Load a collection of objects from a string
    /// </summary>
    /// <param name="content">
    /// The string to load the objects from.
    /// </param>
    /// <param name="typeMap">
    /// A map from apiVersion/kind to Type. For example "v1/Pod" -> typeof(V1Pod). If null, a default mapping will
    /// be used.
    /// </param>
    /// <returns>collection of objects</returns>
    public static List<object> LoadAllFromString(string content, IDictionary<string, Type> typeMap = null, bool strict = false)
    {
        var mergedTypeMap = new Dictionary<string, Type>(ModelTypeMap);
        // merge in KVPs from typeMap, overriding any in ModelTypeMap
        typeMap?.ToList().ForEach(x => mergedTypeMap[x.Key] = x.Value);

        var types = new List<Type>();
        var parser = new MergingParser(new Parser(new StringReader(content)));
        parser.Consume<StreamStart>();
        while (parser.Accept<DocumentStart>(out _))
        {
            Dictionary<object, object> dict;
            lock (s_deserializerLockObject)
            {
                dict = GetDeserializer(strict).Deserialize<Dictionary<object, object>>(parser);
            }
            types.Add(mergedTypeMap[dict["apiVersion"] + "/" + dict["kind"]]);
        }

        parser = new MergingParser(new Parser(new StringReader(content)));
        parser.Consume<StreamStart>();
        var ix = 0;
        var results = new List<object>();
        while (parser.Accept<DocumentStart>(out _))
        {
            var objType = types[ix++];
            object obj;
            lock (s_deserializerLockObject)
            {
                obj = GetDeserializer(strict).Deserialize(parser, objType);
            }

            results.Add(obj);
        }

        return results;
    }

    public static async Task<T> LoadFromStreamAsync<T>(Stream stream, bool strict = false)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return Deserialize<T>(content, strict);
    }

    public static async Task<T> LoadFromFileAsync<T>(string file, bool strict = false)
    {
        await using var fs = File.OpenRead(file);
        return await LoadFromStreamAsync<T>(fs, strict).ConfigureAwait(false);
    }

    public static TValue Deserialize<TValue>(string yaml, bool strict = false)
    {
        using var reader = new StringReader(yaml);
        lock (s_deserializerLockObject)
        {
            return GetDeserializer(strict).Deserialize<TValue>(new MergingParser(new Parser(reader)));
        }
    }

    public static TValue Deserialize<TValue>(Stream yaml, bool strict = false)
    {
        using var reader = new StreamReader(yaml);
        lock (s_deserializerLockObject)
        {
            return GetDeserializer(strict).Deserialize<TValue>(new MergingParser(new Parser(reader)));
        }
    }

    public static object? Deserialize(string yaml, Type type, bool strict = false)
    {
        using var reader = new StringReader(yaml);
        lock (s_deserializerLockObject)
        {
            return GetDeserializer(strict).Deserialize(new MergingParser(new Parser(reader)), type);
        }
    }

    public static object? Deserialize(IParser parser, Type type, bool strict = false)
    {
        lock (s_deserializerLockObject)
        {
            return GetDeserializer(strict).Deserialize(parser, type);
        }
    }

    public static object? Deserialize(IParser parser, bool strict = false)
    {
        lock (s_deserializerLockObject)
        {
            return GetDeserializer(strict).Deserialize(parser);
        }
    }

    public static string SerializeAll(IEnumerable<object> values)
    {
        if (values == null)
        {
            return "";
        }

        var stringBuilder = new StringBuilder();
        var writer = new StringWriter(stringBuilder);
        var emitter = new Emitter(writer);

        emitter.Emit(new StreamStart());

        foreach (var value in values)
        {
            if (value != null)
            {
                emitter.Emit(new DocumentStart());
                lock (s_serializerLockObject)
                {
                    Serializer.SerializeValue(emitter, value, value.GetType());
                }
                emitter.Emit(new DocumentEnd(true));
            }
        }

        return stringBuilder.ToString();
    }

    public static string Serialize(object value)
    {
        if (value == null)
        {
            return "";
        }

        var stringBuilder = new StringBuilder();
        var writer = new StringWriter(stringBuilder);
        var emitter = new Emitter(writer);

        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());
        lock (s_serializerLockObject)
        {
            Serializer.SerializeValue(emitter, value, value.GetType());
        }

        return stringBuilder.ToString();
    }
}

