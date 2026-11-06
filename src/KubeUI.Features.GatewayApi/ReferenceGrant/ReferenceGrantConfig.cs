using KubeUI.Features.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Features.GatewayApi;

internal sealed class ReferenceGrantConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => StableGroup;

    public override string? KindName => "ReferenceGrant";

    protected override string Template => """
            kind: ReferenceGrant
            metadata:
              name: example
            spec:
              from:
                - group: gateway.networking.k8s.io
                  kind: HTTPRoute
                  namespace: default
              to:
                - group: ''
                  kind: Service
            """;
}
