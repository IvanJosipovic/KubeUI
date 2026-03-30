using k8s.Models;

namespace KubeUI.Kubernetes;

public interface IMetricsService
{
    ObservableCollection<PodMetrics> PodMetrics { get; }

    ObservableCollection<NodeMetrics> NodeMetrics { get; }

    bool IsMetricsAvailable { get; }

    ActiveMetricsBackend ActiveMetricsBackend { get; }

    Task InitializeAsync(Cluster cluster);

    Task StopAsync();

    Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync();
}
