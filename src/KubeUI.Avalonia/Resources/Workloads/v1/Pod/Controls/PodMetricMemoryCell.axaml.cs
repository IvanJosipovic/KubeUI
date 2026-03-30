using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Humanizer;
using k8s.Models;
using KubeUI.Kubernetes;
using System.Collections.Concurrent;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;

public partial class PodMetricMemoryCell : UserControl, IInitializeCluster
{
    private static readonly TimeSpan s_prometheusRefreshInterval = TimeSpan.FromSeconds(30);
    private ClusterWorkspaceViewModel? _cluster;
    private bool _isRefreshing;

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private static readonly ConcurrentDictionary<string, (DateTimeOffset Timestamp, string Value)> s_prometheusValues = new(StringComparer.Ordinal);

    [GeneratedDirectProperty]
    public partial string PrettyString { get; set; } = string.Empty;

    public PodMetricMemoryCell()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(30);
            s_timer.Start();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _ = UpdateAsync();
    }

    private async void Timer_Tick(object? sender, EventArgs e) => await UpdateAsync();

    private async Task UpdateAsync()
    {
        if (_cluster == null || DataContext is not V1Pod pod)
        {
            await SetPrettyStringAsync(string.Empty).ConfigureAwait(false);
            return;
        }

        switch (_cluster.MetricsServiceType)
        {
            case MetricsServiceType.KubernetesMetricsServer:
                UpdateFromMetricsServer(pod);
                break;
            case MetricsServiceType.Prometheus:
                await UpdateFromPrometheusAsync(pod).ConfigureAwait(false);
                break;
            default:
                await SetPrettyStringAsync(string.Empty).ConfigureAwait(false);
                break;
        }
    }

    private void UpdateFromMetricsServer(V1Pod pod)
    {
        var metric = _cluster?.PodMetrics.FirstOrDefault(m =>
            m.Name() == pod.Name() && m.Namespace() == pod.Namespace());

        if (metric == null)
        {
            PrettyString = string.Empty;
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

    private async Task UpdateFromPrometheusAsync(V1Pod pod)
    {
        var cacheKey = $"{_cluster!.Name}:{pod.Namespace()}:{pod.Name()}";
        if (s_prometheusValues.TryGetValue(cacheKey, out var cached)
            && DateTimeOffset.UtcNow - cached.Timestamp < s_prometheusRefreshInterval)
        {
            await SetPrettyStringAsync(cached.Value).ConfigureAwait(false);
            return;
        }

        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;

        try
        {
            var result = await _cluster.RequestMetricsAsync(new MetricRequest
            {
                Category = MetricCategory.Pods,
                RangeSeconds = 300,
                StepSeconds = 60,
                Frames = 5,
                Queries =
                [
                    new MetricQueryDefinition
                    {
                        Name = "memoryUsage",
                        Options = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["namespace"] = pod.Namespace(),
                            ["pods"] = System.Text.RegularExpressions.Regex.Escape(pod.Name()),
                            ["selector"] = "pod, namespace",
                        },
                    },
                ],
            }).ConfigureAwait(false);

            var latest = result.Metrics.TryGetValue("memoryUsage", out var series)
                ? series.SelectMany(static x => x.Points).OrderBy(static x => x.Timestamp).LastOrDefault()
                : null;

            var prettyString = latest == null ? string.Empty : ((long)latest.Value).Bytes().Humanize();
            await SetPrettyStringAsync(prettyString).ConfigureAwait(false);
            s_prometheusValues[cacheKey] = (DateTimeOffset.UtcNow, prettyString);
        }
        catch
        {
            await SetPrettyStringAsync(string.Empty).ConfigureAwait(false);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private Task SetPrettyStringAsync(string value)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            PrettyString = value;
            return Task.CompletedTask;
        }

        return Dispatcher.UIThread.InvokeAsync(() => PrettyString = value).GetTask();
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
        _ = UpdateAsync();
    }
}


