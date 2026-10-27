using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class ListenerSetConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "ListenerSet";

    protected override string Template => """
            kind: ListenerSet
            metadata:
              name: example
            spec:
              parentRef:
                name: example-gateway
              listeners:
                - name: http
                  protocol: HTTP
                  port: 80
            """;
}
