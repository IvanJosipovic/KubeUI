using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class BackendTLSPolicyConfig : CustomResourceConfig
{
    public override NavigationPlacement Navigation => NavigationPlacement.InSection(
        NavigationSections.NetworkId,
        [new NavigationGroup("gateway", Assets.Resources.NavigationView_GatewayApi!, 0, null)]);
    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? KindName => "BackendTLSPolicy";

    protected override string Template => """
            kind: BackendTLSPolicy
            metadata:
              name: example
            spec:
              targetRefs:
                - group: ''
                  kind: Service
                  name: example-service
              validation:
                hostname: example.com
                wellKnownCACertificates: System
            """;
}
