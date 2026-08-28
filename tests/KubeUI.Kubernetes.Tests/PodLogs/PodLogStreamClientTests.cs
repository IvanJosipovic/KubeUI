using k8s.Models;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.PodLogs;

public sealed class PodLogStreamClientTests
{
    [Fact]
    public async Task OpenAsync_validates_required_arguments_and_cluster_client()
    {
        PodLogStreamClient client = new();
        PodLogReadOptions options = new("default", "pod", "app", false, false, true, 100);

        Should.Throw<ArgumentNullException>(() => client.OpenAsync(null!, options));

        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        Should.Throw<ArgumentNullException>(() => client.OpenAsync(harness.Cluster, null!));

        harness.Cluster.Client = null;
        Should.Throw<InvalidOperationException>(() => client.OpenAsync(harness.Cluster, options))
            .Message.ShouldBe("The cluster client is not available.");
    }

    [Fact]
    public async Task OpenAsync_forwards_all_log_options_to_the_cluster_client()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        await harness.Cluster.Connect();
        PodLogStreamClient client = new();
        PodLogReadOptions options = new("default", "pod", "app", true, true, true, 25);
        await harness.Cluster.AddOrUpdateResource(new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
        });

        await using Stream stream = await client.OpenAsync(
            harness.Cluster,
            options,
            TestContext.Current.CancellationToken);

        stream.ShouldNotBeNull();
    }
}
