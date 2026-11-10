namespace KubeUI.Kubernetes;

/// <summary>
/// Reports whether the current Azure CLI identity is authenticated and, when available, its account details.
/// </summary>
public sealed record AzureAuthenticationStatus
{
    /// <summary>Gets whether Azure CLI authentication succeeded.</summary>
    public bool AzureCliSignedIn { get; init; }

    /// <summary>Gets the account name from the Azure CLI token when present.</summary>
    public string? Username { get; init; }

    /// <summary>Gets the tenant identifier from the Azure CLI token when present.</summary>
    public string? TenantId { get; init; }
}
