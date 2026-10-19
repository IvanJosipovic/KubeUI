using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.GatewayApi;

internal sealed class XMeshConfig : GatewayApiCustomResourceConfig
{
    protected override string NavigationSectionId => NavigationSections.NetworkId;

    public override string? ApiGroup => "gateway.networking.x-k8s.io";

    public override string? ApiVersion => "v1alpha1";

    public override string? KindName => "XMesh";

    protected override string Template => """
            kind: XMesh
            metadata:
              name: example
            spec:
              controllerName: example.com/controller
            """;
}
