using k8s.Models;

namespace KubeUI.Client.Metrics
{
    public interface IMetricsService
    {
        MetricsServiceType MetricsServiceType { get; }
        IEnumerable<NodeMetrics> GetNodeMetrics(string name);
        IEnumerable<PodMetrics> GetPodMetrics(string name, string @namespace);
        Task<PrometheusClientQueryRangeResponse?> GetPrometheusMetics(string query, DateTimeOffset start, DateTimeOffset end, string step = "1");
        void Initialize(ICluster cluster);
    }
}
