using System.Collections.Concurrent;
using System.Reflection;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class ClusterAuthTests
{
    [Fact]
    public void cani_returns_false_when_permission_review_has_not_been_cached_yet()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());

        cluster.CanI(typeof(V1Pod), Verb.Create, subresource: "portforward").ShouldBeFalse();
    }

    [Fact]
    public async Task cani_any_namespace_uses_namespace_scoped_permission_when_cluster_scope_is_denied()
    {
        var cluster = new TestClusterRuntime();

        cluster.SetPermission<V1Pod>(Verb.Create, false, subresource: "portforward");
        cluster.SetPermission<V1Pod>(Verb.Create, true, "my-app", "portforward");

        await cluster.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "my-app" }
        });

        cluster.CanIAnyNamespace<V1Pod>(Verb.Create, "portforward").ShouldBeTrue();
    }

    [Fact]
    public void removing_seeded_resource_container_also_clears_cached_seed_task()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());

        var kind = GroupApiVersionKind.From<V1Pod>();
        cluster.Objects[kind] = new ContainerClass<V1Pod>
        {
            Initialized = true,
        };

        var seedTasksField = typeof(Cluster).GetField("_seedTasks", BindingFlags.Instance | BindingFlags.NonPublic);
        seedTasksField.ShouldNotBeNull();

        var seedTasks = seedTasksField!.GetValue(cluster).ShouldBeOfType<ConcurrentDictionary<GroupApiVersionKind, Lazy<Task>>>();
        seedTasks[kind] = new Lazy<Task>(() => Task.CompletedTask);

        var invalidateSeededResourceMethod = typeof(Cluster).GetMethod("InvalidateSeededResource", BindingFlags.Instance | BindingFlags.NonPublic);
        invalidateSeededResourceMethod.ShouldNotBeNull();

        var invalidated = invalidateSeededResourceMethod!.Invoke(cluster, [typeof(V1Pod)]).ShouldBeOfType<bool>();

        invalidated.ShouldBeTrue();
        cluster.Objects.ContainsKey(kind).ShouldBeFalse();
        seedTasks.ContainsKey(kind).ShouldBeFalse();
    }

    private sealed class TestClusterSettingsStore : IClusterSettingsStore
    {
        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
        {
            return [];
        }
    }
}
