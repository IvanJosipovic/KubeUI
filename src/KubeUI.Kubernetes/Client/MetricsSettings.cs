using System.ComponentModel;

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

    /// <summary>Gets the static custom headers sent to the configured Prometheus endpoint.</summary>
    [ObservableProperty]
    public partial ObservableCollection<PrometheusHeader> PrometheusCustomHeaders { get; set; } = [];

    /// <summary>Gets or sets the Basic Auth username for the configured Prometheus endpoint.</summary>
    [ObservableProperty]
    public partial string? PrometheusBasicAuthUsername { get; set; }

    /// <summary>Gets or sets the Basic Auth password for the configured Prometheus endpoint.</summary>
    [ObservableProperty]
    public partial string? PrometheusBasicAuthPassword { get; set; }

    /// <summary>Gets or sets whether authentication must use an HTTPS endpoint.</summary>
    [ObservableProperty]
    public partial bool PrometheusRequireHttpsForAuthentication { get; set; } = true;

    [ObservableProperty]
    public partial string? AzureMonitorWorkspaceId { get; set; }

    [ObservableProperty]
    public partial string? AzureMonitorSubscriptionId { get; set; }

    [ObservableProperty]
    public partial string? AzureMonitorQueryEndpoint { get; set; }
}

/// <summary>A static HTTP header configured for Prometheus requests.</summary>
public sealed partial class PrometheusHeader : ObservableObject
{
    /// <summary>Gets or sets the HTTP header name.</summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP header value.</summary>
    [ObservableProperty]
    public partial string Value { get; set; } = string.Empty;
}
