using Avalonia.Headless.XUnit;
using DynamicData;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Kubernetes;
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

        var page = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 10);

        page.Resources.Select(resource => resource.Name).ShouldBe(["api"]);
        page.NextCursor.ShouldBeNull();

        var emptyPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "missing", 10);
        emptyPage.Resources.ShouldBeEmpty();
        emptyPage.NextCursor.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task list_resources_pages_by_stable_key_while_informer_cache_changes()
    {
        var firstPod = Pod("a");
        var lastPod = Pod("c");
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Resources = [firstPod, lastPod];
        });
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var firstPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1);

        firstPage.Resources.Select(resource => resource.Name).ShouldBe(["a"]);
        firstPage.NextCursor.ShouldNotBeNull();
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "other", 1,
            cursor: firstPage.NextCursor));
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "apps/v1", "Pod", "default", 1,
            cursor: firstPage.NextCursor));
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Deployment", "default", 1,
            cursor: firstPage.NextCursor));

        var podContainer = workspace.Runtime.Objects.Values.OfType<ContainerClass<V1Pod>>().Single();
        podContainer.Items.AddOrUpdate(Pod("b"));

        var secondPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1,
            cursor: firstPage.NextCursor);
        secondPage.Resources.Select(resource => resource.Name).ShouldBe(["b"]);
        secondPage.NextCursor.ShouldNotBeNull();

        var thirdPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1,
            cursor: secondPage.NextCursor);
        thirdPage.Resources.Select(resource => resource.Name).ShouldBe(["c"]);
        thirdPage.NextCursor.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task list_resources_rejects_out_of_range_limits_and_unknown_kinds()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", null, 0));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", null, 501));
        await Should.ThrowAsync<InvalidOperationException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "UnknownKind", null, 10));
    }

    private static V1Pod Pod(string name) => new()
    {
        Metadata = new V1ObjectMeta
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = $"uid-{name}",
        },
    };
}
