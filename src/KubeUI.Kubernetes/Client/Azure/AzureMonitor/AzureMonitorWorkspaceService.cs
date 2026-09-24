using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Microsoft.Extensions.Logging.Abstractions;

namespace KubeUI.Kubernetes;

/// <summary>
/// Uses the signed-in Azure CLI identity to discover Azure Monitor workspaces and query Prometheus.
/// </summary>
public sealed class AzureMonitorWorkspaceService : IAzureMonitorWorkspaceService, IDisposable
{
    private const string ManagementScope = "https://management.azure.com/.default";
    private const string PrometheusScope = "https://prometheus.monitor.azure.com/.default";
    private static readonly TimeSpan s_tokenRefreshBuffer = TimeSpan.FromMinutes(5);
    private readonly TokenCredential _credential;
    private readonly ILogger<AzureMonitorWorkspaceService> _logger;
    private readonly SemaphoreSlim _prometheusTokenLock = new(1, 1);
    private AccessToken? _prometheusAccessToken;

    /// <summary>Creates the service using the Azure CLI credential.</summary>
    public AzureMonitorWorkspaceService()
        : this(new AzureCliCredential(), NullLogger<AzureMonitorWorkspaceService>.Instance)
    {
    }

    /// <summary>Creates the service using the Azure CLI credential and application logger.</summary>
    public AzureMonitorWorkspaceService(ILogger<AzureMonitorWorkspaceService> logger)
        : this(new AzureCliCredential(), logger)
    {
    }

    internal AzureMonitorWorkspaceService(TokenCredential credential)
        : this(credential, NullLogger<AzureMonitorWorkspaceService>.Instance)
    {
    }

    internal AzureMonitorWorkspaceService(TokenCredential credential, ILogger<AzureMonitorWorkspaceService> logger)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.LogDebug("Azure CLI credentials are unavailable. Sign in with Azure CLI to access Azure Monitor workspaces.");
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

        _logger.LogInformation(
            "Discovered {WorkspaceCount} Azure Monitor Prometheus workspace(s) in subscription {SubscriptionId}.",
            workspaces.Count,
            subscriptionId);
        return workspaces;
    }

    /// <inheritdoc />
    public async ValueTask<AccessToken> GetPrometheusAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _prometheusTokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_prometheusAccessToken is { } cachedToken
                && cachedToken.ExpiresOn > DateTimeOffset.UtcNow.Add(s_tokenRefreshBuffer))
            {
                _logger.LogTrace("Reusing cached Azure Monitor Prometheus access token, expiring at {ExpiresOn:u}.", cachedToken.ExpiresOn);
                return cachedToken;
            }

            var refreshStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            try
            {
                var token = await _credential
                    .GetTokenAsync(new TokenRequestContext([PrometheusScope]), cancellationToken)
                    .ConfigureAwait(false);
                _prometheusAccessToken = token;
                _logger.LogDebug(
                    "Acquired Azure Monitor Prometheus access token expiring at {ExpiresOn:u} in {ElapsedMilliseconds}ms.",
                    token.ExpiresOn,
                    (long)System.Diagnostics.Stopwatch.GetElapsedTime(refreshStarted).TotalMilliseconds);
                return token;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to acquire an Azure Monitor Prometheus access token.");
                throw;
            }
        }
        finally
        {
            _prometheusTokenLock.Release();
        }
    }

    /// <summary>Releases resources used by the service.</summary>
    public void Dispose()
    {
        _prometheusTokenLock.Dispose();
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
