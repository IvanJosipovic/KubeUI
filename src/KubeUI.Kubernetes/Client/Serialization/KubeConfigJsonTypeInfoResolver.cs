using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using k8s.KubeConfigModels;
using KubeConfigAuthProvider = k8s.KubeConfigModels.AuthProvider;
using KubeConfigCluster = k8s.KubeConfigModels.Cluster;
using KubeConfigClusterEndpoint = k8s.KubeConfigModels.ClusterEndpoint;
using KubeConfigContext = k8s.KubeConfigModels.Context;
using KubeConfigContextDetails = k8s.KubeConfigModels.ContextDetails;
using KubeConfigExternalExecution = k8s.KubeConfigModels.ExternalExecution;
using KubeConfigUser = k8s.KubeConfigModels.User;
using KubeConfigUserCredentials = k8s.KubeConfigModels.UserCredentials;

namespace KubeUI.Kubernetes.Serialization;

internal sealed class KubeConfigJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private static readonly JsonConverter<K8SConfiguration> s_converter = new KubeConfigJsonConverter();

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options) =>
        type == typeof(K8SConfiguration)
            ? JsonMetadataServices.CreateValueInfo<K8SConfiguration>(options, s_converter)
            : null;
}

internal sealed class KubeConfigJsonConverter : JsonConverter<K8SConfiguration>
{
    public override K8SConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonSerializer.Deserialize(ref reader, KubeConfigJsonContext.Default.KubeConfigDocument)
            ?? throw new JsonException("The kubeconfig document is empty.");
        return ToKubeConfig(document);
    }

    public override void Write(Utf8JsonWriter writer, K8SConfiguration value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, ToKubeConfigDocument(value), KubeConfigJsonContext.Default.KubeConfigDocument);

    private static KubeConfigDocument ToKubeConfigDocument(K8SConfiguration config) => new()
    {
        ApiVersion = config.ApiVersion,
        Kind = config.Kind,
        Preferences = config.Preferences is null ? null : KubeConfigExtensionValueCodec.SerializeToElement(config.Preferences),
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
            Extension = KubeConfigExtensionValueCodec.SerializeToElement((object?)extension.Extension),
        }).ToList();

    private static List<NamedExtension>? ToKubeConfigExtensions(IEnumerable<KubeConfigNamedExtensionDocument>? extensions) =>
        extensions?.Select(extension => new NamedExtension
        {
            Name = extension.Name,
            Extension = KubeConfigExtensionValueCodec.Read(extension.Extension),
        }).ToList();

    private static IDictionary<string, object>? DeserializePreferences(JsonElement? preferences)
    {
        if (preferences is not { ValueKind: JsonValueKind.Object } value)
        {
            return null;
        }

        Dictionary<string, object> result = new(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            result.Add(property.Name, KubeConfigExtensionValueCodec.Read(property.Value)!);
        }

        return result;
    }

}
