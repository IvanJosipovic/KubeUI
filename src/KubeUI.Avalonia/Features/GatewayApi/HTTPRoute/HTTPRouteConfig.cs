using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class HTTPRouteConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
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
