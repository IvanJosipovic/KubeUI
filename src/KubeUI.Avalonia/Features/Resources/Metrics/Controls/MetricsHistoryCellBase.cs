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
    private CancellationTokenSource? _metricsServerCancellation;
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
        RefreshCell(forceRender: true);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RefreshCell(forceRender: true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty
            || change.Property.Name == nameof(IsEffectivelyVisible))
        {
            UpdateRefreshSubscription();
            RefreshCell(forceRender: IsCellVisibleInViewport());
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
        RefreshCell(forceRender: true);
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

        PausePendingHistoryRequest();
        base.OnDetachedFromVisualTree(e);
    }

    private void RefreshCell(bool forceRender = false)
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
            PausePendingHistoryRequest();
            if (historyInvalidated)
            {
                ClearBars();
            }

            return;
        }

        if (backend.Type == MetricsServiceType.KubernetesMetricsServer)
        {
            var now = _timeProvider.GetUtcNow();
            if (now >= _nextRefreshUtc && _metricsServerCancellation == null)
            {
                _nextRefreshUtc = now.AddSeconds(30);
                _ = LoadMetricsServerHistory(resource, backend, now);
                return;
            }

            if (forceRender)
            {
                RenderHistory(resource, _history);
            }

            return;
        }

        var refreshPrometheus = _timeProvider.GetUtcNow() >= _nextRefreshUtc && _prometheusCancellation == null;
        if (refreshPrometheus)
        {
            _nextRefreshUtc = _timeProvider.GetUtcNow() + s_prometheusRefreshInterval;
            _ = LoadPrometheusHistory(resource, backend);
        }

        if (forceRender || refreshPrometheus)
        {
            RenderHistory(resource, _history);
        }
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        var wasVisible = IsCellVisibleInViewport();
        _hasEffectiveViewport = true;
        _isInEffectiveViewport = e.EffectiveViewport.Intersects(new Rect(Bounds.Size));
        UpdateRefreshSubscription();
        RefreshCell(forceRender: !wasVisible && IsCellVisibleInViewport());
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
            _refreshSubscription ??= _refreshClock.Subscribe(() => RefreshCell());
            return;
        }

        _refreshSubscription?.Dispose();
        _refreshSubscription = null;
        PausePendingHistoryRequest();
    }

    private void PausePendingHistoryRequest()
    {
        if (_prometheusCancellation is null && _metricsServerCancellation is null)
        {
            return;
        }

        CancelPendingRequest();
        _nextRefreshUtc = DateTimeOffset.MinValue;
    }

    private async Task LoadMetricsServerHistory(TResource resource, ActiveMetricsBackend backend, DateTimeOffset end)
    {
        var cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _metricsServerCancellation = cancellation;
        var requestVersion = ++_requestVersion;

        try
        {
            var history = await Task.Run(
                () =>
                {
                    var result = CaptureMetricsServerHistory(
                        cluster,
                        resource,
                        end - MetricsHistoryBuckets.History,
                        end);
                    return MetricsHistoryAggregator.Aggregate(
                        result,
                        resource,
                        MatchesSeries,
                        cancellation.Token);
                },
                cancellation.Token).ConfigureAwait(false);

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

                _history = history;
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
                if (requestVersion == _requestVersion && ReferenceEquals(_metricsServerCancellation, cancellation))
                {
                    _metricsServerCancellation.Dispose();
                    _metricsServerCancellation = null;
                }
            });
        }
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
            var history = await MetricsHistoryAggregator.AggregateAsync(
                result,
                resource,
                MatchesSeries,
                cancellation.Token).ConfigureAwait(false);
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

                _history = history;
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
        _metricsServerCancellation?.Cancel();
        _metricsServerCancellation?.Dispose();
        _metricsServerCancellation = null;
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

internal static class MetricsHistoryAggregator
{
    internal static Task<MetricHistoryData> AggregateAsync<TResource>(
        MetricResultSet result,
        TResource resource,
        Func<TResource, MetricSeries, bool> matchesSeries,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => Aggregate(result, resource, matchesSeries, cancellationToken),
            cancellationToken);
    }

    internal static MetricHistoryData Aggregate<TResource>(
        MetricResultSet result,
        TResource resource,
        Func<TResource, MetricSeries, bool> matchesSeries,
        CancellationToken cancellationToken)
    {
        return new MetricHistoryData(
            AggregateSeries(result, "cpuUsage", resource, matchesSeries, cancellationToken),
            AggregateSeries(result, "memoryUsage", resource, matchesSeries, cancellationToken));
    }

    private static IReadOnlyList<MetricPoint> AggregateSeries<TResource>(
        MetricResultSet result,
        string name,
        TResource resource,
        Func<TResource, MetricSeries, bool> matchesSeries,
        CancellationToken cancellationToken)
    {
        if (!result.Metrics.TryGetValue(name, out var series))
        {
            return [];
        }

        Dictionary<DateTimeOffset, double> values = [];
        foreach (var metricSeries in series)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!matchesSeries(resource, metricSeries))
            {
                continue;
            }

            var pointIndex = 0;
            foreach (var point in metricSeries.Points)
            {
                if ((pointIndex++ & 127) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                values.TryGetValue(point.Timestamp, out var total);
                values[point.Timestamp] = total + point.Value;
            }
        }

        return values
            .OrderBy(static pair => pair.Key)
            .Select(static pair => new MetricPoint(pair.Key, pair.Value))
            .ToArray();
    }
}
