using Avalonia.VisualTree;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Styles;
using KubeUI.Kubernetes;

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
    private readonly Canvas _barPanel;
    private readonly Border[] _bars;
    private TableViewCell? _tableCell;
    private VerticalAlignment _originalCellContentAlignment;
    private double _originalCellMinimumHeight;
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
                .Background(_normalBrush)
                .Width(0);
        }

        _bars = bars;
        _barPanel = new Canvas()
            .VerticalAlignment(VerticalAlignment.Stretch)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Children(_bars);
        _barPanel.PropertyChanged += OnBarPanelPropertyChanged;

        this.Content(_barPanel)
            .Margin(4, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
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

    /// <summary>Determines whether two snapshots for one resource use the same metrics query target.</summary>
    protected virtual bool IsSameMetricsTarget(TResource previousResource, TResource currentResource) => true;

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
            UpdateRefreshSubscription();
            RefreshCell();
        }
        else if (change.Property == BoundsProperty && _resource is not null)
        {
            RenderHistory(_resource, _history);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _tableCell = this.GetVisualAncestors().OfType<TableViewCell>().FirstOrDefault();
        if (_tableCell is not null)
        {
            _originalCellContentAlignment = _tableCell.VerticalContentAlignment;
            _tableCell.VerticalContentAlignment = VerticalAlignment.Stretch;
            _originalCellMinimumHeight = MinHeight;
            MinHeight = 0;
        }

        EffectiveViewportChanged += OnEffectiveViewportChanged;
        RefreshCell();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        EffectiveViewportChanged -= OnEffectiveViewportChanged;
        _hasEffectiveViewport = false;
        _isInEffectiveViewport = false;
        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
        if (_tableCell is not null)
        {
            _tableCell.VerticalContentAlignment = _originalCellContentAlignment;
            _tableCell = null;
            MinHeight = _originalCellMinimumHeight;
        }

        PausePendingPrometheusRequest();
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

        var sameResource = IsSameResource(_resource, resource);
        var metricsTargetChanged = backend.Type == MetricsServiceType.Prometheus
            && sameResource
            && _resource is { } previousResource
            && !IsSameMetricsTarget(previousResource, resource);
        var historyInvalidated = !sameResource
            || !Equals(_backend, backend)
            || metricsTargetChanged;
        if (historyInvalidated)
        {
            CancelPendingRequest();
            _history = MetricHistoryData.Empty;
            _nextRefreshUtc = DateTimeOffset.MinValue;
        }

        _resource = resource;
        _backend = backend;

        if (!IsCellVisibleInViewport())
        {
            PausePendingPrometheusRequest();
            if (historyInvalidated)
            {
                ClearBars();
            }

            return;
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
            }

            RenderHistory(resource, _history);
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
        UpdateRefreshSubscription();
        RefreshCell();
    }

    private bool IsCellVisibleInViewport()
    {
        return this.IsAttachedToVisualTree()
            && IsEffectivelyVisible
            && _hasEffectiveViewport
            && _isInEffectiveViewport;
    }

    private void UpdateRefreshSubscription()
    {
        if (IsCellVisibleInViewport())
        {
            _refreshSubscription ??= _refreshClock.Subscribe(RefreshCell);
            return;
        }

        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
        PausePendingPrometheusRequest();
    }

    private void PausePendingPrometheusRequest()
    {
        if (_prometheusCancellation is null)
        {
            return;
        }

        CancelPendingRequest();
        _nextRefreshUtc = DateTimeOffset.MinValue;
    }

    private void OnBarPanelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty && _resource is { } resource)
        {
            RenderHistory(resource, _history);
        }
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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_resource is not { } currentResource
                    || requestVersion != _requestVersion
                    || cancellation.IsCancellationRequested
                    || !ReferenceEquals(Cluster, cluster)
                    || !IsSameResource(currentResource, resource)
                    || !Equals(_backend, backend))
                {
                    return;
                }

                _history = CreateHistory(result, currentResource);
                RenderHistory(currentResource, _history);
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
        var chartWidth = _barPanel.Bounds.Width;
        var chartHeight = _barPanel.Bounds.Height;
        var bars = MetricsHistoryBuckets.CreateBars(points, _timeProvider.GetUtcNow(), limit, chartHeight);
        const double barGap = 2;
        var barWidth = Math.Max(0, (chartWidth - ((_bars.Length - 1) * barGap)) / _bars.Length);
        for (var index = 0; index < _bars.Length; index++)
        {
            var data = bars[index];
            var border = _bars[index];
            var left = index * (barWidth + barGap);
            var top = chartHeight - data.Height;
            var background = data.LimitState switch
            {
                MetricsLimitState.Warning => _warningBrush,
                MetricsLimitState.Exceeded => _exceededBrush,
                _ => _normalBrush,
            };
            var tooltip = data.Height > 0 ? CreateTooltip(data, limit) : null;

            if (border.Width != barWidth)
            {
                border.Width = barWidth;
            }

            if (border.Height != data.Height)
            {
                border.Height = data.Height;
            }

            if (!Canvas.GetLeft(border).Equals(left))
            {
                Canvas.SetLeft(border, left);
            }

            if (!Canvas.GetTop(border).Equals(top))
            {
                Canvas.SetTop(border, top);
            }

            if (!Equals(border.Background, background))
            {
                border.Background = background;
            }

            if (!Equals(ToolTip.GetTip(border), tooltip))
            {
                ToolTip.SetTip(border, tooltip);
            }
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

    private static bool IsSameResource(TResource? previousResource, TResource currentResource)
    {
        if (ReferenceEquals(previousResource, currentResource))
        {
            return true;
        }

        if (previousResource is not IKubernetesObject<V1ObjectMeta> previousKubernetesResource
            || currentResource is not IKubernetesObject<V1ObjectMeta> currentKubernetesResource)
        {
            return false;
        }

        var previousMetadata = previousKubernetesResource.Metadata;
        var currentMetadata = currentKubernetesResource.Metadata;
        return previousMetadata?.Name is { Length: > 0 } name
            && string.Equals(name, currentMetadata?.Name, StringComparison.Ordinal)
            && string.Equals(previousKubernetesResource.ApiVersion, currentKubernetesResource.ApiVersion, StringComparison.Ordinal)
            && string.Equals(previousKubernetesResource.Kind, currentKubernetesResource.Kind, StringComparison.Ordinal)
            && string.Equals(previousMetadata.NamespaceProperty, currentMetadata?.NamespaceProperty, StringComparison.Ordinal);
    }
}
