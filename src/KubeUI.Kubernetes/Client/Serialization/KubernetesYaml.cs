using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.Json;
using k8s;
using k8s.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

namespace KubeUI.Kubernetes.Serialization;

/// <summary>
/// This is a utility class that helps you load objects from YAML files.
/// </summary>
public static class KubernetesYaml
{
    private static readonly object s_deserializerLockObject = new();
    private static readonly object s_serializerLockObject = new();
    private static bool s_useStaticContext;

    private static readonly Lazy<IDeserializer> s_staticStrictDeserializer = new(() =>
        CommonDeserializerBuilder.WithDuplicateKeyChecking().Build());
    private static readonly Lazy<IDeserializer> s_staticDeserializer = new(() =>
        CommonDeserializerBuilder.IgnoreUnmatchedProperties().Build());
    private static readonly Lazy<IValueSerializer> s_staticSerializer = new(CreateStaticSerializer);

    private static readonly Lazy<IDeserializer> s_reflectionStrictDeserializer = new(() =>
        CommonReflectionDeserializerBuilder.WithDuplicateKeyChecking().Build());
    private static readonly Lazy<IDeserializer> s_reflectionDeserializer = new(() =>
        CommonReflectionDeserializerBuilder.IgnoreUnmatchedProperties().Build());
    private static readonly Lazy<IValueSerializer> s_reflectionSerializer = new(CreateReflectionSerializer);

    private static readonly KubernetesYamlStaticContext s_staticContext = new KubernetesYamlRuntimeContext();

    private static bool IsStaticStringDictionary(Type type)
    {
        return type == typeof(IDictionary<string, ResourceQuantity>) || type == typeof(Dictionary<string, ResourceQuantity>)
            || type == typeof(IDictionary<string, V1beta1Counter>) || type == typeof(Dictionary<string, V1beta1Counter>)
            || type == typeof(IDictionary<string, V1beta1DeviceAttribute>) || type == typeof(Dictionary<string, V1beta1DeviceAttribute>)
            || type == typeof(IDictionary<string, V1beta1DeviceCapacity>) || type == typeof(Dictionary<string, V1beta1DeviceCapacity>)
            || type == typeof(IDictionary<string, V1beta2Counter>) || type == typeof(Dictionary<string, V1beta2Counter>)
            || type == typeof(IDictionary<string, V1beta2DeviceAttribute>) || type == typeof(Dictionary<string, V1beta2DeviceAttribute>)
            || type == typeof(IDictionary<string, V1beta2DeviceCapacity>) || type == typeof(Dictionary<string, V1beta2DeviceCapacity>)
            || type == typeof(IDictionary<string, V1Counter>) || type == typeof(Dictionary<string, V1Counter>)
            || type == typeof(IDictionary<string, V1DeviceAttribute>) || type == typeof(Dictionary<string, V1DeviceAttribute>)
            || type == typeof(IDictionary<string, V1DeviceCapacity>) || type == typeof(Dictionary<string, V1DeviceCapacity>)
            || type == typeof(IDictionary<string, V1JSONSchemaProps>) || type == typeof(Dictionary<string, V1JSONSchemaProps>)
            || type == typeof(IDictionary<string, byte[]>) || type == typeof(Dictionary<string, byte[]>)
            || type == typeof(IDictionary<string, IList<string>>) || type == typeof(Dictionary<string, IList<string>>)
            || type == typeof(IDictionary<string, DateTime?>) || type == typeof(Dictionary<string, DateTime?>)
            || type == typeof(IDictionary<string, object>) || type == typeof(Dictionary<string, object>)
            || type == typeof(IDictionary<string, string>) || type == typeof(Dictionary<string, string>);
    }

    private static object? CreateStaticStringDictionary(Type type)
    {
        if (type == typeof(IDictionary<string, ResourceQuantity>) || type == typeof(Dictionary<string, ResourceQuantity>)) return new Dictionary<string, ResourceQuantity>();
        if (type == typeof(IDictionary<string, V1beta1Counter>) || type == typeof(Dictionary<string, V1beta1Counter>)) return new Dictionary<string, V1beta1Counter>();
        if (type == typeof(IDictionary<string, V1beta1DeviceAttribute>) || type == typeof(Dictionary<string, V1beta1DeviceAttribute>)) return new Dictionary<string, V1beta1DeviceAttribute>();
        if (type == typeof(IDictionary<string, V1beta1DeviceCapacity>) || type == typeof(Dictionary<string, V1beta1DeviceCapacity>)) return new Dictionary<string, V1beta1DeviceCapacity>();
        if (type == typeof(IDictionary<string, V1beta2Counter>) || type == typeof(Dictionary<string, V1beta2Counter>)) return new Dictionary<string, V1beta2Counter>();
        if (type == typeof(IDictionary<string, V1beta2DeviceAttribute>) || type == typeof(Dictionary<string, V1beta2DeviceAttribute>)) return new Dictionary<string, V1beta2DeviceAttribute>();
        if (type == typeof(IDictionary<string, V1beta2DeviceCapacity>) || type == typeof(Dictionary<string, V1beta2DeviceCapacity>)) return new Dictionary<string, V1beta2DeviceCapacity>();
        if (type == typeof(IDictionary<string, V1Counter>) || type == typeof(Dictionary<string, V1Counter>)) return new Dictionary<string, V1Counter>();
        if (type == typeof(IDictionary<string, V1DeviceAttribute>) || type == typeof(Dictionary<string, V1DeviceAttribute>)) return new Dictionary<string, V1DeviceAttribute>();
        if (type == typeof(IDictionary<string, V1DeviceCapacity>) || type == typeof(Dictionary<string, V1DeviceCapacity>)) return new Dictionary<string, V1DeviceCapacity>();
        if (type == typeof(IDictionary<string, V1JSONSchemaProps>) || type == typeof(Dictionary<string, V1JSONSchemaProps>)) return new Dictionary<string, V1JSONSchemaProps>();
        if (type == typeof(IDictionary<string, byte[]>) || type == typeof(Dictionary<string, byte[]>)) return new Dictionary<string, byte[]>();
        if (type == typeof(IDictionary<string, IList<string>>) || type == typeof(Dictionary<string, IList<string>>)) return new Dictionary<string, IList<string>>();
        if (type == typeof(IDictionary<string, DateTime?>) || type == typeof(Dictionary<string, DateTime?>)) return new Dictionary<string, DateTime?>();
        if (type == typeof(IDictionary<string, object>) || type == typeof(Dictionary<string, object>)) return new Dictionary<string, object>();
        if (type == typeof(IDictionary<string, string>) || type == typeof(Dictionary<string, string>)) return new Dictionary<string, string>();
        return null;
    }

    private static StaticDeserializerBuilder CommonDeserializerBuilder =>
        new StaticDeserializerBuilder(s_staticContext)
            .AddSystemTextJson()
            .WithTypeConverter(new JsonBackedYamlTypeConverter<IntOrString>(KubernetesJsonStaticContext.Default.IntOrString))
            .WithTypeConverter(new ByteArrayStringYamlConverter())
            .WithTypeConverter(new KubernetesByteArrayDictionaryYamlConverter())
            .WithTypeConverter(new KubernetesResourceQuantityYamlConverter())
            .WithTypeConverter(new KubernetesResourceQuantityDictionaryYamlConverter())
            .WithTypeConverter(new KubernetesStringDictionaryYamlConverter())
            .WithTypeConverter(new GenericKubernetesObjectYamlConverter())
            .WithTypeConverter(new KubeConfigNamedExtensionYamlConverter())
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1ContainerRestartRuleOnExitCodes>(KubernetesJsonStaticContext.Default.V1ContainerRestartRuleOnExitCodes))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1LinuxContainerUser>(KubernetesJsonStaticContext.Default.V1LinuxContainerUser))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1ListMeta>(KubernetesJsonStaticContext.Default.V1ListMeta))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1PodDisruptionBudgetStatus>(KubernetesJsonStaticContext.Default.V1PodDisruptionBudgetStatus))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1PodSecurityContext>(KubernetesJsonStaticContext.Default.V1PodSecurityContext))
            .WithTypeConverter(new KubernetesDateTimeYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeOffsetYamlConverter())
            .WithAttemptingUnquotedStringTypeDeserialization();

    private static DeserializerBuilder CommonReflectionDeserializerBuilder =>
        new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new IntOrStringYamlConverter())
            .WithTypeConverter(new ByteArrayStringYamlConverter())
            .WithTypeConverter(new ResourceQuantityYamlConverter())
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeConverter(new KubernetesDateTimeYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeOffsetYamlConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .WithAttemptingUnquotedStringTypeDeserialization();

    /// <summary>
    /// Selects generated static YAML metadata when <see langword="true"/>; defaults to reflection.
    /// </summary>
    public static bool UseStaticContext
    {
        get => Volatile.Read(ref s_useStaticContext);
        set => Volatile.Write(ref s_useStaticContext, value);
    }

    public static IDeserializer StrictDeserializer => UseStaticContext
        ? s_staticStrictDeserializer.Value
        : s_reflectionStrictDeserializer.Value;

    public static IDeserializer Deserializer => UseStaticContext
        ? s_staticDeserializer.Value
        : s_reflectionDeserializer.Value;

    public static IDeserializer GetDeserializer(bool strict = false)
    {
        return strict ? StrictDeserializer : Deserializer;
    }

    public static IValueSerializer Serializer => UseStaticContext
        ? s_staticSerializer.Value
        : s_reflectionSerializer.Value;

    private static IValueSerializer CreateStaticSerializer() =>
        new StaticSerializerBuilder(s_staticContext)
            .AddSystemTextJson()
            .DisableAliases()

            .WithTypeConverter(new JsonBackedYamlTypeConverter<IntOrString>(KubernetesJsonStaticContext.Default.IntOrString))
            .WithTypeConverter(new ByteArrayStringYamlConverter())
            .WithTypeConverter(new KubernetesByteArrayDictionaryYamlConverter())
            .WithTypeConverter(new KubernetesResourceQuantityYamlConverter())
            .WithTypeConverter(new KubernetesResourceQuantityDictionaryYamlConverter())
            .WithTypeConverter(new KubernetesStringDictionaryYamlConverter())
            .WithTypeConverter(new GenericKubernetesObjectYamlConverter())
            .WithTypeConverter(new KubeConfigNamedExtensionYamlConverter())
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1ContainerRestartRuleOnExitCodes>(KubernetesJsonStaticContext.Default.V1ContainerRestartRuleOnExitCodes))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1LinuxContainerUser>(KubernetesJsonStaticContext.Default.V1LinuxContainerUser))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1ListMeta>(KubernetesJsonStaticContext.Default.V1ListMeta))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1PodDisruptionBudgetStatus>(KubernetesJsonStaticContext.Default.V1PodDisruptionBudgetStatus))
            .WithTypeConverter(new JsonBackedYamlTypeConverter<V1PodSecurityContext>(KubernetesJsonStaticContext.Default.V1PodSecurityContext))
            .WithTypeConverter(new KubernetesDateTimeYamlConverter())
            .WithTypeConverter(new KubernetesDateTimeOffsetYamlConverter())

            .WithTypeInspector(x => new SortedTypeInspector(x))

            .WithEventEmitter(e => new StringQuotingEmitter(e))
            .WithEventEmitter(e => new FloatEmitter(e))
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .BuildValueSerializer();

    private static IValueSerializer CreateReflectionSerializer() =>
        new SerializerBuilder()
            .DisableAliases()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
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

    private sealed class KubernetesYamlRuntimeContext : KubernetesYamlStaticContext
    {
        private static readonly StaticObjectFactory s_factory = new KubernetesYamlStaticObjectFactory();

        public override bool IsKnownType(Type type)
        {
            return IsStaticStringDictionary(type)
                || base.IsKnownType(type);
        }

        public override StaticObjectFactory GetFactory()
        {
            return s_factory;
        }
    }

    private sealed class KubernetesYamlStaticObjectFactory : StaticObjectFactory
    {
        public override object Create(Type type)
        {
            var dictionary = CreateStaticStringDictionary(type);
            if (dictionary != null)
            {
                return dictionary;
            }

            return base.Create(type);
        }
    }

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
                        return Array.Empty<byte>();
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

    private sealed class KubernetesDateTimeOffsetYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(DateTimeOffset);

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (parser.Current is not Scalar scalar)
            {
                throw new YamlException("Expected an ISO 8601 timestamp scalar.");
            }

            parser.MoveNext();
            try
            {
                return DateTimeOffset.Parse(scalar.Value, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (FormatException exception)
            {
                throw new YamlException(scalar.Start, scalar.End, $"Invalid ISO 8601 timestamp '{scalar.Value}'.", exception);
            }
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is DateTimeOffset timestamp)
            {
                emitter.Emit(new Scalar(timestamp.ToString("O", CultureInfo.InvariantCulture)));
                return;
            }

            emitter.Emit(new Scalar(string.Empty));
        }
    }

    private sealed class KubeConfigNamedExtensionYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(k8s.KubeConfigModels.NamedExtension);
        }

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            parser.Consume<MappingStart>();
            string? name = null;
            object? extension = null;
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                if (key == "name")
                {
                    name = parser.Consume<Scalar>().Value;
                }
                else if (key == "extension")
                {
                    extension = ReadDynamicYamlValue(rootDeserializer);
                }
                else
                {
                    parser.SkipThisAndNestedEvents();
                }
            }

            parser.Consume<MappingEnd>();
            return new k8s.KubeConfigModels.NamedExtension { Name = name, Extension = extension };
        }

        private static object? ReadDynamicYamlValue(ObjectDeserializer rootDeserializer)
        {
            var json = (JsonElement)rootDeserializer(typeof(JsonElement))!;
            return ConvertJsonElement(json);
        }

        private static object? ConvertJsonElement(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    var dictionary = new Dictionary<object, object?>();
                    foreach (var property in value.EnumerateObject())
                    {
                        dictionary.Add(property.Name, ConvertJsonElement(property.Value));
                    }

                    return dictionary;
                case JsonValueKind.Array:
                    var items = new List<object?>();
                    foreach (var item in value.EnumerateArray())
                    {
                        items.Add(ConvertJsonElement(item));
                    }

                    return items;
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    if (value.TryGetInt64(out var integer))
                    {
                        return integer;
                    }

                    return value.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new YamlException("Unsupported kubeconfig extension data value.");
            }
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var extension = (k8s.KubeConfigModels.NamedExtension)value!;
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar("name"));
            emitter.Emit(new Scalar(extension.Name ?? string.Empty));
            emitter.Emit(new Scalar("extension"));
            object? extensionValue = extension.Extension;
            EmitExtensionValue(emitter, serializer, extensionValue);
            emitter.Emit(new MappingEnd());
        }

        private static void EmitExtensionValue(IEmitter emitter, ObjectSerializer serializer, object? value)
        {
            switch (value)
            {
                case null:
                    emitter.Emit(new Scalar("null"));
                    break;
                case System.Text.Json.JsonElement jsonElement:
                    serializer(jsonElement, typeof(System.Text.Json.JsonElement));
                    break;
                case string text:
                    emitter.Emit(new Scalar(null, null, text, ScalarStyle.DoubleQuoted, false, true));
                    break;
                case bool boolean:
                    emitter.Emit(new Scalar(boolean ? "true" : "false"));
                    break;
                case IDictionary dictionary:
                    emitter.Emit(new MappingStart());
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Key is not string key)
                        {
                            throw new YamlException("Kubeconfig extension data keys must be strings.");
                        }

                        emitter.Emit(new Scalar(key));
                        EmitExtensionValue(emitter, serializer, entry.Value);
                    }

                    emitter.Emit(new MappingEnd());
                    break;
                case IEnumerable sequence:
                    emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Block));
                    foreach (var item in sequence)
                    {
                        EmitExtensionValue(emitter, serializer, item);
                    }

                    emitter.Emit(new SequenceEnd());
                    break;
                case IFormattable formattable:
                    emitter.Emit(new Scalar(formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty));
                    break;
                default:
                    throw new YamlException("Unsupported kubeconfig extension data value.");
            }
        }
    }

    private sealed class KubernetesResourceQuantityYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(ResourceQuantity);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (parser.Current is not Scalar scalar)
            {
                throw new YamlException("Expected a Kubernetes quantity scalar.");
            }

            parser.MoveNext();
            try
            {
                return string.IsNullOrEmpty(scalar.Value) ? null : new ResourceQuantity(scalar.Value);
            }
            catch (ArgumentException exception)
            {
                throw new YamlException(scalar.Start, scalar.End, $"Invalid Kubernetes quantity '{scalar.Value}'.", exception);
            }
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new Scalar(value is ResourceQuantity quantity ? quantity.ToString() : string.Empty));
        }
    }

    private sealed class KubernetesByteArrayDictionaryYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(IDictionary<string, byte[]>)
                || type == typeof(Dictionary<string, byte[]>);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            parser.Consume<MappingStart>();
            var values = new Dictionary<string, byte[]>();
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                var value = (byte[]?)rootDeserializer(typeof(byte[]))
                    ?? throw new YamlException($"Kubernetes binary data value for '{key}' cannot be null.");
                values.Add(key, value);
            }

            parser.Consume<MappingEnd>();
            return values;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new MappingStart());
            if (value is IDictionary<string, byte[]> values)
            {
                foreach (var (key, item) in values)
                {
                    emitter.Emit(new Scalar(key));
                    serializer(item, typeof(byte[]));
                }
            }

            emitter.Emit(new MappingEnd());
        }
    }

    private sealed class KubernetesResourceQuantityDictionaryYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(IDictionary<string, ResourceQuantity>)
                || type == typeof(Dictionary<string, ResourceQuantity>);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            parser.Consume<MappingStart>();
            var quantities = new Dictionary<string, ResourceQuantity>();
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                var quantity = (ResourceQuantity?)rootDeserializer(typeof(ResourceQuantity));
                if (quantity is null)
                {
                    throw new YamlException($"Kubernetes resource quantity for '{key}' cannot be empty.");
                }

                quantities.Add(key, quantity);
            }

            parser.Consume<MappingEnd>();
            return quantities;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new MappingStart());
            if (value is IDictionary<string, ResourceQuantity> quantities)
            {
                foreach (var (key, quantity) in quantities)
                {
                    emitter.Emit(new Scalar(key));
                    emitter.Emit(new Scalar(quantity.ToString()));
                }
            }

            emitter.Emit(new MappingEnd());
        }
    }

    private sealed class KubernetesStringDictionaryYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(IDictionary<string, string>)
                || type == typeof(Dictionary<string, string>);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            parser.Consume<MappingStart>();
            var values = new Dictionary<string, string>();
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                var value = rootDeserializer(typeof(string)) as string
                    ?? throw new YamlException($"Kubernetes string dictionary value for '{key}' cannot be null.");
                values.Add(key, value);
            }

            parser.Consume<MappingEnd>();
            return values;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            emitter.Emit(new MappingStart());
            if (value is IDictionary<string, string> values)
            {
                foreach (var (key, item) in values)
                {
                    emitter.Emit(new Scalar(key));
                    serializer(item, typeof(string));
                }
            }

            emitter.Emit(new MappingEnd());
        }
    }

    private sealed class GenericKubernetesObjectYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(GenericKubernetesObject);
        }

        public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            parser.Consume<MappingStart>();
            var resource = new GenericKubernetesObject();
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                switch (key)
                {
                    case "apiVersion":
                        resource.ApiVersion = parser.Consume<Scalar>().Value;
                        break;
                    case "kind":
                        resource.Kind = parser.Consume<Scalar>().Value;
                        break;
                    case "metadata":
                        resource.Metadata = (V1ObjectMeta?)rootDeserializer(typeof(V1ObjectMeta))!;
                        break;
                    default:
                        resource.Properties[key] = (System.Text.Json.JsonElement)rootDeserializer(typeof(System.Text.Json.JsonElement))!;
                        break;
                }
            }

            parser.Consume<MappingEnd>();
            return resource;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var resource = (GenericKubernetesObject)value!;
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar("apiVersion"));
            emitter.Emit(new Scalar(resource.ApiVersion));
            emitter.Emit(new Scalar("kind"));
            emitter.Emit(new Scalar(resource.Kind));
            if (resource.Metadata != null)
            {
                emitter.Emit(new Scalar("metadata"));
                serializer(resource.Metadata, typeof(V1ObjectMeta));
            }

            foreach (var property in resource.Properties)
            {
                emitter.Emit(new Scalar(property.Key));
                serializer(property.Value, typeof(System.Text.Json.JsonElement));
            }

            emitter.Emit(new MappingEnd());
        }
    }

    /// <summary>
    /// Load a collection of objects from a stream asynchronously.
    /// </summary>
    public static async Task<List<object>> LoadAllFromStreamAsync(Stream stream, IDictionary<string, Type> typeMap)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return LoadAllFromString(content, typeMap);
    }

    public static async Task<List<object>> LoadAllFromStreamAsync(Stream stream, IDictionary<string, Type> typeMap, bool strict)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return LoadAllFromString(content, typeMap, strict);
    }

    public static async Task<List<object>> LoadAllFromStreamAsync(Stream stream, FrozenDictionary<string, Type> typeMap, bool strict)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var content = await reader.ReadToEndAsync().ConfigureAwait(false);
        return LoadAllFromString(content, typeMap, strict);
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
    /// A map from apiVersion/kind to Type. For example "v1/Pod" -> typeof(V1Pod).
    /// </param>
    /// <returns>collection of objects</returns>
    public static async Task<List<object>> LoadAllFromFileAsync(string fileName, IDictionary<string, Type> typeMap)
    {
        await using var fileStream = File.OpenRead(fileName);
        return await LoadAllFromStreamAsync(fileStream, typeMap).ConfigureAwait(false);
    }

    public static async Task<List<object>> LoadAllFromFileAsync(string fileName, IDictionary<string, Type> typeMap, bool strict)
    {
        await using var fileStream = File.OpenRead(fileName);
        return await LoadAllFromStreamAsync(fileStream, typeMap, strict).ConfigureAwait(false);
    }

    public static async Task<List<object>> LoadAllFromFileAsync(string fileName, FrozenDictionary<string, Type> typeMap, bool strict)
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
    /// A map from apiVersion/kind to Type. For example "v1/Pod" -> typeof(V1Pod).
    /// </param>
    /// <returns>collection of objects</returns>
    public static List<object> LoadAllFromString(string content, IDictionary<string, Type> typeMap, bool strict = false)
    {
        return LoadAllFromStringCore(content, key => typeMap[key], strict);
    }

    public static List<object> LoadAllFromString(
        string content,
        Func<string, Type> resolveType,
        bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(resolveType);
        return LoadAllFromStringCore(content, resolveType, strict);
    }

    public static List<object> LoadAllFromString(string content, FrozenDictionary<string, Type> typeMap, bool strict)
    {
        return LoadAllFromStringCore(content, key => typeMap[key], strict);
    }

    private static List<object> LoadAllFromStringCore(string content, Func<string, Type> resolveType, bool strict)
    {
        var types = new List<Type>();
        var parser = new MergingParser(new Parser(new StringReader(content)));
        parser.Consume<StreamStart>();
        while (parser.Accept<DocumentStart>(out _))
        {
            parser.Consume<DocumentStart>();
            parser.Consume<MappingStart>();
            string? apiVersion = null;
            string? kind = null;
            while (!parser.Accept<MappingEnd>(out _))
            {
                var key = parser.Consume<Scalar>().Value;
                if (key == "apiVersion")
                {
                    apiVersion = parser.Consume<Scalar>().Value;
                }
                else if (key == "kind")
                {
                    kind = parser.Consume<Scalar>().Value;
                }
                else
                {
                    parser.SkipThisAndNestedEvents();
                }
            }

            parser.Consume<MappingEnd>();
            parser.Consume<DocumentEnd>();
            if (apiVersion == null || kind == null)
            {
                throw new YamlException("A Kubernetes document must include apiVersion and kind.");
            }

            types.Add(resolveType($"{apiVersion}/{kind}"));
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
