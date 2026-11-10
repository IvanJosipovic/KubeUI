namespace KubeUI.Kubernetes;

public sealed class ManualPrometheusProvider : PrometheusProviderBase
{
    public override PrometheusProviderKind Kind => PrometheusProviderKind.Manual;

    public override string Name => "Manual Service";

    public override bool IsConfigurable => true;

    public override Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl))
        {
            return Task.FromResult<ResolvedPrometheusEndpoint?>(ApplyAuthenticationSettings(new ResolvedPrometheusEndpoint(
                Kind,
                Name,
                IsConfigurable,
                null,
                null,
                null,
                settings.PrometheusDirectUrl,
                false,
                NormalizePrefix(settings.PrometheusPathPrefix)), settings));
        }

        if (string.IsNullOrWhiteSpace(settings.PrometheusServiceNamespace)
            || string.IsNullOrWhiteSpace(settings.PrometheusServiceName)
            || settings.PrometheusServicePort is not > 0)
        {
            return Task.FromResult<ResolvedPrometheusEndpoint?>(null);
        }

        return Task.FromResult<ResolvedPrometheusEndpoint?>(ApplyAuthenticationSettings(new ResolvedPrometheusEndpoint(
            Kind,
            Name,
            IsConfigurable,
            settings.PrometheusServiceNamespace,
            settings.PrometheusServiceName,
            settings.PrometheusServicePort,
            null,
            settings.PrometheusUseHttps,
            NormalizePrefix(settings.PrometheusPathPrefix)), settings));
    }
}
