using FluentIcons.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [
            new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null),
            new NavigationGroup("experimental", Assets.Resources.NavigationView_GatewayApiExperimental!, int.MaxValue, Icon.Beaker),
        ]);
    public override string? ApiGroup => "gateway.networking.x-k8s.io";

    public override string? KindName => "XBackend";

    protected override string Template => """
            kind: XBackend
            metadata:
              name: example
            spec:
              type: ExternalHostname
              port:
                port: 443
              externalHostname:
                hostname: example.com
            """;
}
