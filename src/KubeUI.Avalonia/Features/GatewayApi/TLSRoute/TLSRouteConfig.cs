using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class TLSRouteConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
    public override string? ApiGroup => "gateway.networking.k8s.io";

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
