using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class HTTPRouteConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1";

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
