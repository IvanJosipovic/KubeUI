using System.Text.Json;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.GatewayApi;

/// <summary>
/// Identifies Gateway API resource kinds and provides starter specs for new resources.
/// </summary>
public static class GatewayApiResources
{
    /// <summary>The Gateway API group.</summary>
    public const string Group = "gateway.networking.k8s.io";

    /// <summary>The group used by experimental <c>X</c>-prefixed Gateway API kinds.</summary>
    public const string ExperimentalGroup = "gateway.networking.x-k8s.io";

    /// <summary>
    /// Determines whether <paramref name="group"/> is a Gateway API group.
    /// </summary>
    /// <param name="group">The API group.</param>
    /// <returns><see langword="true"/> for Gateway API groups.</returns>
    public static bool IsGatewayApiGroup(string? group) =>
        string.Equals(group, Group, StringComparison.Ordinal)
        || string.Equals(group, ExperimentalGroup, StringComparison.Ordinal);

    /// <summary>
    /// Classifies a Gateway API custom resource definition by release channel.
    /// </summary>
    /// <param name="crd">The custom resource definition.</param>
    /// <returns>The channel, or <see langword="null"/> when the definition is not part of Gateway API.</returns>
    /// <remarks>
    /// The <c>gateway.networking.k8s.io/channel</c> annotation is not used because the experimental bundle
    /// annotates stable kinds such as Gateway as experimental too. A kind is standard when it serves a non-alpha version.
    /// </remarks>
    public static GatewayApiChannel? GetChannel(V1CustomResourceDefinition crd)
    {
        ArgumentNullException.ThrowIfNull(crd);

        var group = crd.Spec?.Group;
        if (!IsGatewayApiGroup(group))
        {
            return null;
        }

        if (string.Equals(group, ExperimentalGroup, StringComparison.Ordinal))
        {
            return GatewayApiChannel.Experimental;
        }

        foreach (var version in crd.Spec?.Versions ?? [])
        {
            if (version.Served && !version.Name.Contains("alpha", StringComparison.Ordinal))
            {
                return GatewayApiChannel.Standard;
            }
        }

        return GatewayApiChannel.Experimental;
    }

    /// <summary>
    /// Gets a starter <c>spec</c> for a new Gateway API resource.
    /// </summary>
    /// <param name="kind">The resource kind, for example <c>HTTPRoute</c>.</param>
    /// <param name="spec">The starter spec when one exists.</param>
    /// <returns><see langword="true"/> when a template exists for <paramref name="kind"/>.</returns>
    public static bool TryGetSpecTemplate(string kind, out JsonElement spec)
    {
        var json = kind switch
        {
            "GatewayClass" => """{"controllerName":"example.com/gateway-controller"}""",
            "Gateway" => """{"gatewayClassName":"example","listeners":[{"name":"http","protocol":"HTTP","port":80}]}""",
            "HTTPRoute" => """{"parentRefs":[{"name":"example-gateway"}],"hostnames":["example.com"],"rules":[{"matches":[{"path":{"type":"PathPrefix","value":"/"}}],"backendRefs":[{"name":"example-service","port":80}]}]}""",
            "GRPCRoute" => """{"parentRefs":[{"name":"example-gateway"}],"rules":[{"backendRefs":[{"name":"example-service","port":50051}]}]}""",
            "TLSRoute" => """{"parentRefs":[{"name":"example-gateway"}],"hostnames":["example.com"],"rules":[{"backendRefs":[{"name":"example-service","port":443}]}]}""",
            "TCPRoute" or "UDPRoute" => """{"parentRefs":[{"name":"example-gateway"}],"rules":[{"backendRefs":[{"name":"example-service","port":8080}]}]}""",
            "ReferenceGrant" => """{"from":[{"group":"gateway.networking.k8s.io","kind":"HTTPRoute","namespace":"default"}],"to":[{"group":"","kind":"Service"}]}""",
            "BackendTLSPolicy" => """{"targetRefs":[{"group":"","kind":"Service","name":"example-service"}],"validation":{"hostname":"example.com","wellKnownCACertificates":"System"}}""",
            "ListenerSet" or "XListenerSet" => """{"parentRef":{"name":"example-gateway"},"listeners":[{"name":"http","protocol":"HTTP","port":80}]}""",
            _ => null,
        };

        if (json is null)
        {
            spec = default;
            return false;
        }

        using var document = JsonDocument.Parse(json);
        spec = document.RootElement.Clone();
        return true;
    }
}
