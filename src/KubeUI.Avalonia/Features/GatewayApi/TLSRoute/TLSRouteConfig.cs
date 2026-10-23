using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class TLSRouteConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "TLSRoute";

    protected override string Template => """
            kind: TLSRoute
            metadata:
              name: example
            spec:
              parentRefs:
                - name: example-gateway
              hostnames:
                - example.com
              rules:
                - backendRefs:
                    - name: example-service
                      port: 443
            """;
}
