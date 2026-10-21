using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class GatewayConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? KindName => "Gateway";

    protected override string Template => """
            kind: Gateway
            metadata:
              name: example
            spec:
              gatewayClassName: example
              listeners:
                - name: http
                  protocol: HTTP
                  port: 80
            """;
}
