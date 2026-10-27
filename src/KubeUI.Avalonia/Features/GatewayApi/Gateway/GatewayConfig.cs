using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class GatewayConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

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
