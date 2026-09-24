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
    public async Task PrepareAsync_with_same_service_proxy_endpoint_logs_transport_once()
    {
        var logger = new TestLogger<PrometheusQueryClient>();
        var client = new PrometheusQueryClient(logger);
        var fixture = CreateCluster("microk8s");
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
    public async Task PrepareAsync_with_changed_endpoint_logs_transport_again()
    {
        var logger = new TestLogger<PrometheusQueryClient>();
        var client = new PrometheusQueryClient(logger);
        var fixture = CreateCluster("microk8s");
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
        var client = new PrometheusQueryClient(
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

        var response = JsonSerializer.Deserialize<PrometheusClientQueryRangeResponse>(json);

        response.ShouldNotBeNull();
        var value = response.Data.Result[0].Values[0];
        value.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1735689600250));
        value.Value.ShouldBe(1.5d);
    }

    private static (Cluster Cluster, MetricsService Service) CreateCluster(string name)
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
            Client = new k8s.Kubernetes(new KubernetesClientConfiguration
            {
                Host = "http://localhost",
            }),
        };
        return (cluster, metricsService);
    }

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

    private sealed class RecordingHandler(Action<HttpRequestMessage> record) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            record(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"success\",\"data\":{\"resultType\":\"matrix\",\"result\":[]}}"),
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
