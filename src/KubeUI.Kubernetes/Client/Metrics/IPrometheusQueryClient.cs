using System.Diagnostics.CodeAnalysis;

namespace KubeUI.Kubernetes;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public API contract; renaming a parameter is a breaking change. Revisit in next major version.")]
public interface IPrometheusQueryClient
{
    Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default);

    Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken = default);

    Task ResetAsync();
}
