using System.Collections.Immutable;
using k8s.Models;

namespace KubeUI.Kubernetes;

public interface IMetricsService
{
    ObservableCollection<PodMetrics> PodMetrics { get; }

    ObservableCollection<NodeMetrics> NodeMetrics { get; }

    /// <summary>Gets an immutable snapshot of retained metrics for one namespaced Pod.</summary>
    ImmutableArray<PodMetrics> GetPodMetricsSnapshot(string? namespaceName, string podName);

    /// <summary>Gets an immutable snapshot of retained metrics for one Node.</summary>
    ImmutableArray<NodeMetrics> GetNodeMetricsSnapshot(string nodeName);

    bool IsMetricsAvailable { get; }

    ActiveMetricsBackend ActiveMetricsBackend { get; }

    Task InitializeAsync(Cluster cluster);

    Task StopAsync();

    Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync();
}
