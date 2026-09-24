namespace KubeUI.Kubernetes;

/// <summary>
/// Describes an Azure subscription available to the current Azure CLI identity.
/// </summary>
public sealed record AzureMonitorSubscriptionInfo
{
    /// <summary>Gets the subscription identifier.</summary>
    public required string SubscriptionId { get; init; }

    /// <summary>Gets the display name.</summary>
    public required string DisplayName { get; init; }
}

/// <summary>
/// Describes an Azure Monitor workspace with a Prometheus query endpoint.
/// </summary>
public sealed record AzureMonitorWorkspaceInfo
{
    /// <summary>Gets the Azure resource identifier.</summary>
    public required string ResourceId { get; init; }

    /// <summary>Gets the workspace name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the workspace location.</summary>
    public required string Location { get; init; }

    /// <summary>Gets the Prometheus query endpoint.</summary>
    public required string QueryEndpoint { get; init; }
}
