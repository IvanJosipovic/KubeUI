using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XBackendConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => ExperimentalGroup;

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
