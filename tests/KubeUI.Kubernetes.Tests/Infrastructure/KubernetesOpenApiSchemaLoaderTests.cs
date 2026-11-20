using System.Net;
using System.Net.Http;
using KubeUI.Kubernetes.Client;
using KubeUI.Testing.Kubernetes.Bootstrap;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.Logging;
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
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, new DebugLogger());

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

    [Fact]
    public async Task EnsureAsync_reports_partial_schema_loads_as_failures()
    {
        var config = TestClusterConfig.Fake();
        config.Name = "openapi-partial-test";
        config.HttpHandlerFactory = () => [new FailSchemaDocumentHandler()];
        await using var cluster = await new TestClusterGenerator().CreateAsync(
            config,
            TestContext.Current.CancellationToken);
        await cluster.Cluster.Connect().WaitAsync(TestContext.Current.CancellationToken);
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, new DebugLogger());

        var loaded = await loader.EnsureAsync(
            (k8s.Kubernetes)cluster.Client!, "fake", TestContext.Current.CancellationToken);

        loaded.ShouldBeFalse();
        // A failed load is not marked attempted, so a later call can retry the document.
        await Should.NotThrowAsync(() => loader.EnsureAsync(
            (k8s.Kubernetes)cluster.Client!,
            "fake",
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EnsureAsync_reports_invalid_index_as_failure()
    {
        var config = TestClusterConfig.Fake();
        config.Name = "openapi-invalid-index-test";
        config.HttpHandlerFactory = () => [new InvalidOpenApiIndexHandler()];
        await using var cluster = await new TestClusterGenerator().CreateAsync(
            config,
            TestContext.Current.CancellationToken);
        await cluster.Cluster.Connect().WaitAsync(TestContext.Current.CancellationToken);
        var catalog = new KubernetesOpenApiSchemaCatalog();
        using var loader = new KubernetesOpenApiSchemaLoader(catalog, new DebugLogger());

        var loaded = await loader.EnsureAsync(
            (k8s.Kubernetes)cluster.Client!, "fake", TestContext.Current.CancellationToken);

        loaded.ShouldBeFalse();
    }

    private sealed class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed class InvalidOpenApiIndexHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath == "/openapi/v3")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"paths\":null}"),
                    RequestMessage = request,
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FailSchemaDocumentHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.StartsWith("/openapi/v3/", StringComparison.Ordinal) == true)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    RequestMessage = request,
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
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
