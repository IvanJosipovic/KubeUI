using Humanizer;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Styles;
using KubeUI.Kubernetes;
using Avalonia.VisualTree;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

/// <summary>Renders reusable CPU or memory history bars for a resource list item.</summary>
/// <typeparam name="TResource">The Kubernetes resource represented by the cell.</typeparam>
public abstract class MetricsHistoryCellBase<TResource> : UserControl, IInitializeCluster
    where TResource : class
{
    private static readonly TimeSpan s_prometheusRefreshInterval = TimeSpan.FromMinutes(1);

    private readonly IUiRefreshClock _refreshClock;
    private readonly TimeProvider _timeProvider;
    private readonly IBrush _normalBrush;
    private readonly IBrush _warningBrush;
    private readonly IBrush _exceededBrush;
    private readonly Grid _barPanel;
    private readonly Border[] _bars;
    private IDisposable? _refreshSubscription;
    private CancellationTokenSource? _prometheusCancellation;
    private TResource? _resource;
    private ActiveMetricsBackend? _backend;
    private MetricHistoryData _history = MetricHistoryData.Empty;
    private DateTimeOffset _nextRefreshUtc;
    private long _requestVersion;
    private bool _hasEffectiveViewport;
    private bool _isInEffectiveViewport;

    protected MetricsHistoryCellBase(
        IUiRefreshClock refreshClock,
        TimeProvider timeProvider)
    {
        _refreshClock = refreshClock;
        _timeProvider = timeProvider;
        _normalBrush = ApplicationBrushResources.GetBrush("SystemAccentColor");
        _warningBrush = ApplicationBrushResources.GetBrush("PodStatusWarningBrush");
        _exceededBrush = ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush");

        var bars = new Border[MetricsHistoryBuckets.BucketCount];
        for (var index = 0; index < bars.Length; index++)
        {
            bars[index] = new Border()
                .Height(0)
                .VerticalAlignment(VerticalAlignment.Bottom)
                .HorizontalAlignment(HorizontalAlignment.Stretch)
                .Background(_normalBrush)
                .Col(index);
        }

        _bars = bars;
        _barPanel = new Grid()
            .Cols("*,*,*,*,*,*,*,*,*,*,*,*")
            .ColumnSpacing(1)
            .VerticalAlignment(VerticalAlignment.Bottom)
            .Children(_bars);

        this.Content(_barPanel)
            .Margin(4, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Center)
            .MinHeight(24);
    }

    public ClusterWorkspace? Cluster { get; private set; }

    protected abstract bool IsMemoryMetric { get; }

    protected abstract MetricResultSet CaptureMetricsServerHistory(
        ClusterWorkspace cluster,
        TResource resource,
        DateTimeOffset start,
        DateTimeOffset end);

    protected abstract MetricRequest CreatePrometheusRequest(TResource resource, DateTimeOffset end);

    protected abstract double? GetMetricLimit(TResource resource);

    protected virtual bool MatchesSeries(TResource resource, MetricSeries series) => true;

    protected static MetricRequest CreateMetricRequest(
        MetricCategory category,
        IReadOnlyDictionary<string, string> options,
        DateTimeOffset end)
    {
        return new MetricRequest
        {
            Category = category,
            Start = end.AddHours(-1),
            End = end,
            RangeSeconds = 3600,
            StepSeconds = 60,
            Frames = 60,
            Queries =
            [
                new MetricQueryDefinition { Name = "cpuUsage", Options = options },
                new MetricQueryDefinition { Name = "memoryUsage", Options = options },
            ],
        };
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        if (!ReferenceEquals(Cluster, cluster))
        {
            CancelPendingRequest();
            _resource = null;
            _backend = null;
            _history = MetricHistoryData.Empty;
        }

        Cluster = cluster;
        RefreshCell();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshCell();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty
            || change.Property.Name == nameof(IsEffectivelyVisible))
        {
            RefreshCell();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        EffectiveViewportChanged += OnEffectiveViewportChanged;
        _refreshSubscription = _refreshClock.Subscribe(RefreshCell);
        RefreshCell();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        EffectiveViewportChanged -= OnEffectiveViewportChanged;
        _hasEffectiveViewport = false;
        _isInEffectiveViewport = false;
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
        CancelPendingRequest();
        base.OnDetachedFromVisualTree(e);
    }

    private void RefreshCell()
    {
        if (Cluster == null || DataContext is not TResource resource)
        {
            CancelPendingRequest();
            _resource = null;
            _history = MetricHistoryData.Empty;
            ClearBars();
            return;
        }

        var backend = Cluster.Runtime.ActiveMetricsBackend;
        if (backend.Type is not (MetricsServiceType.KubernetesMetricsServer or MetricsServiceType.Prometheus))
        {
            _resource = resource;
            _backend = backend;
            _history = MetricHistoryData.Empty;
            CancelPendingRequest();
            ClearBars();
            return;
        }

        if (!ReferenceEquals(_resource, resource) || !Equals(_backend, backend))
        {
            CancelPendingRequest();
            _resource = resource;
            _backend = backend;
            _history = MetricHistoryData.Empty;
            _nextRefreshUtc = DateTimeOffset.MinValue;
        }

        if (backend.Type == MetricsServiceType.KubernetesMetricsServer)
        {
            var now = _timeProvider.GetUtcNow();
            if (now >= _nextRefreshUtc)
            {
                _nextRefreshUtc = now.AddSeconds(30);
                var result = CaptureMetricsServerHistory(
                    Cluster,
                    resource,
                    now - MetricsHistoryBuckets.History,
                    now);
                _history = CreateHistory(result, resource);
                RenderHistory(resource, _history);
            }

            return;
        }

        if (!this.IsAttachedToVisualTree()
            || !IsEffectivelyVisible
            || !_hasEffectiveViewport
            || !_isInEffectiveViewport)
        {
            CancelPendingRequest();
            return;
        }

        if (_timeProvider.GetUtcNow() >= _nextRefreshUtc && _prometheusCancellation == null)
        {
            _nextRefreshUtc = _timeProvider.GetUtcNow() + s_prometheusRefreshInterval;
            _ = LoadPrometheusHistory(resource, backend);
        }

        RenderHistory(resource, _history);
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _hasEffectiveViewport = true;
        _isInEffectiveViewport = e.EffectiveViewport.Intersects(new Rect(Bounds.Size));
        RefreshCell();
    }

    private async Task LoadPrometheusHistory(TResource resource, ActiveMetricsBackend backend)
    {
        var cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _prometheusCancellation = cancellation;
        var requestVersion = ++_requestVersion;

        try
        {
            var requestEnd = DateTimeOffset.FromUnixTimeSeconds(_timeProvider.GetUtcNow().ToUnixTimeSeconds() / 60 * 60);
            var result = await cluster.Runtime.RequestMetricsAsync(
                CreatePrometheusRequest(resource, requestEnd),
                CancellationToken.None).WaitAsync(cancellation.Token).ConfigureAwait(false);
            var history = CreateHistory(result, resource);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion != _requestVersion
                    || cancellation.IsCancellationRequested
                    || !ReferenceEquals(Cluster, cluster)
                    || !ReferenceEquals(_resource, resource)
                    || !Equals(_backend, backend))
                {
                    return;
                }

                _history = history;
                RenderHistory(resource, history);
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion == _requestVersion && !cancellation.IsCancellationRequested)
                {
                    _history = MetricHistoryData.Empty;
                    ClearBars();
                }
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion == _requestVersion)
                {
                    _prometheusCancellation?.Dispose();
                    _prometheusCancellation = null;
                }
            });
        }
    }

    private MetricHistoryData CreateHistory(MetricResultSet result, TResource resource)
    {
        return new MetricHistoryData(
            AggregatePrometheusSeries(result, "cpuUsage", resource),
            AggregatePrometheusSeries(result, "memoryUsage", resource));
    }

    private IReadOnlyList<MetricPoint> AggregatePrometheusSeries(MetricResultSet result, string name, TResource resource)
    {
        if (!result.Metrics.TryGetValue(name, out var series))
        {
            return [];
        }

        Dictionary<DateTimeOffset, double> values = [];
        foreach (var metricSeries in series)
        {
            if (!MatchesSeries(resource, metricSeries))
            {
                continue;
            }

            foreach (var point in metricSeries.Points)
            {
                values.TryGetValue(point.Timestamp, out var total);
                values[point.Timestamp] = total + point.Value;
            }
        }

        return values
            .OrderBy(static pair => pair.Key)
            .Select(static pair => new MetricPoint(pair.Key, pair.Value))
            .ToArray();
    }

    private void RenderHistory(TResource resource, MetricHistoryData history)
    {
        var points = IsMemoryMetric ? history.Memory : history.Cpu;
        var limit = GetMetricLimit(resource);
        var bars = MetricsHistoryBuckets.CreateBars(points, _timeProvider.GetUtcNow(), limit);
        for (var index = 0; index < _bars.Length; index++)
        {
            var data = bars[index];
            var border = _bars[index];
            border.Height = data.Height;
            border.Background = data.LimitState switch
            {
                MetricsLimitState.Warning => _warningBrush,
                MetricsLimitState.Exceeded => _exceededBrush,
                _ => _normalBrush,
            };
            ToolTip.SetTip(border, data.Height > 0 ? CreateTooltip(data, limit) : null);
        }
    }

    private string CreateTooltip(MetricsBarData bar, double? limit)
    {
        var range = $"{bar.Start.ToLocalTime():t}–{bar.End.ToLocalTime():t}";
        var value = FormatValue(bar.Value);
        return limit is > 0
            ? string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                Assets.Resources.MetricsHistoryCell_TooltipWithLimit!,
                range,
                Assets.Resources.Metrics_Usage,
                value,
                Assets.Resources.Metrics_Limits,
                FormatValue(limit.Value),
                bar.Value / limit.Value)
            : string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                Assets.Resources.MetricsHistoryCell_TooltipWithoutLimit!,
                range,
                Assets.Resources.Metrics_Usage,
                value);
    }

    private string FormatValue(double value)
    {
        return IsMemoryMetric
            ? ((long)value).Bytes().Humanize()
            : $"{value:0.##}c";
    }

    private void ClearBars()
    {
        foreach (var bar in _bars)
        {
            bar.Height = 0;
            ToolTip.SetTip(bar, null);
        }
    }

    private void CancelPendingRequest()
    {
        _requestVersion++;
        _prometheusCancellation?.Cancel();
        _prometheusCancellation?.Dispose();
        _prometheusCancellation = null;
    }
}
