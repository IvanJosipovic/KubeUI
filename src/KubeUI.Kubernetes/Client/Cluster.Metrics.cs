using System.Collections.Immutable;
using k8s.Models;

namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private readonly ObservableCollection<PodMetrics> _emptyPodMetrics = [];
    private readonly ObservableCollection<NodeMetrics> _emptyNodeMetrics = [];

    public ObservableCollection<PodMetrics> PodMetrics => _metricsService?.PodMetrics ?? _emptyPodMetrics;

    public ObservableCollection<NodeMetrics> NodeMetrics => _metricsService?.NodeMetrics ?? _emptyNodeMetrics;

    public bool IsMetricsAvailable => _metricsService?.IsMetricsAvailable ?? false;

    public ActiveMetricsBackend ActiveMetricsBackend => _metricsService?.ActiveMetricsBackend ?? ActiveMetricsBackend.None;

    public ImmutableArray<PodMetrics> GetPodMetricsSnapshot(string? namespaceName, string podName)
        => _metricsService?.GetPodMetricsSnapshot(namespaceName, podName) ?? ImmutableArray<PodMetrics>.Empty;

    public ImmutableArray<NodeMetrics> GetNodeMetricsSnapshot(string nodeName)
        => _metricsService?.GetNodeMetricsSnapshot(nodeName) ?? ImmutableArray<NodeMetrics>.Empty;

    public ImmutableArray<PodMetrics> GetAllPodMetricsSnapshot()
        => _metricsService?.GetAllPodMetricsSnapshot() ?? ImmutableArray<PodMetrics>.Empty;

    public ImmutableArray<NodeMetrics> GetAllNodeMetricsSnapshot()
        => _metricsService?.GetAllNodeMetricsSnapshot() ?? ImmutableArray<NodeMetrics>.Empty;

    public Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default)
        => _metricsService?.RequestMetricsAsync(request, cancellationToken)
            ?? Task.FromException<MetricResultSet>(new InvalidOperationException("Metrics service is not registered."));
}
