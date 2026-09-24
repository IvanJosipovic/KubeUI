using Azure.Core;

namespace KubeUI.Kubernetes;

/// <summary>
/// Provides Azure CLI authentication and Azure Monitor workspace discovery.
/// </summary>
public interface IAzureMonitorWorkspaceService
{
    /// <summary>Returns the current Azure CLI authentication state.</summary>
    Task<AzureAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists subscriptions visible to the current Azure CLI identity.</summary>
    Task<IReadOnlyList<AzureMonitorSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists Azure Monitor workspaces in the selected subscription.</summary>
    Task<IReadOnlyList<AzureMonitorWorkspaceInfo>> GetWorkspacesAsync(string subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>Gets an access token for the Azure Monitor Prometheus query API.</summary>
    ValueTask<AccessToken> GetPrometheusAccessTokenAsync(CancellationToken cancellationToken = default);
}
