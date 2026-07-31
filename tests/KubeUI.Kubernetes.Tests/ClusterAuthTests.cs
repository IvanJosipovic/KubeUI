using System.Reflection;
using System.Collections.ObjectModel;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
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
            new KubernetesTestSettingsStore(),
            new ServiceCollection().BuildServiceProvider());

        cluster.CanI(typeof(V1Pod), Verb.Create, subresource: "portforward").ShouldBeFalse();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task cani_any_namespace_uses_namespace_scoped_permission_when_cluster_scope_is_denied(KubernetesBackend backend)
    {
        await using var harness = await KubernetesScenarioHarnessFactory.CreateAsync(
            backend,
            TestContext.Current.CancellationToken);
        var cluster = (Cluster)await harness.CreateLimitedAccessClusterAsync(
            includeNamespaceFallback: true,
            cancellationToken: TestContext.Current.CancellationToken);

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
            new KubernetesTestSettingsStore(),
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
            new KubernetesTestSettingsStore(),
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
    public async Task resource_container_runs_only_one_seed_factory_for_concurrent_requests()
    {
        var container = new ContainerClass<V1Pod>();
        var seedStarted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSeed = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var seedCount = 0;

        Task SeedAsync()
        {
            Interlocked.Increment(ref seedCount);
            seedStarted.SetResult(null);
            return releaseSeed.Task;
        }

        var requests = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(
                async () => await container.GetOrCreateSeedTask(SeedAsync).Value,
                TestContext.Current.CancellationToken))
            .ToArray();

        await seedStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        seedCount.ShouldBe(1);

        releaseSeed.SetResult(null);
        await Task.WhenAll(requests);
        seedCount.ShouldBe(1);
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
            new KubernetesTestSettingsStore(),
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
        await firstEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        queueMethod.Invoke(cluster, [second]);

        Volatile.Read(ref readyCount).ShouldBe(1);

        releaseFirst.TrySetResult(null);
        await secondReady.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        readyCount.ShouldBe(2);
    }

}
