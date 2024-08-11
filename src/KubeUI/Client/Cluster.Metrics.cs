using k8s;
using k8s.Models;

namespace KubeUI.Client;

public partial class Cluster
{
    [ObservableProperty]
    private ObservableCollection<PodMetrics> _podMetrics = [];

    [ObservableProperty]
    private ObservableCollection<NodeMetrics> _nodeMetrics = [];

    public bool IsMetricsAvailable => APIGroups.Groups.Any(g => g.Name == "metrics.k8s.io");

    private DispatcherTimer _metricsRefreshTimer;

    private async void SyncData(object? sender, EventArgs e)
    {
        try
        {
            var nodeMetricslist = await _client.GetKubernetesNodesMetricsAsync();
            NodeMetrics.Clear();
            foreach (var item in nodeMetricslist.Items)
            {
                NodeMetrics.Add(item);
            }

            var podMetricsList = await _client.GetKubernetesPodsMetricsAsync();
            PodMetrics.Clear();
            foreach (var item in podMetricsList.Items)
            {
                PodMetrics.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Metrics");
        }
    }
}
