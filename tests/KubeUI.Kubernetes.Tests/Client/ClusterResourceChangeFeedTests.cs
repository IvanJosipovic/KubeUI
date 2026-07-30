using KubeUI.Testing;
using Shouldly;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Tests.Client;

[Trait("Category", "Kind")]
public sealed class ClusterResourceChangeFeedTests
{
    [Theory, KubernetesBackendDataAttribute]
    public async Task ConnectResources_tracks_existing_and_late_seeded_source_caches(KubernetesBackend backend)
    {
        await using var harness = await KubernetesScenarioHarnessFactory.CreateAsync(
            backend,
            TestContext.Current.CancellationToken);
        var runtime = harness.Cluster;
        await runtime.SeedResource<V1Service>(true);
        await runtime.SeedResource<V1Pod>(true);
        await runtime.SeedResource<V1ConfigMap>(true);
        V1Service existingService = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "existing", NamespaceProperty = "default" },
        };
        await runtime.AddOrUpdateResource(existingService);

        List<ResourceChange> changes = [];
        using IDisposable subscription = ((IClusterRuntime)runtime).ConnectResources().Subscribe(changes.Add);

        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Service service
                && service.Name() == "existing"
                && change.EventType == WatchEventType.Added),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        V1Pod pod = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "web", NamespaceProperty = "default" } };
        await runtime.AddOrUpdateResource(pod);
        V1Pod updatedPod = new()
        {
            ApiVersion = pod.ApiVersion,
            Kind = pod.Kind,
            Metadata = new V1ObjectMeta
            {
                Name = pod.Name(),
                NamespaceProperty = pod.Namespace(),
                Uid = pod.Uid(),
                ResourceVersion = pod.ResourceVersion(),
                Labels = new Dictionary<string, string> { ["version"] = "two" },
            },
        };
        await runtime.AddOrUpdateResource(updatedPod);
        await runtime.DeleteResource(updatedPod);

        V1ConfigMap configMap = new() { ApiVersion = "v1", Kind = V1ConfigMap.KubeKind, Metadata = new() { Name = "settings", NamespaceProperty = "default" } };
        await runtime.AddOrUpdateResource(configMap);

        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1ConfigMap && change.EventType == WatchEventType.Added),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Pod && change.EventType == WatchEventType.Modified),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Pod && change.EventType == WatchEventType.Deleted),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Added);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Modified);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Deleted);
    }
}
