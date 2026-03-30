namespace KubeUI.Kubernetes;

public enum MetricCategory
{
    Cluster,
    Nodes,
    Pods,
    WorkloadPods,
    Pvc,
    Ingress,
    Namespace,
}

public sealed record ActiveMetricsBackend(MetricsServiceType Type, PrometheusProviderKind? PrometheusProviderKind = null)
{
    public static ActiveMetricsBackend None { get; } = new(MetricsServiceType.None);

    public static ActiveMetricsBackend KubernetesMetricsServer { get; } = new(MetricsServiceType.KubernetesMetricsServer);

    public static ActiveMetricsBackend Prometheus(PrometheusProviderKind providerKind) => new(MetricsServiceType.Prometheus, providerKind);
}

public sealed record MetricProviderInfo(PrometheusProviderKind Kind, string Name, bool IsConfigurable);

public sealed record MetricPoint(DateTimeOffset Timestamp, double Value);

public sealed class MetricSeries
{
    public required string Name { get; init; }

    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    public IReadOnlyList<MetricPoint> Points { get; init; } = [];
}

public sealed class MetricResultSet
{
    public IDictionary<string, IReadOnlyList<MetricSeries>> Metrics { get; init; } = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal);

    public bool IsEmpty => Metrics.Values.All(static series => series.Count == 0 || series.All(static item => item.Points.Count == 0));

    public static MetricResultSet Empty { get; } = new();
}

public sealed class MetricQueryDefinition
{
    public required string Name { get; init; }

    public IReadOnlyDictionary<string, string> Options { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed class MetricRequest
{
    public required MetricCategory Category { get; init; }

    public required IReadOnlyList<MetricQueryDefinition> Queries { get; init; }

    public DateTimeOffset? Start { get; init; }

    public DateTimeOffset? End { get; init; }

    public int? StepSeconds { get; init; }

    public int? RangeSeconds { get; init; }

    public int Frames { get; init; } = 60;
}

public sealed record ResolvedPrometheusEndpoint(
    PrometheusProviderKind ProviderKind,
    string ProviderName,
    bool IsConfigurable,
    string? Namespace,
    string? ServiceName,
    int? ServicePort,
    string? DirectUrl,
    bool UseHttps,
    string PathPrefix,
    string? BearerToken);
