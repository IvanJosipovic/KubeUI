using k8s.Models;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using Avalonia.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public abstract class PodMetricCellBase : RefreshingCellTextBlock, IInitializeCluster
{
    private CancellationTokenSource? _prometheusCancellation;
    private V1Pod? _prometheusPod;
    private ActiveMetricsBackend? _prometheusBackend;
    private bool _prometheusRequestActive;
    private bool _prometheusValueLoaded;
    private string? _prometheusText;
    private long _prometheusRequestVersion;

    public ClusterWorkspace? Cluster { get; private set; }

    protected PodMetricCellBase(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        _prometheusCancellation?.Cancel();
        _prometheusCancellation?.Dispose();
        _prometheusCancellation = null;
        _prometheusPod = null;
        _prometheusBackend = null;
        _prometheusRequestActive = false;
        _prometheusValueLoaded = false;
        _prometheusText = null;
        Cluster = cluster;
        RefreshText();
    }

    protected sealed override string ResolveText(object? dataContext)
    {
        if (Cluster == null || dataContext is not V1Pod pod || Cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.None)
        {
            return string.Empty;
        }

        if (Cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.Prometheus)
        {
            StartPrometheusRequest(pod);
            return _prometheusText ?? string.Empty;
        }

        var metric = Cluster.Runtime.PodMetrics.FirstOrDefault(metric =>
            metric.Name() == pod.Name() && metric.Namespace() == pod.Namespace());

        return metric == null ? string.Empty : FormatMetric(metric);
    }

    protected abstract string PrometheusQueryName { get; }

    protected abstract string FormatMetric(PodMetrics metric);

    protected abstract string FormatPrometheusMetric(double value);

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _prometheusCancellation?.Cancel();
        base.OnDetachedFromVisualTree(e);
    }

    private void StartPrometheusRequest(V1Pod pod)
    {
        var backend = Cluster!.Runtime.ActiveMetricsBackend;
        if (ReferenceEquals(_prometheusPod, pod)
            && Equals(_prometheusBackend, backend)
            && (_prometheusRequestActive || _prometheusValueLoaded))
        {
            return;
        }

        _prometheusCancellation?.Cancel();
        _prometheusCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _prometheusCancellation = cancellation;
        _prometheusPod = pod;
        _prometheusBackend = backend;
        _prometheusRequestActive = true;
        _prometheusValueLoaded = false;
        _prometheusText = null;
        var requestVersion = ++_prometheusRequestVersion;

        _ = LoadPrometheusMetricAsync(pod, cancellation, requestVersion);
    }

    private async Task LoadPrometheusMetricAsync(V1Pod pod, CancellationTokenSource cancellation, long requestVersion)
    {
        try
        {
            var result = await Cluster!.Runtime.RequestMetricsAsync(
                new MetricRequest
                {
                    Category = MetricCategory.Pods,
                    RangeSeconds = 300,
                    StepSeconds = 60,
                    Frames = 1,
                    Queries =
                    [
                        new MetricQueryDefinition
                        {
                            Name = PrometheusQueryName,
                            Options = new Dictionary<string, string>(StringComparer.Ordinal)
                            {
                                ["namespace"] = pod.Namespace(),
                                ["pods"] = pod.Name(),
                                ["selector"] = "pod, namespace",
                            },
                        },
                    ],
                },
                cancellation.Token).ConfigureAwait(false);

            var value = result.Metrics.TryGetValue(PrometheusQueryName, out var series)
                ? series.SelectMany(static item => item.Points).LastOrDefault()?.Value
                : null;
            var text = value.HasValue ? FormatPrometheusMetric(value.Value) : string.Empty;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion != _prometheusRequestVersion || cancellation.IsCancellationRequested)
                {
                    return;
                }

                _prometheusRequestActive = false;
                _prometheusValueLoaded = true;
                _prometheusText = text;
                RefreshText();
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion != _prometheusRequestVersion || cancellation.IsCancellationRequested)
                {
                    return;
                }

                _prometheusRequestActive = false;
                _prometheusValueLoaded = true;
                _prometheusText = string.Empty;
                RefreshText();
            });
        }
    }
}
