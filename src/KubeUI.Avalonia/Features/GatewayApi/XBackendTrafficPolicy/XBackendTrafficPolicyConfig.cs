using FluentIcons.Common;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendTrafficPolicyConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [
            new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null),
            new NavigationGroup("experimental", Assets.Resources.NavigationView_GatewayApiExperimental!, int.MaxValue, Icon.Beaker),
        ]);
    public override string? ApiGroup => "gateway.networking.x-k8s.io";

    public override string? ApiVersion => "v1alpha1";

    public override string? KindName => "XBackendTrafficPolicy";

    protected override string Template => """
            kind: XBackendTrafficPolicy
            metadata:
              name: example
            spec:
              targetRefs:
                - group: gateway.networking.k8s.io
                  kind: HTTPRoute
                  name: example-route
            """;
}
