using k8s;
using KubernetesCRDModelGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class MetricsServiceTests
{
    [Fact]
    public async Task InitializeAsync_with_external_prometheus_activates_prometheus_backend()
    {
        var queryClient = new FakePrometheusQueryClient();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        var service = CreateMetricsService(settings, queryClient);
        var cluster = CreateCluster("prom-cluster", service, settings);

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeTrue();
        service.ActiveMetricsBackend.Type.ShouldBe(MetricsServiceType.Prometheus);
        service.ActiveMetricsBackend.PrometheusProviderKind.ShouldBe(PrometheusProviderKind.External);
        queryClient.PrepareCalls.ShouldBe(1);
    }

    [Fact]
    public async Task RequestMetricsAsync_returns_fake_transport_data()
    {
        var queryClient = new FakePrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25, 2.5));

        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        var service = CreateMetricsService(settings, queryClient);
        var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);

        var result = await service.RequestMetricsAsync(CreateRequest());

        result.IsEmpty.ShouldBeFalse();
        queryClient.QueryCalls.ShouldBe(1);
        result.Metrics["cpuUsage"].Single().Points.Last().Value.ShouldBe(2.5);
    }

    [Fact]
    public async Task RequestMetricsAsync_reuses_cached_result_for_identical_request()
    {
        var queryClient = new FakePrometheusQueryClient();
        queryClient.EnqueueResponse(CreateSuccessResponse(1.25, 2.5));

        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        var service = CreateMetricsService(settings, queryClient);
        var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);
        var request = CreateRequest();

        _ = await service.RequestMetricsAsync(request);
        _ = await service.RequestMetricsAsync(request);

        queryClient.QueryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task RequestMetricsAsync_enters_cooldown_after_transport_failure()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            ExceptionToThrow = new HttpRequestException("boom"),
        };

        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        var service = CreateMetricsService(settings, queryClient);
        var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);
        var request = CreateRequest();

        var first = await service.RequestMetricsAsync(request);
        var callsAfterFirstRequest = queryClient.QueryCalls;
        var second = await service.RequestMetricsAsync(request);

        first.IsEmpty.ShouldBeTrue();
        second.IsEmpty.ShouldBeTrue();
        callsAfterFirstRequest.ShouldBeGreaterThan(0);
        queryClient.QueryCalls.ShouldBe(callsAfterFirstRequest);
    }

    private static MetricsService CreateMetricsService(TestClusterSettingsStore settings, FakePrometheusQueryClient queryClient)
    {
        return new MetricsService(
            NullLogger<MetricsService>.Instance,
            settings,
            [
                new OperatorPrometheusProvider(),
                new OpenShiftPrometheusProvider(),
                new ManualPrometheusProvider(),
                new ExternalPrometheusProvider(),
            ],
            queryClient);
    }

    private static Cluster CreateCluster(string name, MetricsService metricsService, IClusterSettingsStore settings)
    {
        return new Cluster(
            NullLogger<Cluster>.Instance,
            NullLoggerFactory.Instance,
            new ModelCache(),
            new Generator(),
            settings,
            new ServiceCollection().BuildServiceProvider(),
            metricsService)
        {
            Name = name,
            Client = new k8s.Kubernetes(new KubernetesClientConfiguration
            {
                Host = "http://localhost",
            }),
        };
    }

    private static MetricRequest CreateRequest()
    {
        return new MetricRequest
        {
            Category = MetricCategory.Pods,
            RangeSeconds = 300,
            StepSeconds = 60,
            Frames = 5,
            Queries =
            [
                new MetricQueryDefinition
                {
                    Name = "cpuUsage",
                    Options = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["namespace"] = "default",
                        ["pods"] = "pod-1",
                    },
                },
            ],
        };
    }

    private static PrometheusClientQueryRangeResponse CreateSuccessResponse(double firstValue, double secondValue)
    {
        return new PrometheusClientQueryRangeResponse
        {
            Status = "success",
            Data = new PrometheusClientQueryRangeResponse.DataObject
            {
                ResultType = "matrix",
                Result =
                [
                    new PrometheusClientQueryRangeResponse.ResultObject
                    {
                        Metric = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["pod"] = "pod-1",
                        },
                        Values =
                        [
                            (DateTimeOffset.FromUnixTimeSeconds(60), firstValue),
                            (DateTimeOffset.FromUnixTimeSeconds(120), secondValue),
                        ],
                    },
                ],
            },
        };
    }

    private sealed class FakePrometheusQueryClient : IPrometheusQueryClient
    {
        private readonly Queue<PrometheusClientQueryRangeResponse> _responses = new();

        public int PrepareCalls { get; private set; }

        public int QueryCalls { get; private set; }

        public Exception? ExceptionToThrow { get; set; }

        public void EnqueueResponse(PrometheusClientQueryRangeResponse response)
        {
            _responses.Enqueue(response);
        }

        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            PrepareCalls++;
            return Task.CompletedTask;
        }

        public Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
            Cluster cluster,
            ResolvedPrometheusEndpoint endpoint,
            string query,
            DateTimeOffset start,
            DateTimeOffset end,
            int stepSeconds,
            CancellationToken cancellationToken = default)
        {
            QueryCalls++;

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : null);
        }

        public Task ResetAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestClusterSettingsStore(ClusterMetricsSettings metricsSettings) : IClusterSettingsStore
    {
        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
        {
            return [];
        }

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster)
        {
            return metricsSettings;
        }

        public void Persist()
        {
        }
    }
}
