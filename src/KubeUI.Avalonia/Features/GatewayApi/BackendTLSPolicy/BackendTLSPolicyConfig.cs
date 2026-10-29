using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class BackendTLSPolicyConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "BackendTLSPolicy";

    protected override string Template => """
            kind: BackendTLSPolicy
            metadata:
              name: example
            spec:
              targetRefs:
                - group: ''
                  kind: Service
                  name: example-service
              validation:
                hostname: example.com
                wellKnownCACertificates: System
            """;
}
