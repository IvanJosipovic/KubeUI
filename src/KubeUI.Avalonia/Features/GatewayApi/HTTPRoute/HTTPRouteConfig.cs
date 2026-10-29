using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class HTTPRouteConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "HTTPRoute";

    protected override string Template => """
            kind: HTTPRoute
            metadata:
              name: example
            spec:
              parentRefs:
                - name: example-gateway
              hostnames:
                - example.com
              rules:
                - matches:
                    - path:
                        type: PathPrefix
                        value: /
                  backendRefs:
                    - name: example-service
                      port: 80
            """;
}
