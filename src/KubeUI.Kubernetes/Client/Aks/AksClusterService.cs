using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;
using Azure.ResourceManager.Resources;
using k8s;
using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes;

/// <summary>
/// Azure SDK backed AKS discovery and credential service.
/// </summary>
public sealed class AksClusterService : IAksClusterService
{
    private const string AzureManagementScope = "https://management.azure.com/.default";
    private readonly AzureCliCredential _azureCliCredential;

    public AksClusterService()
        : this(new AzureCliCredential())
    {
    }

    private AksClusterService(AzureCliCredential azureCliCredential)
    {
        _azureCliCredential = azureCliCredential;
    }

    /// <inheritdoc />
    public async Task<AksAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
    {
        AccessToken? azureCliToken = await TryGetTokenAsync(_azureCliCredential, cancellationToken).ConfigureAwait(false);

        return new AksAuthenticationStatus
        {
            AzureCliSignedIn = azureCliToken.HasValue,
            AzureCliUsername = azureCliToken.HasValue ? TryGetPreferredUsername(azureCliToken.Value.Token) : null,
            AzureCliTenantId = azureCliToken.HasValue ? TryGetTenantId(azureCliToken.Value.Token) : null
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AksSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        ArmClient armClient = CreateArmClient();
        List<AksSubscriptionInfo> result = [];

        SubscriptionCollection subscriptions = armClient.GetSubscriptions();

        await foreach (SubscriptionResource subscription in subscriptions.GetAllAsync(cancellationToken).ConfigureAwait(false))
        {
            result.Add(new AksSubscriptionInfo
            {
                SubscriptionId = subscription.Data.SubscriptionId,
                DisplayName = subscription.Data.DisplayName ?? subscription.Data.SubscriptionId
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AksClusterInfo>> GetClustersAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

        var armClient = CreateArmClient();
        var subscription = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

        List<AksClusterInfo> result = [];

        await foreach (var resourceGroup in subscription.GetResourceGroups().GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            ContainerServiceManagedClusterCollection clusters = resourceGroup.GetContainerServiceManagedClusters();

            await foreach (ContainerServiceManagedClusterResource cluster in clusters.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                result.Add(new AksClusterInfo
                {
                    SubscriptionId = subscriptionId,
                    ResourceGroupName = resourceGroup.Data.Name,
                    Name = cluster.Data.Name,
                    Location = cluster.Data.Location.ToString(),
                    KubernetesVersion = cluster.Data.KubernetesVersion,
                    Fqdn = cluster.Data.Fqdn
                });
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<AksClusterCredentials> GetCredentialsAsync(
        AksClusterInfo cluster,
        AksCredentialKind credentialKind = AksCredentialKind.User,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        var armClient = CreateArmClient();
        ResourceIdentifier clusterId = new($"/subscriptions/{cluster.SubscriptionId}/resourceGroups/{cluster.ResourceGroupName}/providers/Microsoft.ContainerService/managedClusters/{cluster.Name}");
        var clusterResource = armClient.GetContainerServiceManagedClusterResource(clusterId);

        var response = credentialKind == AksCredentialKind.Admin
            ? await clusterResource.GetClusterAdminCredentialsAsync(cluster.Fqdn, cancellationToken).ConfigureAwait(false)
            : await clusterResource.GetClusterUserCredentialsAsync(cluster.Fqdn, KubeConfigFormat.Exec, cancellationToken).ConfigureAwait(false);

        ManagedClusterCredential? firstKubeConfig = response.Value.Kubeconfigs.FirstOrDefault();
        if (firstKubeConfig?.Value == null)
        {
            throw new InvalidOperationException("AKS did not return any kubeconfig data.");
        }

        string decodedConfig = Encoding.UTF8.GetString(firstKubeConfig.Value);
        var kubeConfig = Serialization.KubernetesYaml.Deserialize<K8SConfiguration>(decodedConfig);
        kubeConfig.FileName = string.Empty;
        NormalizeKubeConfigNames(kubeConfig, cluster.Name);

        return new AksClusterCredentials
        {
            Cluster = cluster,
            CredentialKind = credentialKind,
            KubeConfig = kubeConfig,
            RequiresKubelogin = credentialKind == AksCredentialKind.User
        };
    }

    internal static void NormalizeKubeConfigNames(K8SConfiguration kubeConfig, string clusterName)
    {
        if (string.IsNullOrWhiteSpace(clusterName))
        {
            return;
        }

        foreach (Context context in kubeConfig.Contexts)
        {
            context.Name = clusterName;

            if (context.ContextDetails is not null)
            {
                context.ContextDetails.Cluster = clusterName;
            }
        }

        foreach (k8s.KubeConfigModels.Cluster configuredCluster in kubeConfig.Clusters)
        {
            configuredCluster.Name = clusterName;
        }

        kubeConfig.CurrentContext = clusterName;
    }

    private static async Task<AccessToken?> TryGetTokenAsync(TokenCredential credential, CancellationToken cancellationToken)
    {
        try
        {
            return await credential.GetTokenAsync(new TokenRequestContext([AzureManagementScope]), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? TryGetPreferredUsername(string jwt)
    {
        return TryGetJwtClaim(jwt, "preferred_username")
            ?? TryGetJwtClaim(jwt, "upn")
            ?? TryGetJwtClaim(jwt, "unique_name")
            ?? TryGetJwtClaim(jwt, "email")
            ?? TryGetJwtClaim(jwt, "appid");
    }

    private static string? TryGetTenantId(string jwt)
    {
        return TryGetJwtClaim(jwt, "tid");
    }

    private static string? TryGetJwtClaim(string jwt, string claimName)
    {
        string[] parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            string payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            int padding = (4 - payload.Length % 4) % 4;
            if (padding > 0)
            {
                payload = payload + new string('=', padding);
            }

            byte[] bytes = Convert.FromBase64String(payload);
            using JsonDocument document = JsonDocument.Parse(bytes);
            if (document.RootElement.TryGetProperty(claimName, out JsonElement claim))
            {
                return claim.GetString();
            }
        }
        catch (Exception)
        {
        }

        return null;
    }

    private ArmClient CreateArmClient()
    {
        return new ArmClient(_azureCliCredential);
    }
}
