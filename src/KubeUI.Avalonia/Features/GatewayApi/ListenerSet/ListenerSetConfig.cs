using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class ListenerSetConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? KindName => "ListenerSet";

    protected override string Template => """
            kind: ListenerSet
            metadata:
              name: example
            spec:
              parentRef:
                name: example-gateway
              listeners:
                - name: http
                  protocol: HTTP
                  port: 80
            """;
}
