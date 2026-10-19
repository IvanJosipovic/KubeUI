using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class ReferenceGrantConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1beta1";

    public override string? KindName => "ReferenceGrant";

    protected override string Template => """
            kind: ReferenceGrant
            metadata:
              name: example
            spec:
              from:
                - group: gateway.networking.k8s.io
                  kind: HTTPRoute
                  namespace: default
              to:
                - group: ''
                  kind: Service
            """;
}
