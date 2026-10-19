using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class ReferenceGrantConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

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
