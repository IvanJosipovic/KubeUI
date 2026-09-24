using System.ComponentModel;
using System.Text.Json.Serialization;

namespace KubeUI.Kubernetes;

public enum MetricsServiceType
{
    Auto,
    None,

    [Description("Kubernetes Metrics Server")]
    KubernetesMetricsServer,

    [Description("Prometheus")]
    Prometheus,
}

public partial class ClusterMetricsSettings : ObservableObject
{
    [ObservableProperty]
    public partial MetricsServiceType MetricsServiceType { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServerUrl { get; set; }

    [ObservableProperty]
    public partial PrometheusProviderKind? PrometheusProviderKind { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServiceName { get; set; }

    [ObservableProperty]
    public partial string? PrometheusServiceNamespace { get; set; }

    [ObservableProperty]
    public partial int? PrometheusServicePort { get; set; }

    [ObservableProperty]
    public partial string? PrometheusPathPrefix { get; set; }

    [ObservableProperty]
    public partial bool PrometheusUseHttps { get; set; }

    [ObservableProperty]
    public partial string? PrometheusDirectUrl { get; set; }

    [ObservableProperty]
    [property: JsonIgnore]
    public partial string? PrometheusBearerToken { get; set; }

    [ObservableProperty]
    public partial string? AzureMonitorWorkspaceId { get; set; }

    [ObservableProperty]
    public partial string? AzureMonitorSubscriptionId { get; set; }

    [ObservableProperty]
    public partial string? AzureMonitorQueryEndpoint { get; set; }
}
