using Avalonia;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpClusterSessionTests
{
    [AvaloniaFact]
    public async Task list_resources_uses_the_real_kubeui_workspace_cache_and_namespace_filter()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" }
        };
        var otherNamespacePod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "worker", NamespaceProperty = "other" }
        };
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Resources = [pod, otherNamespacePod];
        });
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var resources = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 10);

        resources.Select(resource => resource.Metadata?.Name).ShouldBe(["api"]);
    }
}
