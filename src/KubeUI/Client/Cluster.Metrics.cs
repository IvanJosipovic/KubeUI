using k8s;
using k8s.Models;

namespace KubeUI.Client;

public partial class Cluster
{
    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];
    public bool IsMetricsAvailable { get; private set; }

    private DispatcherTimer _metricsRefreshTimer;

    private async void SyncData(object? sender, EventArgs e)
    {
        try
        {
            var nodeMetricslist = await Client.GetKubernetesNodesMetricsAsync();
            NodeMetrics.Clear();
            foreach (var item in nodeMetricslist.Items)
            {
                NodeMetrics.Add(item);
            }

            var podMetricsList = await Client.GetKubernetesPodsMetricsAsync();
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

    private async Task InitMetrics()
    {
        var kube = Client as Kubernetes;

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "metrics.k8s.io",
                    Resource = "pods",
                    Verb = "list"
                }
            }
        };

        var resp = await kube.CreateSelfSubjectAccessReviewAsync(model);

        var model2 = new V1SelfSubjectAccessReview()
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "metrics.k8s.io",
                    Resource = "nodes",
                    Verb = "list"
                }
            }
        };

        var resp2 = await kube.CreateSelfSubjectAccessReviewAsync(model);

        IsMetricsAvailable = resp.Status.Allowed && resp2.Status.Allowed;

        if (IsMetricsAvailable)
        {
            _metricsRefreshTimer = new(TimeSpan.FromSeconds(30), DispatcherPriority.Background, SyncData);
            _metricsRefreshTimer.Start();
            SyncData(null, null);
        }
    }
}
