namespace KubeUI.Kubernetes.Resources.GatewayApi;

/// <summary>
/// Gateway API release channel a resource kind belongs to.
/// </summary>
public enum GatewayApiChannel
{
    /// <summary>Stable kinds served with GA or beta versions.</summary>
    Standard,

    /// <summary>Experimental kinds: alpha-only versions or the <c>gateway.networking.x-k8s.io</c> group.</summary>
    Experimental,
}
