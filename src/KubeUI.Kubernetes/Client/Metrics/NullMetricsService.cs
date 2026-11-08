using System.Collections.Immutable;
using k8s.Models;

namespace KubeUI.Kubernetes;

/// <summary>Supplies empty metrics behavior when metrics services are not registered.</summary>
internal sealed class NullMetricsService : ObservableObject, IMetricsService
{
    public ObservableCollection<PodMetrics> KubernetesPodMetrics { get; } = [];

    public ObservableCollection<NodeMetrics> KubernetesNodeMetrics { get; } = [];

    public bool IsMetricsAvailable => false;

    public ActiveMetricsBackend ActiveMetricsBackend => ActiveMetricsBackend.None;

    public ImmutableArray<PodMetrics> GetKubernetesPodMetricsSnapshot(string? namespaceName, string podName) =>
        ImmutableArray<PodMetrics>.Empty;

    public ImmutableArray<NodeMetrics> GetKubernetesNodeMetricsSnapshot(string nodeName) =>
        ImmutableArray<NodeMetrics>.Empty;

    public ImmutableArray<PodMetrics> GetAllKubernetesPodMetricsSnapshot() =>
        ImmutableArray<PodMetrics>.Empty;

    public ImmutableArray<NodeMetrics> GetAllKubernetesNodeMetricsSnapshot() =>
        ImmutableArray<NodeMetrics>.Empty;

    public Task<MetricResultSet> QueryPrometheusAsync(MetricRequest request, CancellationToken cancellationToken = default) =>
        Task.FromException<MetricResultSet>(new InvalidOperationException("Metrics service is not registered."));

    public Task InitializeAsync(Cluster cluster) => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    public Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync() =>
        Task.FromResult<IReadOnlyList<MetricProviderInfo>>([]);
}
