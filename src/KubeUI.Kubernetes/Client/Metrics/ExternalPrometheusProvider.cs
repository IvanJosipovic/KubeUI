using k8s;

namespace KubeUI.Kubernetes;

public sealed class ExternalPrometheusProvider : PrometheusProviderBase
{
    public override PrometheusProviderKind Kind => PrometheusProviderKind.External;

    public override string Name => "External URL";

    public override bool IsConfigurable => true;

    public override Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl) && string.IsNullOrWhiteSpace(settings.PrometheusServerUrl))
        {
            return Task.FromResult<ResolvedPrometheusEndpoint?>(null);
        }

        var url = string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl)
            ? settings.PrometheusServerUrl
            : settings.PrometheusDirectUrl;

        return Task.FromResult<ResolvedPrometheusEndpoint?>(new ResolvedPrometheusEndpoint(
            Kind,
            Name,
            IsConfigurable,
            null,
            null,
            null,
            url,
            settings.PrometheusUseHttps,
            NormalizePrefix(settings.PrometheusPathPrefix),
            settings.PrometheusBearerToken));
    }
}
