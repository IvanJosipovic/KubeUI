namespace KubeUI.Kubernetes;

/// <summary>Runs the configured Kubernetes Metrics Server or Prometheus metrics backend for one cluster.</summary>
public interface IMetricsService : IClusterMetrics
{
    /// <summary>Starts the configured backend, selecting Kubernetes Metrics Server or Prometheus.</summary>
    Task InitializeAsync(Cluster cluster);

    /// <summary>Stops the active metrics backend and clears retained samples.</summary>
    Task StopAsync();

    /// <summary>Gets the registered Prometheus providers.</summary>
    Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync();
}
