using KubeUI.Kubernetes.Resources.GatewayApi;

namespace KubeUI.Avalonia.Resources;

/// <summary>
/// Resource config that may represent a Gateway API kind.
/// </summary>
public interface IGatewayApiResourceConfig
{
    /// <summary>
    /// Gets the Gateway API channel of the configured kind, or <see langword="null"/> when it is not a Gateway API kind.
    /// </summary>
    GatewayApiChannel? GatewayApiChannel { get; }
}
