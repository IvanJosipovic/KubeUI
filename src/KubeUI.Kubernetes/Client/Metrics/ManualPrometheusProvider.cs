using k8s;

namespace KubeUI.Kubernetes;

public sealed class ManualPrometheusProvider : PrometheusProviderBase
{
    public override PrometheusProviderKind Kind => PrometheusProviderKind.Manual;

    public override string Name => "Manual Service";

    public override bool IsConfigurable => true;

    public override Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.PrometheusServiceNamespace)
            || string.IsNullOrWhiteSpace(settings.PrometheusServiceName)
            || settings.PrometheusServicePort is not > 0)
        {
            return Task.FromResult<ResolvedPrometheusEndpoint?>(null);
        }

        return Task.FromResult<ResolvedPrometheusEndpoint?>(new ResolvedPrometheusEndpoint(
            Kind,
            Name,
            IsConfigurable,
            settings.PrometheusServiceNamespace,
            settings.PrometheusServiceName,
            settings.PrometheusServicePort,
            settings.PrometheusDirectUrl,
            settings.PrometheusUseHttps,
            NormalizePrefix(settings.PrometheusPathPrefix),
            settings.PrometheusBearerToken));
    }
}
