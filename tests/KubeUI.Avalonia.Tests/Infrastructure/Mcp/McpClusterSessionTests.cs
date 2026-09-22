using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpClusterSessionTests
{
    [AvaloniaFact]
    public async Task list_resources_honors_request_cancellation_during_resource_readiness()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 10, cancellation.Token));
    }

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
