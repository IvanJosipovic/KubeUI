using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;

namespace KubeUI.Kubernetes;

/// <summary>
/// Uses the signed-in Azure CLI identity to discover Azure Monitor workspaces and query Prometheus.
/// </summary>
public sealed class AzureMonitorWorkspaceService : IAzureMonitorWorkspaceService
{
    private const string ManagementScope = "https://management.azure.com/.default";
    private const string PrometheusScope = "https://prometheus.monitor.azure.com/.default";
    private readonly TokenCredential _credential;

    /// <summary>Creates the service using the Azure CLI credential.</summary>
    public AzureMonitorWorkspaceService()
        : this(new AzureCliCredential())
    {
    }

    internal AzureMonitorWorkspaceService(TokenCredential credential)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
    }

    /// <inheritdoc />
    public async Task<AzureAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _credential.GetTokenAsync(new TokenRequestContext([ManagementScope]), cancellationToken).ConfigureAwait(false);
            return new AzureAuthenticationStatus
            {
                AzureCliSignedIn = true,
                Username = ReadClaim(token.Token, "preferred_username") ?? ReadClaim(token.Token, "upn"),
                TenantId = ReadClaim(token.Token, "tid"),
            };
        }
        catch (CredentialUnavailableException)
        {
            return new AzureAuthenticationStatus { AzureCliSignedIn = false };
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AzureMonitorSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        ArmClient armClient = new(_credential);
        List<AzureMonitorSubscriptionInfo> subscriptions = [];
        await foreach (var subscription in armClient.GetSubscriptions().GetAllAsync(cancellationToken).ConfigureAwait(false))
        {
            subscriptions.Add(new AzureMonitorSubscriptionInfo
            {
                SubscriptionId = subscription.Data.SubscriptionId,
                DisplayName = subscription.Data.DisplayName ?? subscription.Data.SubscriptionId,
            });
        }

        return subscriptions;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AzureMonitorWorkspaceInfo>> GetWorkspacesAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

        ArmClient armClient = new(_credential);
        var subscription = armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));
        List<AzureMonitorWorkspaceInfo> workspaces = [];
        await foreach (var resourceGroup in subscription.GetResourceGroups().GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            await foreach (var workspace in resourceGroup.GetMonitorWorkspaceResources().GetAllAsync(cancellationToken).ConfigureAwait(false))
            {
                var endpoint = workspace.Data.Metrics?.PrometheusQueryEndpoint;
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    continue;
                }

                workspaces.Add(new AzureMonitorWorkspaceInfo
                {
                    ResourceId = workspace.Id.ToString(),
                    Name = workspace.Data.Name,
                    Location = workspace.Data.Location.ToString(),
                    QueryEndpoint = endpoint,
                });
            }
        }

        return workspaces;
    }

    /// <inheritdoc />
    public ValueTask<AccessToken> GetPrometheusAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return _credential.GetTokenAsync(new TokenRequestContext([PrometheusScope]), cancellationToken);
    }

    private static string? ReadClaim(string jwt, string claimName)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload += new string('=', (4 - payload.Length % 4) % 4);
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            return document.RootElement.TryGetProperty(claimName, out var claim) ? claim.GetString() : null;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
