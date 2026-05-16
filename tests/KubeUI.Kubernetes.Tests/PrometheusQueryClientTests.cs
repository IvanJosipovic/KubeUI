using System.Text.Json;
using k8s;
using KubernetesCRDModelGen;
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
        var cluster = CreateCluster("microk8s");
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
        var cluster = CreateCluster("microk8s");
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

        PrometheusClientQueryRangeResponse? response = JsonSerializer.Deserialize<PrometheusClientQueryRangeResponse>(json);

        response.ShouldNotBeNull();
        var value = response.Data.Result[0].Values[0];
        value.Timestamp.ShouldBe(DateTimeOffset.FromUnixTimeMilliseconds(1735689600250));
        value.Value.ShouldBe(1.5d);
    }

    private static Cluster CreateCluster(string name)
    {
        return new Cluster(
            NullLogger<Cluster>.Instance,
            NullLoggerFactory.Instance,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider(),
            new MetricsService(
                NullLogger<MetricsService>.Instance,
                new TestClusterSettingsStore(),
                [
                    new OperatorPrometheusProvider(),
                    new OpenShiftPrometheusProvider(),
                    new ManualPrometheusProvider(),
                    new ExternalPrometheusProvider(),
                ],
                new NoopPrometheusQueryClient()))
        {
            Name = name,
            Client = new k8s.Kubernetes(new KubernetesClientConfiguration
            {
                Host = "http://localhost",
            }),
        };
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
