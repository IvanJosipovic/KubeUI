using System.Collections.Immutable;
using System.Collections.ObjectModel;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

/// <summary>
/// Covers the per-query cache and the in-flight coalescing of <see cref="PrometheusMetricQueries"/>.
/// </summary>
public sealed class PrometheusMetricQueriesTests
{
    [Fact]
    public async Task RequestAsync_caches_a_successful_query_and_reuses_it()
    {
        var queryClient = new RecordingPrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();

        var first = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);
        var second = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);

        first.HadRequestFailures.ShouldBeFalse();
        second.HadRequestFailures.ShouldBeFalse();
        queryClient.QueryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task RequestAsync_coalesces_concurrent_requests_for_the_same_query()
    {
        var queryClient = new RecordingPrometheusQueryClient { WaitForRelease = true };
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();

        var first = queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);
        var second = queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);

        await queryClient.QueryStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        queryClient.Release();

        var results = await Task.WhenAll(first, second);

        results.ShouldAllBe(result => !result.HadRequestFailures);
        queryClient.QueryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task RequestAsync_does_not_cache_a_failed_query()
    {
        var queryClient = new RecordingPrometheusQueryClient
        {
            ExceptionToThrow = new InvalidOperationException("boom"),
            FailuresRemaining = 1,
        };
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();

        var first = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);
        var second = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);

        first.HadRequestFailures.ShouldBeTrue();
        second.HadRequestFailures.ShouldBeFalse();
        queryClient.QueryCalls.ShouldBe(2);
    }

    [Fact]
    public async Task RequestAsync_caches_each_query_of_a_request_independently()
    {
        var queryClient = new RecordingPrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        queryClient.EnqueueResponse(CreateSuccessResponse(2.5));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();
        var request = CreateRequest(
            new MetricQueryDefinition
            {
                Name = "cpuUsage",
                Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["namespace"] = "default", ["pods"] = "pod-1" },
            },
            new MetricQueryDefinition
            {
                Name = "memoryUsage",
                Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["namespace"] = "default", ["pods"] = "pod-1" },
            });

        var first = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), request, CancellationToken.None, CancellationToken.None);
        var second = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), request, CancellationToken.None, CancellationToken.None);

        first.Metrics.Keys.ShouldBe(["cpuUsage", "memoryUsage"], ignoreOrder: true);
        second.Metrics.Keys.ShouldBe(["cpuUsage", "memoryUsage"], ignoreOrder: true);
        queryClient.QueryCalls.ShouldBe(2);
    }

    [Fact]
    public async Task RequestAsync_uses_a_different_cache_entry_for_a_different_time_range()
    {
        var queryClient = new RecordingPrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        queryClient.EnqueueResponse(CreateSuccessResponse(2.5));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();

        _ = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);
        _ = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateWiderRequest(), CancellationToken.None, CancellationToken.None);

        queryClient.QueryCalls.ShouldBe(2);
    }

    [Fact]
    public async Task StopAsync_clears_the_cache_so_the_next_request_queries_again()
    {
        var queryClient = new RecordingPrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25));
        queryClient.EnqueueResponse(CreateSuccessResponse(2.5));
        var queries = new PrometheusMetricQueries(NullLogger.Instance, queryClient, TimeProvider.System);
        await using var cluster = CreateCluster();

        _ = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);
        await queries.StopAsync();
        _ = await queries.RequestAsync(cluster, CreateEndpoint(), CreateProvider(), CreateRequest(), CancellationToken.None, CancellationToken.None);

        queryClient.QueryCalls.ShouldBe(2);
    }

    private static Cluster CreateCluster() => new(
        NullLogger<Cluster>.Instance,
        NullLoggerFactory.Instance,
        new ClusterModelCatalog(new KubernetesModelCatalog()),
        new NoopClusterSettingsStore(),
        new ServiceCollection().BuildServiceProvider(),
        new ImmediateThreadDispatcher(),
        new Serialization.KubernetesJsonSerializer(new System.Text.Json.JsonSerializerOptions(Serialization.KubernetesJsonStaticContext.Default.Options) { TypeInfoResolver = Serialization.KubernetesJsonStaticContext.Default }),
        Moq.Mock.Of<Serialization.IKubernetesYamlSerializer>(),
        new NoopMetricsService())
    {
        Name = "prom-cluster",
        Client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" }),
    };

    private static ResolvedPrometheusEndpoint CreateEndpoint() => new(
        PrometheusProviderKind.External,
        "External",
        true,
        null,
        null,
        null,
        "http://prometheus.example",
        false,
        string.Empty,
        null);

    private static IPrometheusProvider CreateProvider() => new ExternalPrometheusProvider();

    private static MetricRequest CreateRequest(params MetricQueryDefinition[] queries) => new()
    {
        Category = MetricCategory.Pods,
        RangeSeconds = 300,
        StepSeconds = 60,
        Frames = 5,
        Queries = queries.Length > 0
            ? queries
            :
            [
                new MetricQueryDefinition
                {
                    Name = "cpuUsage",
                    Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["namespace"] = "default", ["pods"] = "pod-1" },
                },
            ],
    };

    private static MetricRequest CreateWiderRequest() => new()
    {
        Category = MetricCategory.Pods,
        RangeSeconds = 600,
        StepSeconds = 60,
        Frames = 5,
        Queries =
        [
            new MetricQueryDefinition
            {
                Name = "cpuUsage",
                Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["namespace"] = "default", ["pods"] = "pod-1" },
            },
        ],
    };

    private static PrometheusClientQueryRangeResponse CreateSuccessResponse(double value) => new()
    {
        Status = "success",
        Data = new PrometheusClientQueryRangeResponse.DataObject
        {
            ResultType = "matrix",
            Result =
            [
                new PrometheusClientQueryRangeResponse.ResultObject
                {
                    Metric = new Dictionary<string, string>(StringComparer.Ordinal) { ["pod"] = "pod-1" },
                    Values = [(DateTimeOffset.FromUnixTimeSeconds(60), value)],
                },
            ],
        },
    };

    private sealed class RecordingPrometheusQueryClient : IPrometheusQueryClient
    {
        private readonly Queue<PrometheusClientQueryRangeResponse> _responses = new();
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int QueryCalls { get; private set; }

        public TaskCompletionSource QueryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool WaitForRelease { get; init; }

        public Exception? ExceptionToThrow { get; set; }

        public int FailuresRemaining { get; set; } = -1;

        public void EnqueueResponse(PrometheusClientQueryRangeResponse response) => _responses.Enqueue(response);

        public void Release() => _release.TrySetResult();

        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public async Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
            Cluster cluster,
            ResolvedPrometheusEndpoint endpoint,
            string query,
            DateTimeOffset start,
            DateTimeOffset end,
            int stepSeconds,
            CancellationToken cancellationToken = default)
        {
            QueryCalls++;
            QueryStarted.TrySetResult();

            if (WaitForRelease)
            {
                await _release.Task.WaitAsync(cancellationToken);
            }

            if (ExceptionToThrow is not null && (FailuresRemaining < 0 || FailuresRemaining-- > 0))
            {
                throw ExceptionToThrow;
            }

            return _responses.Count > 0 ? _responses.Dequeue() : null;
        }

        public Task ResetAsync()
        {
            _release.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class NoopClusterSettingsStore : IClusterSettingsStore
    {
        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public bool KubeConfigAutoDiscoveryEnabled { get; set; }

        public void RemoveKubeConfigPath(string path)
        {
        }

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster) => [];

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster) => new();
    }

    private sealed class NoopMetricsService : IMetricsService
    {
        public ObservableCollection<PodMetrics> PodMetrics { get; } = [];

        public ObservableCollection<NodeMetrics> NodeMetrics { get; } = [];

        public ImmutableArray<PodMetrics> GetPodMetricsSnapshot(string? namespaceName, string podName)
            => ImmutableArray<PodMetrics>.Empty;

        public ImmutableArray<NodeMetrics> GetNodeMetricsSnapshot(string nodeName)
            => ImmutableArray<NodeMetrics>.Empty;

        public ImmutableArray<PodMetrics> GetAllPodMetricsSnapshot()
            => ImmutableArray<PodMetrics>.Empty;

        public ImmutableArray<NodeMetrics> GetAllNodeMetricsSnapshot()
            => ImmutableArray<NodeMetrics>.Empty;

        public bool IsMetricsAvailable => false;

        public ActiveMetricsBackend ActiveMetricsBackend => ActiveMetricsBackend.None;

        public Task InitializeAsync(Cluster cluster) => Task.CompletedTask;

        public Task StopAsync() => Task.CompletedTask;

        public Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(MetricResultSet.Empty);

        public Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync()
            => Task.FromResult<IReadOnlyList<MetricProviderInfo>>([]);
    }
}
