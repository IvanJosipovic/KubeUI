using k8s;

namespace KubeUI.Kubernetes;

public sealed class OpenShiftPrometheusProvider : PrometheusProviderBase
{
    public override PrometheusProviderKind Kind => PrometheusProviderKind.OpenShift;

    public override string Name => "OpenShift";

    public override bool IsConfigurable => true;

    public override async Task<ResolvedPrometheusEndpoint?> TryResolveServiceAsync(k8s.Kubernetes client, ClusterMetricsSettings settings, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl))
        {
            return new ResolvedPrometheusEndpoint(
                Kind,
                Name,
                IsConfigurable,
                settings.PrometheusServiceNamespace,
                settings.PrometheusServiceName,
                settings.PrometheusServicePort,
                settings.PrometheusDirectUrl,
                settings.PrometheusUseHttps,
                NormalizePrefix(settings.PrometheusPathPrefix),
                settings.PrometheusBearerToken);
        }

        var service = await FindNamespacedServiceAsync(client, "prometheus-k8s", "openshift-monitoring", cancellationToken).ConfigureAwait(false);
        return service == null ? null : CreateServiceEndpoint(Kind, Name, IsConfigurable, service, settings);
    }
}
