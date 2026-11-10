using KubeUI.Features.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Features.GatewayApi;

internal sealed class UDPRouteConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "UDPRoute";

    protected override string Template => """
            kind: UDPRoute
            metadata:
              name: example
            spec:
              parentRefs:
                - name: example-gateway
              rules:
                - backendRefs:
                    - name: example-service
                      port: 8080
            """;
}
