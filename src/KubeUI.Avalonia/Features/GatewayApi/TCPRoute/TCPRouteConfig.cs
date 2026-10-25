using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class TCPRouteConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "TCPRoute";

    protected override string Template => """
            kind: TCPRoute
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
