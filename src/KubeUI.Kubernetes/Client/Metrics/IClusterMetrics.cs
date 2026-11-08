using System.Collections.Immutable;
using System.ComponentModel;
using k8s.Models;

namespace KubeUI.Kubernetes;

/// <summary>Exposes metrics data and queries supported by a cluster runtime.</summary>
/// <remarks>
/// Kubernetes Metrics Server supplies the Pod and Node sample collections and snapshots.
/// Prometheus supplies historical query results. <see cref="ActiveMetricsBackend"/> reports which
/// backend is currently active.
/// </remarks>
public interface IClusterMetrics : INotifyPropertyChanged
{
    /// <summary>Gets live Pod samples collected from Kubernetes Metrics Server.</summary>
    ObservableCollection<PodMetrics> KubernetesPodMetrics { get; }

    /// <summary>Gets live Node samples collected from Kubernetes Metrics Server.</summary>
    ObservableCollection<NodeMetrics> KubernetesNodeMetrics { get; }

    /// <summary>Gets whether the active Kubernetes Metrics Server or Prometheus backend is available.</summary>
    bool IsMetricsAvailable { get; }

    /// <summary>Gets the active backend; provider kind is populated only for Prometheus.</summary>
    ActiveMetricsBackend ActiveMetricsBackend { get; }

    /// <summary>Gets retained Kubernetes Metrics Server samples for one namespaced Pod.</summary>
    ImmutableArray<PodMetrics> GetKubernetesPodMetricsSnapshot(string? namespaceName, string podName);

    /// <summary>Gets retained Kubernetes Metrics Server samples for one Node.</summary>
    ImmutableArray<NodeMetrics> GetKubernetesNodeMetricsSnapshot(string nodeName);

    /// <summary>Gets all retained Kubernetes Metrics Server Pod samples.</summary>
    ImmutableArray<PodMetrics> GetAllKubernetesPodMetricsSnapshot();

    /// <summary>Gets all retained Kubernetes Metrics Server Node samples.</summary>
    ImmutableArray<NodeMetrics> GetAllKubernetesNodeMetricsSnapshot();

    /// <summary>Executes Prometheus queries; reports backend-unavailable when Prometheus is inactive.</summary>
    Task<MetricResultSet> QueryPrometheusAsync(MetricRequest request, CancellationToken cancellationToken = default);
}
