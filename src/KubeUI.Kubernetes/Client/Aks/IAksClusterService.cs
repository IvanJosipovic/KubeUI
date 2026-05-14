using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes;

/// <summary>
/// Describes an Azure Kubernetes Service subscription that can contain one or more clusters.
/// </summary>
public sealed record AksSubscriptionInfo
{
    public required string SubscriptionId { get; init; }

    public required string DisplayName { get; init; }
}

/// <summary>
/// Describes a discovered AKS cluster.
/// </summary>
public sealed record AksClusterInfo
{
    public required string SubscriptionId { get; init; }

    public required string ResourceGroupName { get; init; }

    public required string Name { get; init; }

    public required string Location { get; init; }

    public string? KubernetesVersion { get; init; }

    public string? Fqdn { get; init; }
}

/// <summary>
/// Specifies which AKS credential flavor should be requested.
/// </summary>
public enum AksCredentialKind
{
    User,
    Admin
}

/// <summary>
/// Represents the credentials needed to connect to an AKS cluster.
/// </summary>
public sealed record AksClusterCredentials
{
    public required AksClusterInfo Cluster { get; init; }

    public required AksCredentialKind CredentialKind { get; init; }

    public required K8SConfiguration KubeConfig { get; init; }

    public bool RequiresKubelogin { get; init; }
}

/// <summary>
/// Describes the available Azure sign-in status for AKS discovery.
/// </summary>
public sealed record AksAuthenticationStatus
{
    public bool AzureCliSignedIn { get; init; }

    public string? AzureCliUsername { get; init; }

    public string? AzureCliTenantId { get; init; }
}

/// <summary>
/// Discovers AKS subscriptions, clusters, and cluster credentials.
/// </summary>
public interface IAksClusterService
{
    /// <summary>
    /// Returns the current Azure sign-in status for AKS discovery.
    /// </summary>
    Task<AksAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the Azure subscriptions that can be used for AKS discovery.
    /// </summary>
    Task<IReadOnlyList<AksSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the AKS clusters available in the specified subscription.
    /// </summary>
    Task<IReadOnlyList<AksClusterInfo>> GetClustersAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns kubeconfig credentials for the specified AKS cluster.
    /// </summary>
    Task<AksClusterCredentials> GetCredentialsAsync(
        AksClusterInfo cluster,
        AksCredentialKind credentialKind = AksCredentialKind.User,
        CancellationToken cancellationToken = default);
}
