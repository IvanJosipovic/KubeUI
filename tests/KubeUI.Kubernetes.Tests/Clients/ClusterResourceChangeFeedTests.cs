using Shouldly;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Tests.Clients;

[Trait("Category", "Kind")]
public sealed class ClusterResourceChangeFeedTests
{
    [Theory, KubernetesBackendData]
    public async Task ConnectResources_tracks_existing_and_late_seeded_source_caches(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        var runtime = harness.Cluster;
        await runtime.Connect();
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<V1Service>(Verb.List);
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<V1Service>(Verb.Watch);
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<V1Secret>(Verb.List);
        await runtime.Permissions.UpdatePermissionsAllNamespaceAsync<V1Secret>(Verb.Watch);
        await runtime.SeedResource<V1Service>(true);
        await runtime.SeedResource<V1Secret>(true);
        V1Service existingService = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "existing", NamespaceProperty = "default" },
            Spec = new V1ServiceSpec
            {
                Ports = [new V1ServicePort { Port = 80 }],
            },
        };
        await runtime.AddOrUpdateResource(existingService);

        List<ResourceChange> changes = [];
        using var subscription = ((IClusterRuntime)runtime).ConnectResources().Subscribe(changes.Add);

        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Service service
                && service.Name() == "existing"
                && change.EventType == WatchEventType.Added),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        V1Secret secret = new()
        {
            ApiVersion = "v1",
            Kind = V1Secret.KubeKind,
            Metadata = new() { Name = "web", NamespaceProperty = "default" },
            StringData = new Dictionary<string, string> { ["value"] = "one" },
        };
        await runtime.AddOrUpdateResource(secret);
        V1Secret updatedSecret = new()
        {
            ApiVersion = secret.ApiVersion,
            Kind = secret.Kind,
            Metadata = new V1ObjectMeta
            {
                Name = secret.Name(),
                NamespaceProperty = secret.Namespace(),
                Uid = secret.Uid(),
                ResourceVersion = secret.ResourceVersion(),
                Labels = new Dictionary<string, string> { ["version"] = "two" },
            },
            StringData = new Dictionary<string, string> { ["value"] = "two" },
        };
        await runtime.AddOrUpdateResource(updatedSecret);
        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Secret && change.EventType == WatchEventType.Modified),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await runtime.DeleteResource(updatedSecret);
        await TestWait.UntilAsync(
            () => changes.Any(change => change.Resource is V1Secret && change.EventType == WatchEventType.Deleted),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Added);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Modified);
        changes.Select(change => change.EventType).ShouldContain(WatchEventType.Deleted);
    }
}
