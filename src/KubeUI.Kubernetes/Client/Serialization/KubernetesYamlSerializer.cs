using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using k8s.KubeConfigModels;
using SVCSystems.Text.Json.Yaml;
using KubeConfigAuthProvider = k8s.KubeConfigModels.AuthProvider;
using KubeConfigCluster = k8s.KubeConfigModels.Cluster;
using KubeConfigClusterEndpoint = k8s.KubeConfigModels.ClusterEndpoint;
using KubeConfigContext = k8s.KubeConfigModels.Context;
using KubeConfigContextDetails = k8s.KubeConfigModels.ContextDetails;
using KubeConfigExternalExecution = k8s.KubeConfigModels.ExternalExecution;
using KubeConfigUser = k8s.KubeConfigModels.User;
using KubeConfigUserCredentials = k8s.KubeConfigModels.UserCredentials;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubernetesYamlSerializer : IKubernetesYamlSerializer
{
    private static readonly JsonTypeInfo<JsonElement> s_jsonElementTypeInfo = KubernetesJsonStaticContext.Default.JsonElement;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly JsonSerializerOptions _strictJsonOptions;

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

        var json = SerializeToJsonElement(value, _jsonOptions);
        return YamlSerializer.Serialize(json, s_jsonElementTypeInfo);
    }

    public string SerializeAll(IEnumerable<object> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        List<JsonElement> documents = [];
        foreach (var value in values)
        {
            if (value is null)
            {
                continue;
            }

            documents.Add(SerializeToJsonElement(value, _jsonOptions));
        }

        return YamlSerializer.SerializeDocuments(documents, s_jsonElementTypeInfo);
    }

    public T Deserialize<T>(string yaml, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        var json = DeserializeElement(yaml, strict);
        return (T)DeserializeJsonElement(json, typeof(T), GetOptions(strict))!;
    }

    public object? Deserialize(string yaml, Type type, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(type);

        var json = DeserializeElement(yaml, strict);
        return DeserializeJsonElement(json, type, GetOptions(strict));
    }

    public List<object> LoadAllFromString(string yaml, Func<string, Type> resolveType, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(resolveType);

        var jsonOptions = GetOptions(strict);
        List<object> resources = [];
        var normalizedYaml = YamlInputNormalizer.Normalize(yaml, strict);
        foreach (var document in YamlSerializer.DeserializeDocuments(normalizedYaml, s_jsonElementTypeInfo))
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
            resources.Add(DeserializeJsonElement(document, type, jsonOptions)!);
        }

        return resources;
    }

    private static JsonElement DeserializeElement(string yaml, bool strict) =>
        YamlSerializer.Deserialize(YamlInputNormalizer.Normalize(yaml, strict), s_jsonElementTypeInfo);

    private static JsonElement SerializeToJsonElement(object value, JsonSerializerOptions options)
    {
        if (value is K8SConfiguration kubeConfig)
        {
            return JsonSerializer.SerializeToElement(
                ToKubeConfigDocument(kubeConfig),
                KubeConfigJsonContext.Default.KubeConfigDocument);
        }

        if (value is NamedExtension extension)
        {
            KubeConfigNamedExtensionDocument document = new()
            {
                Name = extension.Name,
                Extension = SerializeExtension((object?)extension.Extension),
            };
            return JsonSerializer.SerializeToElement(
                document,
                KubeConfigJsonContext.Default.KubeConfigNamedExtensionDocument);
        }

        return JsonSerializer.SerializeToElement(value, options.GetTypeInfo(value.GetType()));
    }

    private static object? DeserializeJsonElement(JsonElement json, Type type, JsonSerializerOptions options)
    {
        if (type == typeof(object))
        {
            return json.ValueKind == JsonValueKind.Object
                ? DeserializeUntypedObjectMapping(json)
                : DeserializeUntyped(json);
        }

        if (type == typeof(Dictionary<string, object>))
        {
            return DeserializeUntypedMapping(json);
        }

        if (type == typeof(K8SConfiguration))
        {
            var document = JsonSerializer.Deserialize(json, KubeConfigJsonContext.Default.KubeConfigDocument)
                ?? throw new JsonException("The kubeconfig document is empty.");
            return ToKubeConfig(document);
        }

        if (type == typeof(NamedExtension))
        {
            var document = JsonSerializer.Deserialize(json, KubeConfigJsonContext.Default.KubeConfigNamedExtensionDocument)
                ?? throw new JsonException("The kubeconfig extension document is empty.");
            return new NamedExtension
            {
                Name = document.Name,
                Extension = DeserializeExtension(document.Extension),
            };
        }

        return JsonSerializer.Deserialize(json, options.GetTypeInfo(type));
    }

    private static Dictionary<string, object?>? DeserializeUntypedMapping(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (json.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("The YAML value must be a mapping.");
        }

        Dictionary<string, object?> mapping = new(StringComparer.Ordinal);
        foreach (var property in json.EnumerateObject())
        {
            mapping.Add(property.Name, DeserializeUntyped(property.Value));
        }

        return mapping;
    }

    private static object? DeserializeUntyped(JsonElement json)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Object:
                return DeserializeUntypedObjectMapping(json);
            case JsonValueKind.Array:
                List<object?> sequence = [];
                foreach (var item in json.EnumerateArray())
                {
                    sequence.Add(DeserializeUntyped(item));
                }

                return sequence;
            case JsonValueKind.String:
                return json.GetString();
            case JsonValueKind.Number:
                return json.TryGetInt64(out var integer) ? integer : json.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new JsonException("Unsupported YAML value.");
        }
    }

    private static Dictionary<object, object?> DeserializeUntypedObjectMapping(JsonElement json)
    {
        Dictionary<object, object?> mapping = [];
        foreach (var property in json.EnumerateObject())
        {
            mapping.Add(property.Name, DeserializeUntyped(property.Value));
        }

        return mapping;
    }

    private static KubeConfigDocument ToKubeConfigDocument(K8SConfiguration config) => new()
    {
        ApiVersion = config.ApiVersion,
        Kind = config.Kind,
        Preferences = config.Preferences is null ? null : SerializeExtension(config.Preferences),
        CurrentContext = config.CurrentContext,
        Contexts = config.Contexts?.Select(ToKubeConfigContextDocument).ToList(),
        Clusters = config.Clusters?.Select(ToKubeConfigClusterDocument).ToList(),
        Users = config.Users?.Select(ToKubeConfigUserDocument).ToList(),
        Extensions = ToKubeConfigExtensionDocuments(config.Extensions),
    };

    private static K8SConfiguration ToKubeConfig(KubeConfigDocument document) => new()
    {
        ApiVersion = document.ApiVersion,
        Kind = document.Kind,
        Preferences = DeserializePreferences(document.Preferences),
        CurrentContext = document.CurrentContext,
        Contexts = document.Contexts?.Select(ToKubeConfigContext).ToList(),
        Clusters = document.Clusters?.Select(ToKubeConfigCluster).ToList(),
        Users = document.Users?.Select(ToKubeConfigUser).ToList(),
        Extensions = ToKubeConfigExtensions(document.Extensions),
    };

    private static KubeConfigContextDocument ToKubeConfigContextDocument(KubeConfigContext context) => new()
    {
        Name = context.Name,
        Context = context.ContextDetails is null ? null : new KubeConfigContextDetailsDocument
        {
            Cluster = context.ContextDetails.Cluster,
            User = context.ContextDetails.User,
            Namespace = context.ContextDetails.Namespace,
            Extensions = ToKubeConfigExtensionDocuments(context.ContextDetails.Extensions),
        },
        Extensions = ToKubeConfigExtensionDocuments(context.Extensions),
        Namespace = context.Namespace,
    };

    private static KubeConfigContext ToKubeConfigContext(KubeConfigContextDocument context) => new()
    {
        Name = context.Name,
        ContextDetails = context.Context is null ? null : new KubeConfigContextDetails
        {
            Cluster = context.Context.Cluster,
            User = context.Context.User,
            Namespace = context.Context.Namespace,
            Extensions = ToKubeConfigExtensions(context.Context.Extensions),
        },
        Extensions = ToKubeConfigExtensions(context.Extensions),
        Namespace = context.Namespace,
    };

    private static KubeConfigClusterDocument ToKubeConfigClusterDocument(KubeConfigCluster cluster) => new()
    {
        Name = cluster.Name,
        Cluster = cluster.ClusterEndpoint is null ? null : new KubeConfigClusterEndpointDocument
        {
            CertificateAuthority = cluster.ClusterEndpoint.CertificateAuthority,
            CertificateAuthorityData = cluster.ClusterEndpoint.CertificateAuthorityData,
            Server = cluster.ClusterEndpoint.Server,
            TlsServerName = cluster.ClusterEndpoint.TlsServerName,
            SkipTlsVerify = cluster.ClusterEndpoint.SkipTlsVerify,
            Extensions = ToKubeConfigExtensionDocuments(cluster.ClusterEndpoint.Extensions),
        },
    };

    private static KubeConfigCluster ToKubeConfigCluster(KubeConfigClusterDocument cluster) => new()
    {
        Name = cluster.Name,
        ClusterEndpoint = cluster.Cluster is null ? null : new KubeConfigClusterEndpoint
        {
            CertificateAuthority = cluster.Cluster.CertificateAuthority,
            CertificateAuthorityData = cluster.Cluster.CertificateAuthorityData,
            Server = cluster.Cluster.Server,
            TlsServerName = cluster.Cluster.TlsServerName,
            SkipTlsVerify = cluster.Cluster.SkipTlsVerify,
            Extensions = ToKubeConfigExtensions(cluster.Cluster.Extensions),
        },
    };

    private static KubeConfigUserDocument ToKubeConfigUserDocument(KubeConfigUser user) => new()
    {
        Name = user.Name,
        User = user.UserCredentials is null ? null : ToKubeConfigUserCredentialsDocument(user.UserCredentials),
    };

    private static KubeConfigUser ToKubeConfigUser(KubeConfigUserDocument user) => new()
    {
        Name = user.Name,
        UserCredentials = user.User is null ? null : ToKubeConfigUserCredentials(user.User),
    };

    private static KubeConfigUserCredentialsDocument ToKubeConfigUserCredentialsDocument(KubeConfigUserCredentials credentials) => new()
    {
        ClientCertificateData = credentials.ClientCertificateData,
        ClientCertificate = credentials.ClientCertificate,
        ClientKeyData = credentials.ClientKeyData,
        ClientKey = credentials.ClientKey,
        Token = credentials.Token,
        Impersonate = credentials.Impersonate,
        ImpersonateGroups = credentials.ImpersonateGroups?.ToList(),
        ImpersonateUserExtra = credentials.ImpersonateUserExtra,
        UserName = credentials.UserName,
        Password = credentials.Password,
        AuthProvider = credentials.AuthProvider is null ? null : new KubeConfigAuthProviderDocument
        {
            Name = credentials.AuthProvider.Name,
            Config = credentials.AuthProvider.Config,
        },
        ExternalExecution = credentials.ExternalExecution is null ? null : new KubeConfigExternalExecutionDocument
        {
            ApiVersion = credentials.ExternalExecution.ApiVersion,
            Command = credentials.ExternalExecution.Command,
            EnvironmentVariables = credentials.ExternalExecution.EnvironmentVariables?
                .Select(static variables => new Dictionary<string, string>(variables, StringComparer.Ordinal))
                .ToList(),
            Arguments = credentials.ExternalExecution.Arguments?.ToList(),
            InstallHint = credentials.ExternalExecution.InstallHint,
            ProvideClusterInfo = credentials.ExternalExecution.ProvideClusterInfo,
        },
        Extensions = ToKubeConfigExtensionDocuments(credentials.Extensions),
    };

    private static KubeConfigUserCredentials ToKubeConfigUserCredentials(KubeConfigUserCredentialsDocument credentials) => new()
    {
        ClientCertificateData = credentials.ClientCertificateData,
        ClientCertificate = credentials.ClientCertificate,
        ClientKeyData = credentials.ClientKeyData,
        ClientKey = credentials.ClientKey,
        Token = credentials.Token,
        Impersonate = credentials.Impersonate,
        ImpersonateGroups = credentials.ImpersonateGroups,
        ImpersonateUserExtra = credentials.ImpersonateUserExtra,
        UserName = credentials.UserName,
        Password = credentials.Password,
        AuthProvider = credentials.AuthProvider is null ? null : new KubeConfigAuthProvider
        {
            Name = credentials.AuthProvider.Name,
            Config = credentials.AuthProvider.Config,
        },
        ExternalExecution = credentials.ExternalExecution is null ? null : new KubeConfigExternalExecution
        {
            ApiVersion = credentials.ExternalExecution.ApiVersion,
            Command = credentials.ExternalExecution.Command,
            EnvironmentVariables = credentials.ExternalExecution.EnvironmentVariables,
            Arguments = credentials.ExternalExecution.Arguments,
            InstallHint = credentials.ExternalExecution.InstallHint,
            ProvideClusterInfo = credentials.ExternalExecution.ProvideClusterInfo,
        },
        Extensions = ToKubeConfigExtensions(credentials.Extensions),
    };

    private static List<KubeConfigNamedExtensionDocument>? ToKubeConfigExtensionDocuments(IEnumerable<NamedExtension>? extensions) =>
        extensions?.Select(extension => new KubeConfigNamedExtensionDocument
        {
            Name = extension.Name,
            Extension = SerializeExtension((object?)extension.Extension),
        }).ToList();

    private static List<NamedExtension>? ToKubeConfigExtensions(IEnumerable<KubeConfigNamedExtensionDocument>? extensions) =>
        extensions?.Select(extension => new NamedExtension
        {
            Name = extension.Name,
            Extension = DeserializeExtension(extension.Extension),
        }).ToList();

    private static JsonElement SerializeExtension(object? value)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            WriteExtension(writer, value);
        }

        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }

    private static void WriteExtension(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                return;
            case JsonElement element:
                element.WriteTo(writer);
                return;
            case string text:
                writer.WriteStringValue(text);
                return;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                return;
            case byte number:
                writer.WriteNumberValue(number);
                return;
            case short number:
                writer.WriteNumberValue(number);
                return;
            case int number:
                writer.WriteNumberValue(number);
                return;
            case long number:
                writer.WriteNumberValue(number);
                return;
            case float number:
                writer.WriteNumberValue(number);
                return;
            case double number:
                writer.WriteNumberValue(number);
                return;
            case decimal number:
                writer.WriteNumberValue(number);
                return;
            case System.Collections.IDictionary dictionary:
                writer.WriteStartObject();
                foreach (System.Collections.DictionaryEntry pair in dictionary)
                {
                    writer.WritePropertyName(pair.Key?.ToString() ?? string.Empty);
                    WriteExtension(writer, pair.Value);
                }

                writer.WriteEndObject();
                return;
            case System.Collections.IEnumerable sequence:
                writer.WriteStartArray();
                foreach (var item in sequence)
                {
                    WriteExtension(writer, item);
                }

                writer.WriteEndArray();
                return;
            default:
                throw new JsonException($"Unsupported kubeconfig extension value type '{value.GetType()}'.");
        }
    }

    private static object? DeserializeExtension(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                Dictionary<object, object> dictionary = [];
                foreach (var property in value.EnumerateObject())
                {
                    dictionary.Add(property.Name, DeserializeExtension(property.Value)!);
                }

                return dictionary;
            case JsonValueKind.Array:
                List<object> sequence = [];
                foreach (var item in value.EnumerateArray())
                {
                    sequence.Add(DeserializeExtension(item)!);
                }

                return sequence;
            case JsonValueKind.String:
                return value.GetString();
            case JsonValueKind.Number:
                return value.TryGetInt64(out var integer) ? integer : value.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                throw new JsonException("Unsupported kubeconfig extension data value.");
        }
    }

    private static IDictionary<string, object>? DeserializePreferences(JsonElement? preferences)
    {
        if (preferences is not { ValueKind: JsonValueKind.Object } value)
        {
            return null;
        }

        Dictionary<string, object> result = new(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            result.Add(property.Name, DeserializeExtension(property.Value)!);
        }

        return result;
    }

    private JsonSerializerOptions GetOptions(bool strict) => strict ? _strictJsonOptions : _jsonOptions;
}
