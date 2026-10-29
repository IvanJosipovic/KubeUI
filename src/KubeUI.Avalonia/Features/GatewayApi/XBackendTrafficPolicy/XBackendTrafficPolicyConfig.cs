using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendTrafficPolicyConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => ExperimentalGroup;

    public override string? KindName => "XBackendTrafficPolicy";

    protected override string Template => """
            kind: XBackendTrafficPolicy
            metadata:
              name: example
            spec:
              targetRefs:
                - group: gateway.networking.k8s.io
                  kind: HTTPRoute
                  name: example-route
            """;
}
