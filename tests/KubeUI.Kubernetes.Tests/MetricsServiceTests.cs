using k8s;
using k8s.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
            PrometheusProviderKind.AzureMonitor,
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
    public async Task InitializeAsync_with_azure_monitor_workspace_activates_authenticated_prometheus_backend()
    {
        var queryClient = new FakePrometheusQueryClient();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.AzureMonitor,
            AzureMonitorWorkspaceId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Monitor/accounts/amw",
            AzureMonitorQueryEndpoint = "https://amw.eastus.prometheus.monitor.azure.com",
        });

        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("azure-prom-cluster", service, settings);

        await service.InitializeAsync(cluster);

        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.Prometheus(PrometheusProviderKind.AzureMonitor));
        queryClient.PrepareCalls.ShouldBe(1);
        queryClient.PreparedEndpoint?.DirectUrl.ShouldBe("https://amw.eastus.prometheus.monitor.azure.com");
        queryClient.PreparedEndpoint?.UseAzureMonitorAuthentication.ShouldBeTrue();
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
    public async Task InitializeAsync_explicit_prometheus_does_not_fall_back_to_metrics_server()
    {
        using var api = CreateMetricsServerApi();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("explicit-prom-cluster", service, settings, CreateFakeClient(api));

        await service.InitializeAsync(cluster);

        service.IsMetricsAvailable.ShouldBeFalse();
        service.ActiveMetricsBackend.ShouldBe(ActiveMetricsBackend.None);
    }

    [Fact]
    public async Task InitializeAsync_with_available_metrics_server_refreshes_pod_and_node_metrics()
    {
        using var api = CreateMetricsServerApi();
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.KubernetesMetricsServer,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("metrics-server-data-cluster", service, settings, CreateFakeClient(api, includeMetricsData: true));

        await service.InitializeAsync(cluster);

        service.PodMetrics.ShouldHaveSingleItem();
        var podMetric = service.PodMetrics[0];
        podMetric.Name().ShouldBe("pod-1");
        podMetric.Namespace().ShouldBe("default");
        podMetric.Containers.ShouldHaveSingleItem();
        podMetric.Containers[0].Usage["cpu"].ToDecimal().ShouldBe(0.1m);
        podMetric.Containers[0].Usage["memory"].ToInt64().ShouldBe(128L * 1024 * 1024);

        service.NodeMetrics.ShouldHaveSingleItem();
        var nodeMetric = service.NodeMetrics[0];
        nodeMetric.Name().ShouldBe("node-1");
        nodeMetric.Usage["cpu"].ToDecimal().ShouldBe(0.25m);
        nodeMetric.Usage["memory"].ToInt64().ShouldBe(512L * 1024 * 1024);
    }

    [Fact]
    public async Task SyncKubernetesMetricsAsync_appends_recent_samples_and_removes_samples_older_than_one_hour()
    {
        using var api = CreateMetricsServerApi();
        var client = CreateFakeClient(api, out var discoveryHandler);
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.KubernetesMetricsServer,
        });
        using var service = CreateMetricsService(settings, new FakePrometheusQueryClient());
        await using var cluster = CreateCluster("metrics-server-history-cluster", service, settings, client);
        var now = DateTime.UtcNow;
        var older = now.AddHours(-2);
        var retained = now.AddMinutes(-30);
        var latest = now.AddMinutes(-1);

        await service.InitializeAsync(cluster);

        service.PodMetrics.Add(CreatePodMetrics(older, "10m"));
        service.PodMetrics.Add(CreatePodMetrics(retained, "50m"));
        service.PodMetrics.Add(CreatePodMetrics(retained, "55m"));
        service.NodeMetrics.Add(CreateNodeMetrics(older, "100m"));
        service.NodeMetrics.Add(CreateNodeMetrics(retained, "200m"));
        service.NodeMetrics.Add(CreateNodeMetrics(retained, "220m"));
        discoveryHandler.PodMetricsResponse = CreatePodMetricsListJson(
            (older, "10m"),
            (latest, "100m"),
            (latest, "125m"),
            (null, "1m"));
        discoveryHandler.NodeMetricsResponse = CreateNodeMetricsListJson(
            (older, "100m"),
            (latest, "250m"),
            (latest, "275m"),
            (null, "50m"));

        await service.SyncKubernetesMetricsAsync(cluster, TestContext.Current.CancellationToken);
        await service.SyncKubernetesMetricsAsync(cluster, TestContext.Current.CancellationToken);

        service.PodMetrics.Select(static metric => metric.Timestamp).ShouldBe([retained, latest]);
        service.NodeMetrics.Select(static metric => metric.Timestamp).ShouldBe([retained, latest]);
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
    public async Task InitializeAsync_auto_falls_back_to_available_metrics_server_when_prometheus_is_unavailable()
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
    public async Task RequestMetricsAsync_gap_filling_uses_requested_step_seconds()
    {
        var queryClient = new FakePrometheusQueryClient();
        queryClient.EnqueueResponse(new PrometheusClientQueryRangeResponse
        {
            Status = "success",
            Data = new PrometheusClientQueryRangeResponse.DataObject
            {
                ResultType = "matrix",
                Result =
                [
                    new PrometheusClientQueryRangeResponse.ResultObject
                    {
                        Metric = new Dictionary<string, string> { ["pod"] = "pod-1" },
                        Values =
                        [
                            (DateTimeOffset.FromUnixTimeSeconds(0), 1d),
                            (DateTimeOffset.FromUnixTimeSeconds(600), 2d),
                        ],
                    },
                ],
            },
        });
        var settings = new TestClusterSettingsStore(new ClusterMetricsSettings
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
            PrometheusProviderKind = PrometheusProviderKind.External,
            PrometheusDirectUrl = "http://prometheus.example",
        });
        using var service = CreateMetricsService(settings, queryClient);
        await using var cluster = CreateCluster("prom-cluster", service, settings);
        await service.InitializeAsync(cluster);
        var request = CreateRequest(stepSeconds: 300, frames: 3);

        var result = await service.RequestMetricsAsync(request);

        result.Metrics["cpuUsage"].Single().Points.Select(static point => point.Timestamp)
            .ShouldBe([DateTimeOffset.FromUnixTimeSeconds(0), DateTimeOffset.FromUnixTimeSeconds(300), DateTimeOffset.FromUnixTimeSeconds(600)]);
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
    public async Task RequestMetricsAsync_allows_new_request_after_transport_failure()
    {
        var queryClient = new FakePrometheusQueryClient
        {
            ExceptionToThrow = new InvalidOperationException("boom"),
            FailuresRemaining = 1,
        };
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

        var first = await service.RequestMetricsAsync(request);
        var second = await service.RequestMetricsAsync(request);

        first.IsEmpty.ShouldBeTrue();
        first.HadRequestFailures.ShouldBeTrue();
        second.IsEmpty.ShouldBeFalse();
        second.HadRequestFailures.ShouldBeFalse();
        queryClient.QueryCalls.ShouldBe(2);
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
                new AzureMonitorPrometheusProvider(),
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

    private static k8s.Kubernetes CreateFakeClient(FakeKubernetesHttpApi api, bool includeMetricsData = false)
    {
        return new k8s.Kubernetes(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            new MetricsDiscoveryHandler(api, includeMetricsData));
    }

    private static k8s.Kubernetes CreateFakeClient(FakeKubernetesHttpApi api, out MetricsDiscoveryHandler handler)
    {
        handler = new MetricsDiscoveryHandler(api, includeMetricsData: true)
        {
            PodMetricsResponse = """{"apiVersion":"metrics.k8s.io/v1beta1","kind":"PodMetricsList","items":[]}""",
            NodeMetricsResponse = """{"apiVersion":"metrics.k8s.io/v1beta1","kind":"NodeMetricsList","items":[]}""",
        };
        return new k8s.Kubernetes(
            new KubernetesClientConfiguration { Host = "http://fake-kubernetes" },
            handler);
    }

    private static PodMetrics CreatePodMetrics(DateTime timestamp, string cpu)
    {
        return KubernetesJson.Deserialize<PodMetrics>(CreatePodMetricsJson(timestamp, cpu));
    }

    private static NodeMetrics CreateNodeMetrics(DateTime timestamp, string cpu)
    {
        return KubernetesJson.Deserialize<NodeMetrics>(CreateNodeMetricsJson(timestamp, cpu));
    }

    private static string CreatePodMetricsJson(DateTime? timestamp, string cpu)
    {
        return JsonSerializer.Serialize(new
        {
            metadata = new { name = "pod-1", @namespace = "default" },
            timestamp,
            window = "30s",
            containers = new[]
            {
                new
                {
                    name = "app",
                    usage = new Dictionary<string, string> { ["cpu"] = cpu, ["memory"] = "128Mi" },
                },
            },
        });
    }

    private static string CreateNodeMetricsJson(DateTime? timestamp, string cpu)
    {
        return JsonSerializer.Serialize(new
        {
            metadata = new { name = "node-1" },
            timestamp,
            window = "30s",
            usage = new Dictionary<string, string> { ["cpu"] = cpu, ["memory"] = "512Mi" },
        });
    }

    private static string CreatePodMetricsListJson(DateTime? timestamp, string cpu)
    {
        return CreatePodMetricsListJson((timestamp, cpu));
    }

    private static string CreatePodMetricsListJson(params (DateTime? Timestamp, string Cpu)[] samples)
    {
        return JsonSerializer.Serialize(new
        {
            apiVersion = "metrics.k8s.io/v1beta1",
            kind = "PodMetricsList",
            items = samples.Select(static sample => JsonSerializer.Deserialize<JsonElement>(CreatePodMetricsJson(sample.Timestamp, sample.Cpu))).ToArray(),
        });
    }

    private static string CreateNodeMetricsListJson(DateTime? timestamp, string cpu)
    {
        return CreateNodeMetricsListJson((timestamp, cpu));
    }

    private static string CreateNodeMetricsListJson(params (DateTime? Timestamp, string Cpu)[] samples)
    {
        return JsonSerializer.Serialize(new
        {
            apiVersion = "metrics.k8s.io/v1beta1",
            kind = "NodeMetricsList",
            items = samples.Select(static sample => JsonSerializer.Deserialize<JsonElement>(CreateNodeMetricsJson(sample.Timestamp, sample.Cpu))).ToArray(),
        });
    }

    private static FakeKubernetesHttpApi CreateMetricsServerApi()
    {
        return new FakeKubernetesHttpApi();
    }

    private static MetricRequest CreateRequest(int stepSeconds = 60, int frames = 5)
    {
        return new MetricRequest
        {
            Category = MetricCategory.Pods,
            RangeSeconds = 300,
            StepSeconds = stepSeconds,
            Frames = frames,
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

        public ResolvedPrometheusEndpoint? PreparedEndpoint { get; private set; }

        public TaskCompletionSource QueryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool WaitForRelease { get; init; }

        public Exception? ExceptionToThrow { get; set; }

        public int FailuresRemaining { get; set; } = -1;

        public void EnqueueResponse(PrometheusClientQueryRangeResponse response)
        {
            _responses.Enqueue(response);
        }

        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            PrepareCalls++;
            PreparedEndpoint = endpoint;
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

            if (ExceptionToThrow != null && (FailuresRemaining < 0 || FailuresRemaining-- > 0))
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

    private sealed class MetricsDiscoveryHandler(HttpMessageHandler innerHandler, bool includeMetricsData = false) : DelegatingHandler(innerHandler)
    {
        public string PodMetricsResponse { get; set; } = CreatePodMetricsListJson(DateTime.UtcNow, "100m");

        public string NodeMetricsResponse { get; set; } = CreateNodeMetricsListJson(DateTime.UtcNow, "250m");

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath.TrimEnd('/');
            if (path == "/apis")
            {
                return Task.FromResult(JsonResponse(
                    request,
                    """{"apiVersion":"v1","kind":"APIGroupList","groups":[{"name":"metrics.k8s.io","versions":[{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}],"preferredVersion":{"groupVersion":"metrics.k8s.io/v1beta1","version":"v1beta1"}}]}"""));
            }

            if (includeMetricsData && path == "/apis/metrics.k8s.io/v1beta1/nodes")
            {
                return Task.FromResult(JsonResponse(
                    request,
                    NodeMetricsResponse));
            }

            if (includeMetricsData && path == "/apis/metrics.k8s.io/v1beta1/pods")
            {
                return Task.FromResult(JsonResponse(
                    request,
                    PodMetricsResponse));
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static HttpResponseMessage JsonResponse(HttpRequestMessage request, string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }
    }
}
