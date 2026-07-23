using System.Reflection;
using System.Collections.ObjectModel;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using KubeUI.Testing;
using KubeUI.Kubernetes.Tests.Infra;
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
    public async Task globally_allowed_namespaced_permission_skips_namespace_reviews()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());
        var namespaces = new ObservableCollection<V1Namespace>
        {
            new() { Metadata = new V1ObjectMeta { Name = "my-app" } }
        };
        cluster.Namespaces = new ReadOnlyObservableCollection<V1Namespace>(namespaces);

        var setPermissionResult = typeof(Cluster).GetMethod("SetPermissionResult", BindingFlags.Instance | BindingFlags.NonPublic);
        setPermissionResult.ShouldNotBeNull();
        setPermissionResult!.Invoke(cluster, [GroupApiVersionKind.From<V1Pod>(), "list", null, null, true]);

        await cluster.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.List);

        cluster.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
    }

    [Fact]
    public void removing_seeded_resource_container_removes_the_container()
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

        var invalidateSeededResourceMethod = typeof(Cluster).GetMethod("InvalidateSeededResource", BindingFlags.Instance | BindingFlags.NonPublic);
        invalidateSeededResourceMethod.ShouldNotBeNull();

        var invalidated = invalidateSeededResourceMethod!.Invoke(cluster, [typeof(V1Pod)]).ShouldBeOfType<bool>();

        invalidated.ShouldBeTrue();
        cluster.Objects.ContainsKey(kind).ShouldBeFalse();
    }

    [Fact]
    public async Task custom_resource_definition_processing_is_serialized()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());
        cluster.Connected = true;

        var first = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(SharedScenarioData.CustomResourceDefinitionYaml);
        var second = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(SharedScenarioData.CustomResourceDefinitionYaml);
        second.Spec!.Names!.Kind = "UpdatedTest";

        var firstEntered = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReady = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var readyCount = 0;
        cluster.OnCustomResourceDefinitionReady += crd =>
        {
            if (Interlocked.Increment(ref readyCount) == 1)
            {
                firstEntered.TrySetResult(null);
                releaseFirst.Task.GetAwaiter().GetResult();
            }
            else
            {
                secondReady.TrySetResult(null);
            }
        };

        var queueMethod = typeof(Cluster).GetMethod("QueueCustomResourceDefinition", BindingFlags.Instance | BindingFlags.NonPublic);
        queueMethod.ShouldNotBeNull();

        queueMethod!.Invoke(cluster, [first]);
        await firstEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        queueMethod.Invoke(cluster, [second]);

        await Task.Delay(100);
        Volatile.Read(ref readyCount).ShouldBe(1);

        releaseFirst.TrySetResult(null);
        await secondReady.Task.WaitAsync(TimeSpan.FromSeconds(5));
        readyCount.ShouldBe(2);
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
