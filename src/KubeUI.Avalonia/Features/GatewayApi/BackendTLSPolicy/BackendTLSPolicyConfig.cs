using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class BackendTLSPolicyConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.k8s.io";

    public override string? ApiVersion => "v1";

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
