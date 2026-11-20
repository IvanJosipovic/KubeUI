using KubeUI.Kubernetes.Client;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Infrastructure;

public sealed class KubernetesOpenApiSchemaLoaderTests
{
    [Fact]
    public async Task EnsureAsync_loads_active_documents_for_served_group_versions()
    {
        await using var cluster = await CreateClusterAsync();
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, NullLogger.Instance);

        var loaded = await loader.EnsureAsync((k8s.Kubernetes)cluster.Client!, "fake", TestContext.Current.CancellationToken);

        loaded.ShouldBeTrue();
        catalog.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task EnsureAsync_returns_quietly_when_the_index_is_unavailable()
    {
        await using var cluster = await CreateClusterAsync(config =>
        {
            config.ThrowOnConnect = true;
        });
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, NullLogger.Instance);

        var loaded = await loader.EnsureAsync((k8s.Kubernetes)cluster.Client!, "fake", TestContext.Current.CancellationToken);

        loaded.ShouldBeFalse();
        catalog.Count.ShouldBe(0);
        // The loader surfaces failures quietly; a follow-up call may retry because nothing loaded.
        await Should.NotThrowAsync(() => loader.EnsureAsync(
            (k8s.Kubernetes)cluster.Client!,
            "fake",
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EnsureAsync_performs_one_load_attempt_per_call()
    {
        await using var cluster = await new TestClusterGenerator().CreateAsync(
            TestClusterConfig.Fake(),
            TestContext.Current.CancellationToken);
        cluster.FakeApi!.OpenApiV3IndexFailuresRemaining = 5;
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, NullLogger.Instance);

        var loaded = await loader.EnsureAsync((k8s.Kubernetes)cluster.Client!, "fake", TestContext.Current.CancellationToken);

        var indexRequests = cluster.FakeApi.RequestUris.Count(uri => uri?.AbsolutePath == "/openapi/v3");
        indexRequests.ShouldBe(1);
        loaded.ShouldBeFalse();
        catalog.Count.ShouldBe(0);
    }

    [Fact]
    public async Task EnsureAsync_skips_loading_after_a_successful_load()
    {
        await using var cluster = await CreateClusterAsync();
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, NullLogger.Instance);
        var client = (k8s.Kubernetes)cluster.Client!;

        var firstLoad = await loader.EnsureAsync(client, "fake", TestContext.Current.CancellationToken);
        var indexRequestsAfterFirstLoad = cluster.FakeApi!.RequestUris.Count(uri => uri?.AbsolutePath == "/openapi/v3");

        var secondLoad = await loader.EnsureAsync(client, "fake", TestContext.Current.CancellationToken);

        firstLoad.ShouldBeTrue();
        secondLoad.ShouldBeTrue();
        cluster.FakeApi.RequestUris.Count(uri => uri?.AbsolutePath == "/openapi/v3")
            .ShouldBe(indexRequestsAfterFirstLoad);
    }

    private static async Task<TestCluster> CreateClusterAsync(Action<TestClusterConfig>? configure = null)
    {
        var config = TestClusterConfig.Fake();
        config.Name = "openapi-loader-test";
        configure?.Invoke(config);
        var cluster = await new TestClusterGenerator().CreateAsync(config, TestContext.Current.CancellationToken);
        try
        {
            await cluster.Cluster.Connect().WaitAsync(TestContext.Current.CancellationToken);
            return cluster;
        }
        catch
        {
            await cluster.DisposeAsync();
            throw;
        }
    }
}
