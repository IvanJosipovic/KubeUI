using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class GRPCRouteConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "GRPCRoute";

    protected override string Template => """
            kind: GRPCRoute
            metadata:
              name: example
            spec:
              parentRefs:
                - name: example-gateway
              rules:
                - backendRefs:
                    - name: example-service
                      port: 50051
            """;
}
