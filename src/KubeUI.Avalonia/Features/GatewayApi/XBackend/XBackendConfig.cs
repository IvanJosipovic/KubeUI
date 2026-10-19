using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.x-k8s.io";

    public override string? ApiVersion => "v1alpha1";

    public override string? KindName => "XBackend";

    protected override string Template => """
            kind: XBackend
            metadata:
              name: example
            spec:
              type: ExternalHostname
              port:
                port: 443
              externalHostname:
                hostname: example.com
            """;
}
