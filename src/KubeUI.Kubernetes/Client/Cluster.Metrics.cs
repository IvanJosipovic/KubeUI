using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public partial class Cluster
{
    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];
    public bool IsMetricsAvailable { get; private set; }

    private PeriodicTimer? _metricsRefreshTimer;
    private CancellationTokenSource? _metricsRefreshCancellationTokenSource;

    private async Task SyncData(CancellationToken cancellationToken)
    {
        using var activity = StartClusterActivity(nameof(SyncData));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeMetricslist = await Client.GetKubernetesNodesMetricsAsync();
            NodeMetrics.Clear();
            foreach (NodeMetrics item in ((IEnumerable)nodeMetricslist.Items).OfType<NodeMetrics>())
            {
                NodeMetrics.Add(item);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var podMetricsList = await Client.GetKubernetesPodsMetricsAsync();
            PodMetrics.Clear();
            foreach (PodMetrics item in ((IEnumerable)podMetricsList.Items).OfType<PodMetrics>())
            {
                PodMetrics.Add(item);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error updating Metrics");
        }
    }

    private async Task RefreshMetricsAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        using var activity = StartClusterActivity(nameof(RefreshMetricsAsync));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await RefreshMetricsDataAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task RefreshMetricsDataAsync(CancellationToken cancellationToken)
    {
        using var activity = StartClusterActivity(nameof(RefreshMetricsDataAsync));
        await SyncData(cancellationToken).ConfigureAwait(false);
    }

    private async Task InitMetrics()
    {
        using var activity = StartClusterActivity(nameof(InitMetrics));

        var kube = Client as k8s.Kubernetes;

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

        var APIGroups = await Client.Apis.GetAPIVersionsAsync();

        IsMetricsAvailable = APIGroups.Groups.Any(g => g.Name == "metrics.k8s.io") && resp.Status.Allowed && resp2.Status.Allowed;

        if (IsMetricsAvailable)
        {
            _metricsRefreshCancellationTokenSource?.Cancel();
            _metricsRefreshTimer?.Dispose();

            var metricsRefreshCancellationTokenSource = new CancellationTokenSource();
            var metricsRefreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            _metricsRefreshCancellationTokenSource = metricsRefreshCancellationTokenSource;
            _metricsRefreshTimer = metricsRefreshTimer;

            await RefreshMetricsDataAsync(metricsRefreshCancellationTokenSource.Token);

            using (ExecutionContext.SuppressFlow())
            {
                _ = Task.Run(() => RefreshMetricsAsync(metricsRefreshTimer, metricsRefreshCancellationTokenSource.Token));
            }
        }
    }
}

