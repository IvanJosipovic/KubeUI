using KubeUI.Features.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Features.GatewayApi;

internal sealed class XMeshConfig : GatewayApiCustomResourceConfig
{
    public override string? ApiGroup => ExperimentalGroup;

    public override string? KindName => "XMesh";

    protected override string Template => """
            kind: XMesh
            metadata:
              name: example
            spec:
              controllerName: example.com/controller
            """;
}
