using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class ListenerSetConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1";

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
