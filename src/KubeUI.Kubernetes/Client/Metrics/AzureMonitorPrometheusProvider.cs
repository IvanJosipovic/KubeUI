namespace KubeUI.Kubernetes;

/// <summary>
/// Resolves the selected Azure Monitor workspace as an authenticated Prometheus endpoint.
/// </summary>
public sealed class AzureMonitorPrometheusProvider : PrometheusProviderBase
{
    /// <inheritdoc />
    public override PrometheusProviderKind Kind => PrometheusProviderKind.AzureMonitor;

    /// <inheritdoc />
    public override string Name => "Azure Managed Prometheus";

    /// <inheritdoc />
    public override bool IsConfigurable => true;

    /// <inheritdoc />
    public override Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(settings.AzureMonitorWorkspaceId)
            || !Uri.TryCreate(settings.AzureMonitorQueryEndpoint, UriKind.Absolute, out var endpoint)
            || (endpoint.Scheme != Uri.UriSchemeHttps && endpoint.Scheme != Uri.UriSchemeHttp))
        {
            return Task.FromResult<ResolvedPrometheusEndpoint?>(null);
        }

        return Task.FromResult<ResolvedPrometheusEndpoint?>(new ResolvedPrometheusEndpoint(
            Kind,
            Name,
            IsConfigurable,
            null,
            null,
            null,
            endpoint.GetLeftPart(UriPartial.Authority),
            endpoint.Scheme == Uri.UriSchemeHttps,
            endpoint.AbsolutePath.TrimEnd('/'),
            null,
            UseAzureMonitorAuthentication: true));
    }
}
