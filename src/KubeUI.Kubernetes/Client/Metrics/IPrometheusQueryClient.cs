namespace KubeUI.Kubernetes;

internal interface IPrometheusQueryClient
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
