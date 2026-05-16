using k8s;

namespace KubeUI.Kubernetes;

public interface IPrometheusProvider
{
    PrometheusProviderKind Kind { get; }

    string Name { get; }

    bool IsConfigurable { get; }

    string BuildQuery(MetricCategory category, string queryName, IReadOnlyDictionary<string, string> options);

    Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default);
}
