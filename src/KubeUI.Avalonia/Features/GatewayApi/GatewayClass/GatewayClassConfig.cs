using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class GatewayClassConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "GatewayClass";

    protected override string Template => """
            kind: GatewayClass
            metadata:
              name: example
            spec:
              controllerName: example.com/gateway-controller
            """;
}
