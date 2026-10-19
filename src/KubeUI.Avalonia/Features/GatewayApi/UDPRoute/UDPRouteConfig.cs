using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class UDPRouteConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1";

    public override string? KindName => "UDPRoute";

    protected override string Template => """
            kind: UDPRoute
            metadata:
              name: example
            spec:
              parentRefs:
                - name: example-gateway
              rules:
                - backendRefs:
                    - name: example-service
                      port: 8080
            """;
}
