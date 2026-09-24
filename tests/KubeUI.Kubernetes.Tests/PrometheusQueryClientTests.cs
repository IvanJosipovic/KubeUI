using System.Text.Json;
using System.Net;
using Azure.Core;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PrometheusQueryClientTests
{
    [Fact]
    public async Task PrepareAsync_lists_service_when_seeded_cache_does_not_contain_target()
    {
        List<string> observedUris = [];
        using var kubernetesHandler = new RecordingHandler(
            request => observedUris.Add(request.RequestUri?.ToString() ?? string.Empty),
            """
            {"apiVersion":"v1","kind":"ServiceList","items":[{"apiVersion":"v1","kind":"Service","metadata":{"name":"prometheus-operated","namespace":"monitoring"},"spec":{"ports":[{"port":9090}]}}]}
            """);
        var fixture = CreateCluster("microk8s", kubernetesHandler);
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        var client = new PrometheusQueryClient(NullLogger<PrometheusQueryClient>.Instance);
        var endpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.Operator,
            "Prometheus Operator",
            false,
            "monitoring",
            "prometheus-operated",
            9090,
            null,
            false,
            string.Empty,
            null);

        await client.PrepareAsync(cluster, endpoint);

        observedUris.Count.ShouldBe(1);
        new Uri(observedUris[0]).AbsolutePath.ShouldBe("/api/v1/namespaces/monitoring/services");
        var service = cluster.GetResource<k8s.Models.V1Service>("monitoring", "prometheus-operated");
        service.ShouldNotBeNull();
        service.Spec.Ports[0].Port.ShouldBe(9090);
        await client.ResetAsync();
    }

    [Fact]
    public async Task QueryRangeAsync_reuses_existing_cluster_service_port_forward()
    {
        List<string> observedUris = [];
        List<string?> observedTokens = [];
        int kubernetesApiRequests = 0;
        using var kubernetesHandler = new RecordingHandler(_ => kubernetesApiRequests++, ServiceListResponseBody);
        using var queryHandler = new RecordingHandler(request =>
        {
            observedUris.Add(request.RequestUri?.ToString() ?? string.Empty);
            observedTokens.Add(request.Headers.Authorization?.Parameter);
        });
        var fixture = CreateCluster("microk8s", kubernetesHandler);
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        var existingPortForwarder = cluster.AddServicePortForward("monitoring", "prometheus-operated", 9090);
        var client = new PrometheusQueryClient(
            NullLogger<PrometheusQueryClient>.Instance,
            null,
            () => queryHandler);
        var endpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.Operator,
            "Prometheus Operator",
            false,
            "monitoring",
            "prometheus-operated",
            9090,
            null,
            false,
            string.Empty,
            "prometheus-token");

        var response = await client.QueryRangeAsync(
            cluster,
            endpoint,
            "up{job=\"kube api\"}",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch.AddMinutes(1),
            60);

        response.ShouldNotBeNull();
        response.Status.ShouldBe("success");
        observedUris.Count.ShouldBe(1);
        var observedUri = new Uri(observedUris[0]);
        observedUri.Host.ShouldBe("prometheus-operated.monitoring.svc");
        observedUri.Port.ShouldBe(existingPortForwarder.LocalPort);
        observedUri.AbsolutePath.ShouldBe("/api/v1/query_range");
        Uri.UnescapeDataString(observedUri.Query).ShouldContain("query=up{job=\"kube api\"}");
        observedTokens.ShouldBe(["prometheus-token"]);
        cluster.PortForwarders.Count.ShouldBe(1);
        kubernetesApiRequests.ShouldBe(1);
        kubernetesHandler.RequestUris.Single().ShouldStartWith("/api/v1/namespaces/monitoring/services?");
        await client.ResetAsync();
    }

    [Fact]
    public async Task PrepareAsync_with_same_service_proxy_endpoint_logs_transport_once()
    {
        var logger = new TestLogger<PrometheusQueryClient>();
        using var client = new PrometheusQueryClient(logger);
        using var kubernetesHandler = new RecordingHandler(_ => { }, ServiceListResponseBody);
        var fixture = CreateCluster("microk8s", kubernetesHandler);
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        var endpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.Operator,
            "Prometheus Operator",
            false,
            "monitoring",
            "prometheus-operated",
            9090,
            null,
            false,
            string.Empty,
            null);

        await client.PrepareAsync(cluster, endpoint);
        await client.PrepareAsync(cluster, endpoint);

        logger.Messages.Count(message => message.Contains("Prepared Prometheus transport", StringComparison.Ordinal))
            .ShouldBe(1);
    }

    [Fact]
    public async Task PrepareAsync_concurrent_same_endpoint_creates_one_http_client()
    {
        using var kubernetesHandler = new RecordingHandler(_ => { }, ServiceListResponseBody);
        var fixture = CreateCluster("microk8s", kubernetesHandler);
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        int httpHandlerCreations = 0;
        using var client = new PrometheusQueryClient(
            NullLogger<PrometheusQueryClient>.Instance,
            null,
            () =>
            {
                Interlocked.Increment(ref httpHandlerCreations);
                return new RecordingHandler(_ => { });
            });
        var endpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.Operator,
            "Prometheus Operator",
            false,
            "monitoring",
            "prometheus-operated",
            9090,
            null,
            false,
            string.Empty,
            null);

        await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => client.PrepareAsync(cluster, endpoint)));

        httpHandlerCreations.ShouldBe(1);
        await client.ResetAsync();
    }

    [Fact]
    public async Task PrepareAsync_with_changed_endpoint_logs_transport_again()
    {
        var logger = new TestLogger<PrometheusQueryClient>();
        using var client = new PrometheusQueryClient(logger);
        using var kubernetesHandler = new RecordingHandler(_ => { }, ServiceListResponseBody);
        var fixture = CreateCluster("microk8s", kubernetesHandler);
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        var firstEndpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.Operator,
            "Prometheus Operator",
            false,
            "monitoring",
            "prometheus-operated",
            9090,
            null,
            false,
            string.Empty,
            null);
        var secondEndpoint = firstEndpoint with { ServiceName = "prometheus-secondary" };

        await client.PrepareAsync(cluster, firstEndpoint);
        await client.PrepareAsync(cluster, secondEndpoint);

        logger.Messages.Count(message => message.Contains("Prepared Prometheus transport", StringComparison.Ordinal))
            .ShouldBe(2);
    }

    [Fact]
    public async Task QueryRangeAsync_uses_a_fresh_azure_token_for_each_query()
    {
        string[] observedTokens = [];
        using var handler = new RecordingHandler(request => observedTokens = [.. observedTokens, request.Headers.Authorization?.Parameter ?? string.Empty]);
        var azureService = new FakeAzureMonitorWorkspaceService();
        using var client = new PrometheusQueryClient(
            NullLogger<PrometheusQueryClient>.Instance,
            azureService,
            () => handler);
        var fixture = CreateCluster("azure-prometheus");
        await using var cluster = fixture.Cluster;
        using var metricsService = fixture.Service;
        var endpoint = new ResolvedPrometheusEndpoint(
            PrometheusProviderKind.AzureMonitor,
            "Azure Managed Prometheus",
            true,
            null,
            null,
            null,
            "https://workspace.eastus.prometheus.monitor.azure.com",
            true,
            string.Empty,
            null,
            UseAzureMonitorAuthentication: true);

        await client.QueryRangeAsync(cluster, endpoint, "up", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddMinutes(1), 60);
        await client.QueryRangeAsync(cluster, endpoint, "up", DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch.AddMinutes(1), 60);

        observedTokens.ShouldBe(["azure-token-1", "azure-token-2"]);
        azureService.TokenRequests.ShouldBe(2);
        await client.ResetAsync();
    }

    [Fact]
    public void QueryRangeResponse_deserializes_fractional_prometheus_timestamps()
    {
        const string json =
            """
            {
              "status": "success",
              "data": {
                "resultType": "matrix",
                "result": [
                  {
                    "metric": { "pod": "pod-a" },
                    "values": [
                      [ 1735689600.25, "1.5" ]
                    ]
                  }
                ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(
            json,
            CustomSourceGenerationContext.Default.PrometheusClientQueryRangeResponse);

        response.ShouldNotBeNull();
        var value = response.Data.Result[0].Values[0];
        value.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1735689600250));
        value.Value.ShouldBe(1.5d);
    }

    private static (Cluster Cluster, MetricsService Service) CreateCluster(string name, DelegatingHandler? handler = null)
    {
        var metricsService = new MetricsService(
            NullLogger<MetricsService>.Instance,
            new TestClusterSettingsStore(),
            [
                new OperatorPrometheusProvider(),
                new OpenShiftPrometheusProvider(),
                new ManualPrometheusProvider(),
                new ExternalPrometheusProvider(),
                new AzureMonitorPrometheusProvider(),
            ],
            new NoopPrometheusQueryClient());
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            NullLoggerFactory.Instance,
            new ClusterModelCatalog(new KubernetesModelCatalog()),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider(),
            new ImmediateThreadDispatcher(),
            metricsService)
        {
            Name = name,
            Client = handler is null
                ? new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" })
                : new k8s.Kubernetes(new KubernetesClientConfiguration { Host = "http://localhost" }, handler),
        };
        return (cluster, metricsService);
    }

    private const string ServiceListResponseBody =
        "{\"apiVersion\":\"v1\",\"kind\":\"ServiceList\",\"items\":["
        + "{\"apiVersion\":\"v1\",\"kind\":\"Service\",\"metadata\":{\"name\":\"prometheus-operated\",\"namespace\":\"monitoring\"},\"spec\":{\"ports\":[{\"port\":9090}]}},"
        + "{\"apiVersion\":\"v1\",\"kind\":\"Service\",\"metadata\":{\"name\":\"prometheus-secondary\",\"namespace\":\"monitoring\"},\"spec\":{\"ports\":[{\"port\":9090}]}}]}";

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class NoopPrometheusQueryClient : IPrometheusQueryClient
    {
        public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
            Cluster cluster,
            ResolvedPrometheusEndpoint endpoint,
            string query,
            DateTimeOffset start,
            DateTimeOffset end,
            int stepSeconds,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PrometheusClientQueryRangeResponse?>(null);
        }

        public Task ResetAsync() => Task.CompletedTask;
    }

    private sealed class FakeAzureMonitorWorkspaceService : IAzureMonitorWorkspaceService
    {
        public int TokenRequests { get; private set; }

        public Task<AzureAuthenticationStatus> GetAuthenticationStatusAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new AzureAuthenticationStatus { AzureCliSignedIn = true });

        public Task<IReadOnlyList<AzureMonitorSubscriptionInfo>> GetSubscriptionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AzureMonitorSubscriptionInfo>>([]);

        public Task<IReadOnlyList<AzureMonitorWorkspaceInfo>> GetWorkspacesAsync(string subscriptionId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AzureMonitorWorkspaceInfo>>([]);

        public ValueTask<AccessToken> GetPrometheusAccessTokenAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new AccessToken($"azure-token-{++TokenRequests}", DateTimeOffset.UtcNow.AddMinutes(5)));
    }

    private sealed class RecordingHandler : DelegatingHandler
    {
        public RecordingHandler(Action<HttpRequestMessage> record, string responseBody = "{\"status\":\"success\",\"data\":{\"resultType\":\"matrix\",\"result\":[]}}")
        {
            _record = record;
            _responseBody = responseBody;
        }

        private readonly Action<HttpRequestMessage> _record;
        private readonly string _responseBody;
        public List<string> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri?.PathAndQuery ?? string.Empty);
            _record(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseBody),
                RequestMessage = request,
            });
        }
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

        public ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster)
        {
            return new ClusterMetricsSettings();
        }

        public void Persist()
        {
        }
    }
}
