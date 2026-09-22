using k8s;
using k8s.Models;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using KubeUI.Testing.Kubernetes.Transport;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class MetricsServiceTests
{
    [Fact]
    public async Task GetAvailablePrometheusProvidersAsync_returns_registered_providers_in_name_order()
    {
        using var service = CreateMetricsService(new TestClusterSettingsStore(new ClusterMetricsSettings()), new FakePrometheusQueryClient());

        var providers = await service.GetAvailablePrometheusProvidersAsync();

        providers.Select(provider => provider.Kind).ShouldBe([
            PrometheusProviderKind.External,
            PrometheusProviderKind.Manual,
            PrometheusProviderKind.OpenShift,
            PrometheusProviderKind.Operator,
        ]);
        providers.ShouldAllBe(provider => !string.IsNullOrWhiteSpace(provider.Name));
    }

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

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeTrue();
        service.ActiveMetricsBackend.Type.ShouldBe(MetricsServiceType.Prometheus);
        service.ActiveMetricsBackend.PrometheusProviderKind.ShouldBe(PrometheusProviderKind.External);
        queryClient.PrepareCalls.ShouldBe(1);
    }

    [Fact]
    public async Task InitializeAsync_with_available_metrics_server_activates_metrics_server_backend()
    {
        using var api = CreateMetricsServerApi();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.KubernetesMetricsServer,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("metrics-server-cluster", service, settings, CreateFakeClient(api));

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeTrue();
        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.KubernetesMetricsServer);
    }

    [Fact]
    public async Task InitializeAsync_with_unavailable_metrics_server_keeps_metrics_disabled()
    {
        using var api = new FakeKubernetesHttpApi();
        api.SetPermission("pods", "list", false);
        api.SetPermission("nodes", "list", false);
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.KubernetesMetricsServer,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("metrics-server-cluster", service, settings, CreateFakeClient(api));

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeFalse();
        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.None);
    }

    [Fact]
    public async Task InitializeAsync_auto_falls_back_to_metrics_server_when_prometheus_is_unavailable()
    {
        using var api = CreateMetricsServerApi();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Auto,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("auto-metrics-cluster", service, settings, CreateFakeClient(api));

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeTrue();
        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.KubernetesMetricsServer);
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

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
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

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
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

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
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

    [Fact]
    public async Task RequestMetricsAsync_honors_caller_cancellation()
    {
        var queryClient = new FakePrometheusQueryClient { WaitForRelease = true };
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);

        using var cancellation = new CancellationTokenSource();
        var request = service.RequestMetricsAsync(CreateRequest(), cancellation.Token);
        await queryClient.QueryStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => request);
    }

    [Fact]
    public async Task StopAsync_cancels_inflight_requests_and_clears_metrics_state()
    {
        var queryClient = new FakePrometheusQueryClient { WaitForRelease = true };
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);
        var request = service.RequestMetricsAsync(CreateRequest());
        await queryClient.QueryStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        var resetCallsBeforeStop = queryClient.ResetCalls;

        await service.StopAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => request);
        service.IsMetricsAvailable.ShouldBeFalse();
        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.None);
        queryClient.ResetCalls.ShouldBe(resetCallsBeforeStop + 1);
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

    private static Cluster CreateCluster(string name, MetricsService metricsService, IClusterSettingsStore settings, IKubernetes? client = null)
    {
        return new Cluster(
            NullLogger<Cluster>.Instance,
            NullLoggerFactory.Instance,
            new ClusterModelCatalog(new KubernetesModelCatalog()),
            settings,
            new ServiceCollection().BuildServiceProvider(),
            new ImmediateThreadDispatcher(),
            metricsService)
        {
            Name = name,
            Client = client ?? new k8s.Kubernetes(new KubernetesClientConfiguration
            {
                Host = "http://localhost",
            }),
        };
    }

    private static k8s.Kubernetes CreateFakeClient(FakeKubernetesHttpApi api)
    {
        return new k8s.Kubernetes(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            new MetricsDiscoveryHandler(api));
    }

    private static FakeKubernetesHttpApi CreateMetricsServerApi()
    {
        return new FakeKubernetesHttpApi();
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
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int PrepareCalls { get; private set; }

        public int QueryCalls { get; private set; }

        public int ResetCalls { get; private set; }

        public TaskCompletionSource QueryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool WaitForRelease { get; init; }

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

            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return _responses.Count > 0 ? _responses.Dequeue() : null;
        }

        public Task ResetAsync()
        {
            ResetCalls++;
            _release.TrySetResult();
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

    private sealed class MetricsDiscoveryHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.TrimEnd('/') == "/apis")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(
                        """{"apiVersion":"v1","kind":"APIGroupList","groups":[{"name":"metrics.k8s.io","versions":[{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}],"preferredVersion":{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}}]}""",
                        Encoding.UTF8,
                        "application/json"),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
