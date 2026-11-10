using System.Text.Json;
using System.Text.Json.Serialization;

namespace KubeUI.Kubernetes.Serialization;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(KubeConfigDocument))]
internal partial class KubeConfigJsonContext : JsonSerializerContext;

internal sealed class KubeConfigDocument
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("preferences")]
    public JsonElement? Preferences { get; set; }

    [JsonPropertyName("current-context")]
    public string? CurrentContext { get; set; }

    [JsonPropertyName("contexts")]
    public List<KubeConfigContextDocument>? Contexts { get; set; }

    [JsonPropertyName("clusters")]
    public List<KubeConfigClusterDocument>? Clusters { get; set; }

    [JsonPropertyName("users")]
    public List<KubeConfigUserDocument>? Users { get; set; }

    [JsonPropertyName("extensions")]
    public List<KubeConfigNamedExtensionDocument>? Extensions { get; set; }
}

internal sealed class KubeConfigClusterDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cluster")]
    public KubeConfigClusterEndpointDocument? Cluster { get; set; }
}

internal sealed class KubeConfigClusterEndpointDocument
{
    [JsonPropertyName("certificate-authority")]
    public string? CertificateAuthority { get; set; }

    [JsonPropertyName("certificate-authority-data")]
    public string? CertificateAuthorityData { get; set; }

    [JsonPropertyName("server")]
    public string? Server { get; set; }

    [JsonPropertyName("tls-server-name")]
    public string? TlsServerName { get; set; }

    [JsonPropertyName("insecure-skip-tls-verify")]
    public bool SkipTlsVerify { get; set; }

    [JsonPropertyName("extensions")]
    public List<KubeConfigNamedExtensionDocument>? Extensions { get; set; }
}

internal sealed class KubeConfigContextDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("context")]
    public KubeConfigContextDetailsDocument? Context { get; set; }

    [JsonPropertyName("extensions")]
    public List<KubeConfigNamedExtensionDocument>? Extensions { get; set; }

    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }
}

internal sealed class KubeConfigContextDetailsDocument
{
    [JsonPropertyName("cluster")]
    public string? Cluster { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }

    [JsonPropertyName("extensions")]
    public List<KubeConfigNamedExtensionDocument>? Extensions { get; set; }
}

internal sealed class KubeConfigUserDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("user")]
    public KubeConfigUserCredentialsDocument? User { get; set; }
}

internal sealed class KubeConfigUserCredentialsDocument
{
    [JsonPropertyName("client-certificate-data")]
    public string? ClientCertificateData { get; set; }

    [JsonPropertyName("client-certificate")]
    public string? ClientCertificate { get; set; }

    [JsonPropertyName("client-key-data")]
    public string? ClientKeyData { get; set; }

    [JsonPropertyName("client-key")]
    public string? ClientKey { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("impersonate")]
    public string? Impersonate { get; set; }

    [JsonPropertyName("impersonate-groups")]
    public List<string>? ImpersonateGroups { get; set; }

    [JsonPropertyName("impersonate-user-extra")]
    public Dictionary<string, string>? ImpersonateUserExtra { get; set; }

    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("auth-provider")]
    public KubeConfigAuthProviderDocument? AuthProvider { get; set; }

    [JsonPropertyName("exec")]
    public KubeConfigExternalExecutionDocument? ExternalExecution { get; set; }

    [JsonPropertyName("extensions")]
    public List<KubeConfigNamedExtensionDocument>? Extensions { get; set; }
}

internal sealed class KubeConfigAuthProviderDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("config")]
    public Dictionary<string, string>? Config { get; set; }
}

internal sealed class KubeConfigExternalExecutionDocument
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("env")]
    public List<Dictionary<string, string>>? EnvironmentVariables { get; set; }

    [JsonPropertyName("args")]
    public List<string>? Arguments { get; set; }

    [JsonPropertyName("installHint")]
    public string? InstallHint { get; set; }

    [JsonPropertyName("provideClusterInfo")]
    public bool ProvideClusterInfo { get; set; }
}

internal sealed class KubeConfigNamedExtensionDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("extension")]
    public JsonElement Extension { get; set; }
}
