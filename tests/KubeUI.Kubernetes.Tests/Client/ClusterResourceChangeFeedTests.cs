using KubeUI.Testing;
using Shouldly;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Tests.Client;

public sealed class ClusterResourceChangeFeedTests
{
    [Fact]
    public async Task ConnectResources_tracks_existing_and_late_seeded_source_caches()
    {
        TestClusterRuntime runtime = new();
        V1Service existingService = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "existing", NamespaceProperty = "demo" },
        };
        await runtime.AddOrUpdateResource(existingService);

        List<ResourceChange> changes = [];
        using IDisposable subscription = ((IClusterRuntime)runtime).ConnectResources().Subscribe(changes.Add);

        changes.Any(change => change.Resource is V1Service service
            && service.Name() == "existing"
            && change.EventType == WatchEventType.Added).ShouldBeTrue();

        V1Pod pod = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "web", NamespaceProperty = "demo" } };
        await runtime.AddOrUpdateResource(pod);
        pod.Metadata!.Labels = new Dictionary<string, string> { ["version"] = "two" };
        await runtime.AddOrUpdateResource(pod);
        await runtime.DeleteResource(pod);

        V1ConfigMap configMap = new() { ApiVersion = "v1", Kind = V1ConfigMap.KubeKind, Metadata = new() { Name = "settings", NamespaceProperty = "demo" } };
        await runtime.AddOrUpdateResource(configMap);

        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Added);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Modified);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Deleted);
        changes.ShouldContain(change => change.Resource is V1ConfigMap && change.EventType == WatchEventType.Added);
    }
}
