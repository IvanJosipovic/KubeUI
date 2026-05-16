using Humanizer;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;

public partial class PodMetricMemoryCell : UserControl, IInitializeCluster
{
    private ClusterWorkspaceViewModel? _cluster;
    private Task? _prometheusRefreshTask;
    private string? _prometheusResourceKey;

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricMemoryCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(1);
            s_timer.Start();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Update();
    }

    private void Timer_Tick(object? sender, EventArgs e) => Update();

    private void Update()
    {
        if (_cluster == null || DataContext is not V1Pod pod)
        {
            return;
        }

        if (_cluster.MetricsServiceType == MetricsServiceType.Prometheus)
        {
            QueuePrometheusUpdate(pod);
            return;
        }

        var metric = _cluster.PodMetrics.FirstOrDefault(m =>
            m.Name() == pod.Name() && m.Namespace() == pod.Namespace());

        if (metric == null)
        {
            return;
        }

        try
        {
            var usageBytes = metric.Containers.Sum(c => c.Usage["memory"].ToInt64());
            PrettyString = usageBytes.Bytes().Humanize();
        }
        catch
        {
            PrettyString = string.Empty;
        }
    }

    private void QueuePrometheusUpdate(V1Pod pod)
    {
        var resourceKey = $"{pod.Namespace()}/{pod.Name()}";
        if (_prometheusRefreshTask?.IsCompleted == false && string.Equals(_prometheusResourceKey, resourceKey, StringComparison.Ordinal))
        {
            return;
        }

        _prometheusResourceKey = resourceKey;
        _prometheusRefreshTask = UpdatePrometheusAsync(pod);
    }

    private async Task UpdatePrometheusAsync(V1Pod pod)
    {
        if (_cluster == null)
        {
            return;
        }

        var result = await _cluster.RequestMetricsAsync(new MetricRequest
        {
            Category = MetricCategory.Pods,
            Queries =
            [
                new MetricQueryDefinition
                {
                    Name = "memoryUsage",
                    Options = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["namespace"] = pod.Namespace(),
                        ["pods"] = pod.Name(),
                    },
                },
            ],
        }).ConfigureAwait(false);

        var value = result.Metrics.TryGetValue("memoryUsage", out var series)
            ? series.SelectMany(static x => x.Points).LastOrDefault().Value
            : 0d;

        await Dispatcher.UIThread.InvokeAsync(() => PrettyString = ((long)value).Bytes().Humanize());
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        _cluster = cluster;
    }
}


