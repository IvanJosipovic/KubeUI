using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendTrafficPolicyConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

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
