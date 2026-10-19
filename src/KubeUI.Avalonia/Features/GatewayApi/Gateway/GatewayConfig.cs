using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class GatewayConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1";

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
