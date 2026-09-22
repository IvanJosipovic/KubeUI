using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodMetricCellTests
{
    [AvaloniaFact]
    public async Task cpu_cell_renders_async_prometheus_value()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("cpuUsage", 1.234),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var pod = CreatePod();
        var cell = new PodMetricCPUCellView(fixture.RefreshClock) { DataContext = pod };
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => cell.Text == "1.234c",
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        cell.Text.ShouldBe("1.234c");
        queryClient.Queries.ShouldBe(1);
    }

    [AvaloniaFact]
    public async Task memory_cell_renders_async_prometheus_value()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            Result = CreateMetricResult("memoryUsage", 1_048_576),
        };
        await using var fixture = await MetricCellFixture.CreateAsync(queryClient);
        var cell = new PodMetricMemoryCellView(fixture.RefreshClock) { DataContext = CreatePod() };
        using var window = Application.Current.CreateTestWindow(content: cell);

        window.Show();
        cell.Initialize(fixture.Workspace);

        await TestWait.UntilAsync(
            () => cell.Text == "1 MB",
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        cell.Text.ShouldBe("1 MB");
        queryClient.Queries.ShouldBe(1);
    }

    private static V1Pod CreatePod() => new()
    {
        Metadata = new V1ObjectMeta
        {
            Name = "metrics-pod",
            NamespaceProperty = "default",
        },
    };

    private static MetricResultSet CreateMetricResult(string queryName, double value) => new()
    {
        Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
        {
            [queryName] =
            [
                new MetricSeries
                {
                    Name = queryName,
                    Points = [new MetricPoint(DateTimeOffset.UtcNow, value)],
                },
            ],
        },
    };

    private sealed class FakePrometheusQueryClient : IPrometheusQueryClient
    {
        public MetricResultSet? Result { get; init; }

        public int Queries { get; private set; }

        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
            Cluster cluster,
            ResolvedPrometheusEndpoint endpoint,
            string query,
            DateTimeOffset start,
            DateTimeOffset end,
            int stepSeconds,
            CancellationToken cancellationToken = default)
        {
            Queries++;
            var value = Result is not null && Result.Metrics.TryGetValue(query, out var metricSeries)
                ? metricSeries.SelectMany(static item => item.Points).LastOrDefault()
                : null;
            return Task.FromResult<PrometheusClientQueryRangeResponse?>(value == null
                ? null
                : new PrometheusClientQueryRangeResponse
                {
                    Status = "success",
                    Data = new PrometheusClientQueryRangeResponse.DataObject
                    {
                        ResultType = "matrix",
                        Result =
                        [
                            new PrometheusClientQueryRangeResponse.ResultObject
                            {
                                Metric = new Dictionary<string, string>(),
                                Values = [(value.Timestamp, value.Value)],
                            },
                        ],
                    },
                });
        }

        public Task ResetAsync() => Task.CompletedTask;
    }

    private sealed class MetricCellFixture : IAsyncDisposable
    {
        private readonly MetricsService _metricsService;
        private readonly Cluster _cluster;

        private MetricCellFixture(Cluster cluster, ClusterWorkspace workspace, MetricsService metricsService, IUiRefreshClock refreshClock, FakePrometheusQueryClient queryClient)
        {
            _cluster = cluster;
            Workspace = workspace;
            _metricsService = metricsService;
            RefreshClock = refreshClock;
            QueryClient = queryClient;
        }

        public ClusterWorkspace Workspace { get; }

        public IUiRefreshClock RefreshClock { get; }

        public FakePrometheusQueryClient QueryClient { get; }

        public static async Task<MetricCellFixture> CreateAsync(FakePrometheusQueryClient queryClient)
        {
            var settings = new MetricCellSettingsStore();
            var services = Application.Current.GetTestServices();
            var metricsService = new MetricsService(
                NullLogger<MetricsService>.Instance,
                settings,
                [new ExternalPrometheusProvider()],
                queryClient);
            var cluster = new Cluster(
                NullLogger<Cluster>.Instance,
                services.GetRequiredService<ILoggerFactory>(),
                new ClusterModelCatalog(services.GetRequiredService<KubernetesModelCatalog>()),
                settings,
                services,
                new ImmediateThreadDispatcher(),
                metricsService)
            {
                Name = "metrics-cell-cluster",
                Client = new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" }),
            };
            settings.MetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
            settings.MetricsSettings.PrometheusProviderKind = PrometheusProviderKind.External;
            settings.MetricsSettings.PrometheusDirectUrl = "http://prometheus.example";
            await metricsService.InitializeAsync(cluster);
            var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, cluster);
            return new MetricCellFixture(
                cluster,
                workspace,
                metricsService,
                services.GetRequiredService<IUiRefreshClock>(),
                queryClient);
        }

        public async ValueTask DisposeAsync()
        {
            Workspace.Dispose();
            await _cluster.DisposeAsync();
            _metricsService.Dispose();
        }
    }

    private sealed class MetricCellSettingsStore : IClusterSettingsStore
    {
        public ClusterMetricsSettings MetricsSettings { get; } = new();

        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster) => [];

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster) => MetricsSettings;

        public void Persist()
        {
        }
    }
}
