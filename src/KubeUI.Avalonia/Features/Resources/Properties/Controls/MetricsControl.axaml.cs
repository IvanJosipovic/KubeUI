using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentIcons.Common;
using Humanizer;
using k8s;
using k8s.Models;
using LiveChartsCore.Drawing;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Themes;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class MetricsControl : UserControl, IInitializeCluster
{
    private static readonly TimeSpan s_metricsServerSampleInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan s_prometheusPanelTimeout = TimeSpan.FromSeconds(15);
    private const string s_loadingStatusText = "Loading metrics...";
    private const string s_noPrometheusMetricsText = "No Prometheus metrics are available for this tab.";
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private static readonly IReadOnlyList<MetricTimeRangeOption> s_timeRangeOptions =
    [
        new MetricTimeRangeOption("1h", 3600),
        new MetricTimeRangeOption("2h", 7200),
        new MetricTimeRangeOption("4h", 14400),
        new MetricTimeRangeOption("24h", 86400),
        new MetricTimeRangeOption("48h", 172800),
        new MetricTimeRangeOption("1 week", 604800),
        new MetricTimeRangeOption("1 month", 2592000),
        new MetricTimeRangeOption("2 months", 5184000),
    ];
    private static readonly MetricTimeRangeOption s_defaultTimeRange = s_timeRangeOptions[0];
    private ClusterWorkspaceViewModel? _cluster;
    private MetricsServerHistoryState? _metricsServerHistory;
    private string? _prometheusCacheKey;
    private int? _prometheusCacheRangeSeconds;
    private bool _suppressSelectedTabRefresh;
    private readonly ILogger<MetricsControl> _logger;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private int _refreshPending;
    private static readonly IReadOnlyList<MetricTimeRangeOption> s_clusterTimeRangeOptions =
    [
        new MetricTimeRangeOption("1h", 3600),
        new MetricTimeRangeOption("2h", 7200),
        new MetricTimeRangeOption("4h", 14400),
        new MetricTimeRangeOption("24h", 86400),
    ];

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricTabViewModel> Tabs { get; set; } = [];

    [GeneratedDirectProperty]
    public partial string? StatusText { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowStatus { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowTabs { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricSummaryItemViewModel> CurrentMetrics { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool ShowCurrentMetrics { get; set; }

    [GeneratedDirectProperty]
    public partial MetricTabViewModel? SelectedTab { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowTimeRangeSelector { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<ClusterScopeOption> ClusterScopeOptions { get; set; } = [];

    [GeneratedDirectProperty]
    public partial ClusterScopeOption? SelectedClusterScope { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowClusterScopeSelector { get; set; }

    [GeneratedDirectProperty]
    public partial ClusterOverviewChartPanelViewModel ClusterTrendChart { get; set; } = new()
    {
        Title = "Cluster CPU Trend",
        EmptyText = "Trend data is unavailable for the selected backend.",
    };

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricTabViewModel> ClusterOverviewModes { get; set; } = [];

    [GeneratedDirectProperty]
    public partial MetricTabViewModel? SelectedClusterOverviewMode { get; set; }

    [GeneratedDirectProperty]
    public partial ClusterOverviewRingChartViewModel ClusterCpuChart { get; set; } = new()
    {
        Title = "CPU",
        EmptyText = "CPU chart data is unavailable for the selected backend.",
    };

    [GeneratedDirectProperty]
    public partial ClusterOverviewRingChartViewModel ClusterMemoryChart { get; set; } = new()
    {
        Title = "Memory",
        EmptyText = "Memory chart data is unavailable for the selected backend.",
    };

    [GeneratedDirectProperty]
    public partial ClusterOverviewRingChartViewModel ClusterPodChart { get; set; } = new()
    {
        Title = "Pods",
        EmptyText = "Pod chart data is unavailable for the selected backend.",
    };

    public static readonly StyledProperty<V1Pod?> PodProperty =
        AvaloniaProperty.Register<MetricsControl, V1Pod?>(nameof(Pod));

    public static readonly StyledProperty<V1Container?> ContainerProperty =
        AvaloniaProperty.Register<MetricsControl, V1Container?>(nameof(Container));

    public static readonly StyledProperty<MetricTimeRangeOption?> SelectedTimeRangeProperty =
        AvaloniaProperty.Register<MetricsControl, MetricTimeRangeOption?>(nameof(SelectedTimeRange));

    public static readonly StyledProperty<bool> ClusterOverviewModeProperty =
        AvaloniaProperty.Register<MetricsControl, bool>(nameof(ClusterOverviewMode));

    static MetricsControl()
    {
        PodProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        ContainerProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        SelectedTimeRangeProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        ClusterOverviewModeProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        SelectedClusterScopeProperty.Changed.AddClassHandler<MetricsControl>((control, _) =>
        {
            if (control.ClusterOverviewMode)
            {
                control.QueueRefresh();
            }
        });
        SelectedClusterOverviewModeProperty.Changed.AddClassHandler<MetricsControl>((control, _) =>
        {
            if (control.ClusterOverviewMode)
            {
                control.SyncClusterOverviewTabSelection();
                control.QueueRefresh();
            }
        });
        BoundsProperty.Changed.AddClassHandler<MetricsControl>((control, args) =>
        {
            if (args.NewValue is Rect bounds && bounds.Width > 0 && bounds.Height > 0)
            {
                control.QueueChartRelayout();
            }
        });
        SelectedTabProperty.Changed.AddClassHandler<MetricsControl>((control, _) =>
        {
            if (!control._suppressSelectedTabRefresh)
            {
                control.QueueRefresh();
            }
        });
    }

    public IReadOnlyList<MetricTimeRangeOption> TimeRangeOptions => s_timeRangeOptions;

    public IReadOnlyList<MetricTimeRangeOption> ClusterTimeRangeOptions => s_clusterTimeRangeOptions;

    public MetricTimeRangeOption? SelectedTimeRange
    {
        get => GetValue(SelectedTimeRangeProperty);
        set => SetValue(SelectedTimeRangeProperty, value);
    }

    public V1Pod? Pod
    {
        get => GetValue(PodProperty);
        set => SetValue(PodProperty, value);
    }

    public V1Container? Container
    {
        get => GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    public bool ClusterOverviewMode
    {
        get => GetValue(ClusterOverviewModeProperty);
        set => SetValue(ClusterOverviewModeProperty, value);
    }

    public MetricsControl()
    {
        InitializeComponent();
        _logger = Application.Current.GetRequiredService<ILogger<MetricsControl>>();
        SelectedTimeRange = s_defaultTimeRange;

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(30);
            s_timer.Start();
        }
    }

    private void ClusterOverviewMode_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.Tag is not string tag)
        {
            return;
        }

        if (tag == "CPU" && ClusterOverviewModes.Count > 0)
        {
            SelectedClusterOverviewMode = ClusterOverviewModes[0];
        }
        else if (tag == "Memory" && ClusterOverviewModes.Count > 1)
        {
            SelectedClusterOverviewMode = ClusterOverviewModes[1];
        }
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        UnsubscribeCluster();
        _cluster = cluster;
        if (ClusterScopeOptions.Count == 0)
        {
            ClusterScopeOptions.Add(new ClusterScopeOption("All Nodes", ClusterScope.All));
            ClusterScopeOptions.Add(new ClusterScopeOption("Worker Nodes", ClusterScope.WorkerNodes));
            ClusterScopeOptions.Add(new ClusterScopeOption("Master Nodes", ClusterScope.MasterNodes));
        }

        if (ClusterOverviewModes.Count == 0)
        {
            ClusterOverviewModes.Add(new MetricTabViewModel { Title = "CPU", IsSelected = true });
            ClusterOverviewModes.Add(new MetricTabViewModel { Title = "Memory" });
        }

        SelectedClusterScope ??= ClusterScopeOptions[0];
        SelectedClusterOverviewMode ??= ClusterOverviewModes[0];
        SyncClusterOverviewTabSelection();
        SubscribeCluster();
        QueueRefresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
        QueueRefresh();
        QueueChartRelayout();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
        UnsubscribeCluster();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        QueueRefresh();
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        QueueRefresh();
    }

    private void SubscribeCluster()
    {
        if (_cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        }

        if (_cluster?.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }
    }

    private void UnsubscribeCluster()
    {
        if (_cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        }

        if (_cluster?.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }
    }

    private void OnMetricsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueRefresh();
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind)
            or nameof(ClusterWorkspaceViewModel.IsMetricsAvailable))
        {
            QueueRefresh();
        }
    }

    private void QueueRefresh()
    {
        Interlocked.Exchange(ref _refreshPending, 1);
        _ = RefreshLoopAsync();
    }

    private void QueueChartRelayout()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(QueueChartRelayout, DispatcherPriority.Loaded);
            return;
        }

        if (VisualRoot == null || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (VisualRoot == null)
            {
                return;
            }

            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();

            foreach (var chart in this.GetVisualDescendants().OfType<LiveChartsCore.SkiaSharpView.Avalonia.CartesianChart>())
            {
                chart.InvalidateMeasure();
                chart.InvalidateArrange();
                chart.InvalidateVisual();
            }
        }, DispatcherPriority.Loaded);
    }

    private async Task RefreshLoopAsync()
    {
        if (!await _refreshGate.WaitAsync(0).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            while (Interlocked.Exchange(ref _refreshPending, 0) == 1)
            {
                try
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        await RefreshCoreAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () => await RefreshCoreAsync().ConfigureAwait(false));
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Metrics refresh canceled for cluster {ClusterName}.", _cluster?.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error refreshing metrics for cluster {ClusterName}.", _cluster?.Name);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (Tabs.Count == 0 && CurrentMetrics.Count == 0)
                        {
                            DisposeTabs();
                            CurrentMetrics.Clear();
                            SelectedTab = null;
                            ShowTabs = false;
                            ShowCurrentMetrics = false;
                            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
                            ShowStatus = true;
                            StatusText = "Unable to load metrics.";
                        }
                    });
                }
            }
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private async Task RefreshCoreAsync()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            throw new InvalidOperationException("MetricsControl refresh must run on the UI thread.");
        }

        if (Pod != null && Container != null)
        {
            await RefreshPodContainerCoreAsync().ConfigureAwait(false);
            return;
        }

if (ClusterOverviewMode && _cluster != null)
{
await RefreshClusterOverviewCoreAsync().ConfigureAwait(false);
return;
}

if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
{
IsVisible = false;
DisposeTabs();
            CurrentMetrics.Clear();
            SelectedTab = null;
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;

        if (Pod != null && Container != null)
        {
            await RefreshPodContainerCoreAsync().ConfigureAwait(false);
            return;
        }

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            var historyTabs = TryCaptureMetricsServerCharts(resource);
            MergeTabs(historyTabs);
            ShowTabs = Tabs.Count > 0;
            ShowCurrentMetrics = false;
            CurrentMetrics.Clear();
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs
                ? null
                : "Current metrics are unavailable for this resource while the cluster is using Kubernetes Metrics Server.";

            if (SelectedTab == null || !Tabs.Contains(SelectedTab))
            {
                _suppressSelectedTabRefresh = true;
                SelectedTab = Tabs.FirstOrDefault();
                _suppressSelectedTabRefresh = false;
            }

            QueueChartRelayout();
            return;
        }

        _metricsServerHistory = null;

        if (_cluster.MetricsServiceType != MetricsServiceType.Prometheus)
        {
            DisposeTabs();
            CurrentMetrics.Clear();
            SelectedTab = null;
            ShowTabs = false;
            ShowCurrentMetrics = false;
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowCurrentMetrics = false;
        ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
        StatusText = s_loadingStatusText;

        var descriptor = await ResourceMetricsCatalog.CreateAsync(_cluster, resource).ConfigureAwait(false);
        if (descriptor == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsVisible = false;
                DisposeTabs();
                CurrentMetrics.Clear();
                SelectedTab = null;
                ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            });
            return;
        }

        var resourceKey = GetResourceKey(resource);
        var selectedRangeSeconds = SelectedTimeRange?.RangeSeconds ?? s_defaultTimeRange.RangeSeconds;
        var shouldInvalidateCache = !string.Equals(_prometheusCacheKey, resourceKey, StringComparison.Ordinal)
            || _prometheusCacheRangeSeconds != selectedRangeSeconds;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (shouldInvalidateCache)
            {
                DisposePrometheusTabData();
            }

            EnsurePrometheusTabShells(descriptor.Tabs);
            CurrentMetrics.Clear();
            ShowTabs = Tabs.Count > 0;
            ShowCurrentMetrics = false;
            ShowTimeRangeSelector = ShowTabs;

            if (SelectedTab == null || !Tabs.Contains(SelectedTab))
            {
                _suppressSelectedTabRefresh = true;
                SelectedTab = Tabs.FirstOrDefault();
                _suppressSelectedTabRefresh = false;
            }

            if (SelectedTab != null && SelectedTab.Panels.Count == 0)
            {
                ShowStatus = true;
                StatusText = s_loadingStatusText;
            }
            else if (ShowTabs)
            {
                ShowStatus = false;
                StatusText = null;
            }
            else
            {
                ShowStatus = true;
                StatusText = descriptor.EmptyState ?? s_noPrometheusMetricsText;
            }

            _prometheusCacheKey = resourceKey;
            _prometheusCacheRangeSeconds = selectedRangeSeconds;
        });

        var activeTab = SelectedTab ?? Tabs.FirstOrDefault();
        if (activeTab == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowTabs = false;
                ShowCurrentMetrics = false;
                ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
                ShowStatus = true;
                StatusText = descriptor.EmptyState ?? s_noPrometheusMetricsText;
            });
            return;
        }

        var tabDefinition = descriptor.Tabs.FirstOrDefault(x => string.Equals(x.Title, activeTab.Title, StringComparison.Ordinal));
        if (tabDefinition == null)
        {
            return;
        }

        var hadExistingContent = activeTab.Panels.Count > 0;
        if (!hadExistingContent)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowStatus = true;
                StatusText = s_loadingStatusText;
            });
        }

        var snapshot = await LoadPrometheusTabSnapshotAsync(tabDefinition).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var tab = Tabs.FirstOrDefault(x => string.Equals(x.Title, tabDefinition.Title, StringComparison.Ordinal));
            if (tab == null)
            {
                return;
            }

            var loadedSnapshot = snapshot.Snapshot;
            tab.Icon = tabDefinition.Icon;
            tab.MergePanels(loadedSnapshot?.Panels ?? [], hadExistingContent && snapshot.HadRequestFailures);

            ShowTabs = Tabs.Count > 0;
            ShowCurrentMetrics = false;
            ShowTimeRangeSelector = ShowTabs;

            if (tab.Panels.Count > 0)
            {
                ShowStatus = false;
                StatusText = null;
                if (SelectedTab == null || !Tabs.Contains(SelectedTab))
                {
                    _suppressSelectedTabRefresh = true;
                    SelectedTab = tab;
                    _suppressSelectedTabRefresh = false;
                }

                QueueChartRelayout();
                return;
            }

            if (snapshot.HadRequestFailures && !hadExistingContent)
            {
                ShowStatus = true;
                StatusText = "Unable to load metrics.";
                return;
            }

            ShowStatus = true;
            StatusText = s_noPrometheusMetricsText;
            QueueChartRelayout();
        });
    }

    private async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPrometheusTabSnapshotAsync(MetricTabDefinition tabDefinition)
    {
        var panelTasks = new Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)>[tabDefinition.Panels.Count];

        for (var i = 0; i < tabDefinition.Panels.Count; i++)
        {
            panelTasks[i] = LoadPrometheusPanelSnapshotAsync(tabDefinition.Title, tabDefinition.Panels[i]);
        }

        var panelResults = await Task.WhenAll(panelTasks).ConfigureAwait(false);
        var panels = new List<MetricPanelSnapshot>(panelResults.Length);
        var hadRequestFailures = false;

        for (var i = 0; i < panelResults.Length; i++)
        {
            var result = panelResults[i];
            hadRequestFailures |= result.HadFailure;

            if (result.Snapshot != null)
            {
                panels.Add(result.Snapshot);
            }
        }

        return (panels.Count == 0 ? null : new MetricTabSnapshot(tabDefinition.Title, tabDefinition.Icon, panels), hadRequestFailures);
    }

    private async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPrometheusPanelSnapshotAsync(string tabTitle, MetricPanelDefinition panel)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            var result = await _cluster!.RequestMetricsAsync(ApplySelectedTimeRange(panel.Request), timeoutCts.Token).ConfigureAwait(false);
            var series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                _logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for cluster {ClusterName}.", panel.Title, tabTitle, _cluster?.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, _cluster?.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed on cluster {ClusterName}.", panel.Title, tabTitle, _cluster?.Name);
            return (null, true);
        }
    }

private async Task RefreshPodContainerCoreAsync()
{
        if (_cluster == null || Pod == null || Container == null || string.IsNullOrWhiteSpace(Container.Name))
        {
            IsVisible = false;
            DisposeTabs();
            SelectedTab = null;
            ShowTabs = false;
            ShowCurrentMetrics = false;
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            ShowStatus = false;
            StatusText = null;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;
        ShowCurrentMetrics = false;
        CurrentMetrics.Clear();

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            var tabs = CapturePodContainerMetricsServerCharts(Pod, Container.Name);
            MergeTabs(tabs);
            ShowTabs = Tabs.Count > 0;
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs ? null : "Current container metrics are unavailable while the cluster is using Kubernetes Metrics Server.";

            if (SelectedTab == null || !Tabs.Contains(SelectedTab))
            {
                _suppressSelectedTabRefresh = true;
                SelectedTab = Tabs.FirstOrDefault();
                _suppressSelectedTabRefresh = false;
            }

            QueueChartRelayout();
            return;
        }

        _metricsServerHistory = null;

        if (_cluster.MetricsServiceType != MetricsServiceType.Prometheus)
        {
            DisposeTabs();
            SelectedTab = null;
            ShowTabs = false;
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowCurrentMetrics = false;
        ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
        StatusText = s_loadingStatusText;

        var request = CreatePodContainerPrometheusRequest(Pod, Container.Name);
        var resourceKey = GetPodContainerResourceKey(Pod, Container.Name);
        var selectedRangeSeconds = SelectedTimeRange?.RangeSeconds ?? s_defaultTimeRange.RangeSeconds;
        var selectedTitle = SelectedTab?.Title;
        var shouldInvalidateCache = !string.Equals(_prometheusCacheKey, resourceKey, StringComparison.Ordinal)
            || _prometheusCacheRangeSeconds != selectedRangeSeconds;

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (shouldInvalidateCache)
            {
                DisposeTabs();
            }

            EnsurePodContainerPrometheusTabShells(request);
            ShowTabs = Tabs.Count > 0;
            ShowTimeRangeSelector = ShowTabs;
            ShowStatus = !ShowTabs || Tabs.All(x => x.Panels.Count == 0);
            StatusText = ShowTabs
                ? Tabs.All(x => x.Panels.Count == 0)
                    ? s_loadingStatusText
                    : null
                : s_noPrometheusMetricsText;

            _suppressSelectedTabRefresh = true;
            SelectedTab = selectedTitle == null
                ? Tabs.FirstOrDefault()
                : Tabs.FirstOrDefault(x => string.Equals(x.Title, selectedTitle, StringComparison.Ordinal)) ?? Tabs.FirstOrDefault();
            _suppressSelectedTabRefresh = false;

            _prometheusCacheKey = resourceKey;
            _prometheusCacheRangeSeconds = selectedRangeSeconds;
        });

        var activeTab = SelectedTab ?? Tabs.FirstOrDefault();
        if (activeTab == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowTabs = false;
                ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = false;
                ShowStatus = true;
                StatusText = s_noPrometheusMetricsText;
            });
            return;
        }

        var tabDefinition = request.FirstOrDefault(x => string.Equals(x.Title, activeTab.Title, StringComparison.Ordinal));
        if (tabDefinition == null)
        {
            return;
        }

        var hadExistingContent = activeTab.Panels.Count > 0;
        if (!hadExistingContent)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ShowStatus = true;
                StatusText = s_loadingStatusText;
            });
        }

        var snapshot = await LoadPodContainerPrometheusTabSnapshotAsync(tabDefinition).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var tab = Tabs.FirstOrDefault(x => string.Equals(x.Title, tabDefinition.Title, StringComparison.Ordinal));
            if (tab == null)
            {
                return;
            }

            tab.Icon = GetPodContainerTabIcon(tabDefinition.Title);

            tab.MergePanels(snapshot.Snapshot?.Panels ?? [], hadExistingContent && snapshot.HadRequestFailures);
            ShowTabs = Tabs.Count > 0;
            ShowTimeRangeSelector = ShowTabs;
            ShowStatus = false;
            StatusText = null;

            if (tab.Panels.Count == 0)
            {
                ShowStatus = true;
                StatusText = snapshot.HadRequestFailures && !hadExistingContent
                    ? "Unable to load container metrics."
                    : s_noPrometheusMetricsText;
                QueueChartRelayout();
                return;
            }

            QueueChartRelayout();
        });
    }

    private async Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPodContainerPrometheusTabSnapshotAsync(MetricPanelDefinition tabDefinition)
    {
        var panelResult = await LoadPodContainerPrometheusPanelSnapshotAsync(tabDefinition.Title, tabDefinition).ConfigureAwait(false);
        return (panelResult.Snapshot == null
            ? null
            : new MetricTabSnapshot(tabDefinition.Title, GetPodContainerTabIcon(tabDefinition.Title), [panelResult.Snapshot]), panelResult.HadFailure);
    }

    private async Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPodContainerPrometheusPanelSnapshotAsync(string tabTitle, MetricPanelDefinition panel)
    {
        try
        {
            using CancellationTokenSource timeoutCts = new();
            timeoutCts.CancelAfter(s_prometheusPanelTimeout);

            var result = await _cluster!.RequestMetricsAsync(ApplySelectedTimeRange(panel.Request), timeoutCts.Token).ConfigureAwait(false);
            var series = CreateSeries(panel, result);
            if (series.Count == 0)
            {
                _logger.LogDebug("Prometheus metrics panel {PanelTitle} for tab {TabTitle} returned no series for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, Container?.Name, Pod?.Name(), _cluster?.Name);
                return (null, false);
            }

            return (new MetricPanelSnapshot(panel.Title, series), false);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} timed out after {Timeout} for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, s_prometheusPanelTimeout, Container?.Name, Pod?.Name(), _cluster?.Name);
            return (null, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Prometheus metrics panel {PanelTitle} for tab {TabTitle} failed for container {ContainerName} in pod {PodName} on cluster {ClusterName}.", panel.Title, tabTitle, Container?.Name, Pod?.Name(), _cluster?.Name);
            return (null, true);
        }
    }

    private IReadOnlyList<MetricTabSnapshot> CapturePodContainerMetricsServerCharts(V1Pod pod, string containerName)
    {
        var resourceKey = GetPodContainerResourceKey(pod, containerName);
        if (_metricsServerHistory == null
            || !string.Equals(_metricsServerHistory.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            _metricsServerHistory = new MetricsServerHistoryState(resourceKey);
        }

        var metric = _cluster?.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));
        var containerMetric = metric?.Containers?.FirstOrDefault(x => string.Equals(x.Name, containerName, StringComparison.Ordinal));

        if (containerMetric == null)
        {
            return CreateMetricsServerTabs(_metricsServerHistory);
        }

        var timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        _metricsServerHistory.Upsert(timestamp, new MetricsServerSample(containerMetric.Usage["cpu"].ToDecimal(), (double)containerMetric.Usage["memory"].ToInt64()));
        return CreateMetricsServerTabs(_metricsServerHistory);
    }

    private static string GetPodContainerResourceKey(V1Pod pod, string containerName)
    {
        return $"pod:{pod.Namespace()}:{pod.Name()}:{containerName}";
    }

    private static Icon GetPodContainerTabIcon(string title)
    {
        return title switch
        {
            "CPU" => Icon.TopSpeed,
            "Memory" => Icon.Ram,
            "Filesystem" => Icon.HardDrive,
            _ => Icon.TopSpeed,
        };
    }

    private void EnsurePodContainerPrometheusTabShells(IReadOnlyList<MetricPanelDefinition> request)
    {
        for (var i = Tabs.Count - 1; i >= 0; i--)
        {
            if (!request.Any(x => string.Equals(x.Title, Tabs[i].Title, StringComparison.Ordinal)))
            {
                Tabs[i].Dispose();
                Tabs.RemoveAt(i);
            }
        }

        for (var index = 0; index < request.Count; index++)
        {
            var definition = request[index];
            var existing = Tabs.FirstOrDefault(x => string.Equals(x.Title, definition.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricTabViewModel
                {
                    Title = definition.Title,
                    Icon = GetPodContainerTabIcon(definition.Title),
                };
                Tabs.Insert(index, existing);
            }
            else
            {
                existing.Icon = GetPodContainerTabIcon(definition.Title);
                var currentIndex = Tabs.IndexOf(existing);
                if (currentIndex != index)
                {
                    Tabs.Move(currentIndex, index);
                }
            }
        }
    }

    private static MetricPanelDefinition[] CreatePodContainerPrometheusRequest(V1Pod pod, string containerName)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = pod.Namespace(),
            ["pods"] = Regex.Escape(pod.Name()),
            ["selector"] = "container, pod, namespace",
            ["container"] = containerName,
        };

        return
        [
            CreatePanel("CPU", [("cpuUsage", "Usage"), ("cpuRequests", "Requests"), ("cpuLimits", "Limits")]),
            CreatePanel("Memory", [("memoryUsage", "Usage"), ("memoryRequests", "Requests"), ("memoryLimits", "Limits")]),
            CreatePanel("Filesystem", [("fsUsage", "Usage"), ("fsReads", "Reads"), ("fsWrites", "Writes")]),
        ];

        MetricPanelDefinition CreatePanel(string title, IReadOnlyList<(string QueryName, string Label)> queries)
        {
            return new MetricPanelDefinition
            {
                Title = title,
                Filter = static _ => true,
                LegendLabels = queries.ToDictionary(static x => x.QueryName, static x => x.Label, StringComparer.Ordinal),
                Request = new MetricRequest
                {
                    Category = MetricCategory.Pods,
                    StepSeconds = 60,
                    RangeSeconds = 3600,
                    Queries = queries.Select(query => new MetricQueryDefinition
                    {
                        Name = query.QueryName,
                        Options = new Dictionary<string, string>(options, StringComparer.Ordinal),
                    }).ToArray(),
                },
            };
        }
    }

    private void MergeCurrentMetrics(IReadOnlyList<MetricSummaryItemViewModel> snapshots)
    {
        for (var i = CurrentMetrics.Count - 1; i >= 0; i--)
        {
            if (!snapshots.Any(x => string.Equals(x.Key, CurrentMetrics[i].Key, StringComparison.Ordinal)))
            {
                CurrentMetrics.RemoveAt(i);
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = CurrentMetrics.FirstOrDefault(x => string.Equals(x.Key, snapshot.Key, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricSummaryItemViewModel
                {
                    Key = snapshot.Key,
                    Value = snapshot.Value,
                };
                CurrentMetrics.Insert(index, existing);
            }
            else
            {
                existing.Value = snapshot.Value;
                var currentIndex = CurrentMetrics.IndexOf(existing);
                if (currentIndex != index)
                {
                    CurrentMetrics.Move(currentIndex, index);
                }
            }
        }
    }

    private IReadOnlyList<MetricTabSnapshot> TryCaptureMetricsServerCharts(IKubernetesObject<V1ObjectMeta> resource)
    {
        var resourceKey = GetMetricsServerResourceKey(resource);
        if (resourceKey == null)
        {
            _metricsServerHistory = null;
            return [];
        }

        if (_metricsServerHistory == null
            || !string.Equals(_metricsServerHistory.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            _metricsServerHistory = new MetricsServerHistoryState(resourceKey);
        }

        var sample = resource switch
        {
            V1Pod pod => TryCreatePodMetricsServerSample(pod),
            V1Node node => TryCreateNodeMetricsServerSample(node),
            _ => null,
        };

        if (sample == null)
        {
            return CreateMetricsServerTabs(_metricsServerHistory);
        }

        var timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        _metricsServerHistory.Upsert(timestamp, sample.Value);
        return CreateMetricsServerTabs(_metricsServerHistory);
    }

    private static string? GetMetricsServerResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => $"pod:{pod.Namespace()}:{pod.Name()}",
            V1Node node => $"node::{node.Name()}",
            _ => null,
        };
    }

    private MetricsServerSample? TryCreatePodMetricsServerSample(V1Pod pod)
    {
        var metric = _cluster?.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));

        if (metric == null || metric.Containers == null || metric.Containers.Count == 0)
        {
            return null;
        }

        var cpu = metric.Containers.Sum(static x => x.Usage["cpu"].ToDecimal());
        var memory = metric.Containers.Sum(static x => (double)x.Usage["memory"].ToInt64());
        return new MetricsServerSample(cpu, memory);
    }

    private MetricsServerSample? TryCreateNodeMetricsServerSample(V1Node node)
    {
        var metric = _cluster?.NodeMetrics.FirstOrDefault(x => string.Equals(x.Name(), node.Name(), StringComparison.Ordinal));
        if (metric == null)
        {
            return null;
        }

        var cpu = metric.Usage["cpu"].ToDecimal();
        var memory = (double)metric.Usage["memory"].ToInt64();
        return new MetricsServerSample(cpu, memory);
    }

    private static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        var seconds = (long)(timestamp.ToUnixTimeSeconds() / s_metricsServerSampleInterval.TotalSeconds * s_metricsServerSampleInterval.TotalSeconds);
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(MetricsServerHistoryState history)
    {
        if (history.Samples.Count == 0)
        {
            return [];
        }

        var cpuPoints = CreateMetricsServerChartPoints(history.Samples, static x => (double)x.Cpu);
        var memoryPoints = CreateMetricsServerChartPoints(history.Samples, static x => x.Memory);

        return
        [
            new MetricTabSnapshot("CPU", Icon.TopSpeed,
            [
                new MetricPanelSnapshot("CPU",
                [
                    new MetricSeriesSnapshot(
                        "Usage",
                        cpuPoints),
                ]),
            ]),
            new MetricTabSnapshot("Memory", Icon.Ram,
            [
                new MetricPanelSnapshot("Memory",
                [
                    new MetricSeriesSnapshot(
                        "Usage",
                        memoryPoints),
                ]),
            ]),
        ];
    }

    private static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<MetricsServerSamplePoint> samples,
        Func<MetricsServerSamplePoint, double> selector)
    {
        if (samples.Count == 1)
        {
            var sample = samples[0];
            var value = selector(sample);
            return
            [
                new DateTimePoint(sample.Timestamp.Subtract(s_metricsServerSampleInterval).LocalDateTime, value),
                new DateTimePoint(sample.Timestamp.LocalDateTime, value),
            ];
        }

        return samples
            .Select(sample => new DateTimePoint(sample.Timestamp.LocalDateTime, selector(sample)))
            .ToArray();
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetrics(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => CreateCurrentMetricsForPods([pod.Name()], pod.Namespace()),
            V1Node node => CreateCurrentMetricsForNode(node.Name()),
            V1Namespace ns => CreateCurrentMetricsForPods(null, ns.Name()),
            V1Deployment deployment => CreateCurrentMetricsForSelector(deployment.Spec?.Selector?.MatchLabels, deployment.Namespace()),
            V1StatefulSet statefulSet => CreateCurrentMetricsForSelector(statefulSet.Spec?.Selector?.MatchLabels, statefulSet.Namespace()),
            V1DaemonSet daemonSet => CreateCurrentMetricsForSelector(daemonSet.Spec?.Selector?.MatchLabels, daemonSet.Namespace()),
            V1ReplicaSet replicaSet => CreateCurrentMetricsForSelector(replicaSet.Spec?.Selector?.MatchLabels, replicaSet.Namespace()),
            V1Job job => CreateCurrentMetricsForSelector(job.Spec?.Selector?.MatchLabels, job.Namespace()),
            _ => [],
        };
    }

    private MetricRequest ApplySelectedTimeRange(MetricRequest request)
    {
        var selectedRangeSeconds = SelectedTimeRange?.RangeSeconds ?? request.RangeSeconds;
        if (request.RangeSeconds == selectedRangeSeconds)
        {
            return request;
        }

        return new MetricRequest
        {
            Category = request.Category,
            Queries = request.Queries,
            Start = request.Start,
            End = request.End,
            StepSeconds = request.StepSeconds,
            RangeSeconds = selectedRangeSeconds,
            Frames = request.Frames,
        };
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForNode(string? nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return [];
        }

        var metric = _cluster?.NodeMetrics.FirstOrDefault(x => x.Name() == nodeName);
        if (metric == null)
        {
            return [];
        }

        return
        [
            new MetricSummaryItemViewModel { Key = "CPU", Value = $"{metric.Usage["cpu"].ToDecimal():F3}c" },
            new MetricSummaryItemViewModel { Key = "Memory", Value = metric.Usage["memory"].ToInt64().Bytes().Humanize() },
        ];
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForSelector(IDictionary<string, string>? selector, string? @namespace)
    {
        if (selector == null || selector.Count == 0 || string.IsNullOrWhiteSpace(@namespace))
        {
            return [];
        }

        var podNames = _cluster?.GetResourceList<V1Pod>()
            .Where(x => x.Namespace() == @namespace && MatchesSelector(x.Metadata?.Labels, selector))
            .Select(x => x.Name())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (podNames == null || podNames.Length == 0)
        {
            return [];
        }

        return CreateCurrentMetricsForPods(podNames, @namespace);
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForPods(IReadOnlyCollection<string>? podNames, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            return [];
        }

        var podMetrics = (_cluster?.PodMetrics ?? [])
            .Where(x => x.Namespace() == @namespace
                && (podNames == null || podNames.Count == 0 || podNames.Contains(x.Name(), StringComparer.Ordinal)))
            .ToList();

        if (podMetrics.Count == 0)
        {
            return [];
        }

        decimal cpu = 0;
        long memoryBytes = 0;
        var containerCount = 0;

        foreach (var metric in podMetrics)
        {
            foreach (var container in metric.Containers)
            {
                containerCount++;
                cpu += container.Usage["cpu"].ToDecimal();
                memoryBytes += container.Usage["memory"].ToInt64();
            }
        }

        return
        [
            new MetricSummaryItemViewModel { Key = "Pods", Value = podMetrics.Count.ToString() },
            new MetricSummaryItemViewModel { Key = "Containers", Value = containerCount.ToString() },
            new MetricSummaryItemViewModel { Key = "CPU", Value = $"{cpu:F3}c" },
            new MetricSummaryItemViewModel { Key = "Memory", Value = memoryBytes.Bytes().Humanize() },
        ];
    }

    private static bool MatchesSelector(IDictionary<string, string>? labels, IDictionary<string, string> selector)
    {
        if (labels == null || selector.Count == 0)
        {
            return false;
        }

        foreach (var pair in selector)
        {
            if (!labels.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string GetResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => $"pod:{pod.Namespace()}:{pod.Name()}",
            V1Node node => $"node::{node.Name()}",
            V1Namespace ns => $"namespace::{ns.Name()}",
            _ => $"{resource.GetType().FullName}:{resource.Namespace()}:{resource.Name()}",
        };
    }

    private void DisposePrometheusTabData()
    {
        foreach (var tab in Tabs)
        {
            tab.Dispose();
        }
    }

    private void EnsurePrometheusTabShells(IReadOnlyList<MetricTabDefinition> definitions)
    {
        for (var i = Tabs.Count - 1; i >= 0; i--)
        {
            if (!definitions.Any(x => string.Equals(x.Title, Tabs[i].Title, StringComparison.Ordinal)))
            {
                Tabs[i].Dispose();
                Tabs.RemoveAt(i);
            }
        }

        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            var existing = Tabs.FirstOrDefault(x => string.Equals(x.Title, definition.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricTabViewModel
                {
                    Title = definition.Title,
                    Icon = definition.Icon,
                };
                Tabs.Insert(index, existing);
            }
            else
            {
                existing.Icon = definition.Icon;
                var currentIndex = Tabs.IndexOf(existing);
                if (currentIndex != index)
                {
                    Tabs.Move(currentIndex, index);
                }
            }
        }
    }

    private void MergeTabs(IReadOnlyList<MetricTabSnapshot> snapshots, bool preserveMissing = false)
    {
        var selectedTitle = SelectedTab?.Title;

        if (!preserveMissing)
        {
            for (var i = Tabs.Count - 1; i >= 0; i--)
            {
                if (!snapshots.Any(x => string.Equals(x.Title, Tabs[i].Title, StringComparison.Ordinal)))
                {
                    Tabs[i].Dispose();
                    Tabs.RemoveAt(i);
                }
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Tabs.FirstOrDefault(x => string.Equals(x.Title, snapshot.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricTabViewModel
                {
                    Title = snapshot.Title,
                    Icon = snapshot.Icon,
                };
                Tabs.Insert(index, existing);
            }
            else
            {
                existing.Icon = snapshot.Icon;
                var currentIndex = Tabs.IndexOf(existing);
                if (currentIndex != index)
                {
                    Tabs.Move(currentIndex, index);
                }
            }

            existing.MergePanels(snapshot.Panels, preserveMissing);
        }

        SelectedTab = selectedTitle == null
            ? Tabs.FirstOrDefault()
            : Tabs.FirstOrDefault(x => string.Equals(x.Title, selectedTitle, StringComparison.Ordinal)) ?? Tabs.FirstOrDefault();
    }

    private void DisposeTabs()
    {
        foreach (var tab in Tabs)
        {
            tab.Dispose();
        }

        Tabs.Clear();
    }

    private static IReadOnlyList<MetricSeriesSnapshot> CreateSeries(MetricPanelDefinition panel, MetricResultSet result)
    {
        if (result.IsEmpty)
        {
            return [];
        }

        var series = new List<MetricSeriesSnapshot>();

        foreach (var query in panel.Request.Queries)
        {
            if (!result.Metrics.TryGetValue(query.Name, out var metricSeries))
            {
                continue;
            }

            foreach (var item in metricSeries.Where(panel.Filter))
            {
                var legend = panel.LegendLabels.TryGetValue(query.Name, out var label)
                    ? label
                    : query.Name;

                if (item.Labels.Count > 0 && metricSeries.Count > 1)
                {
                    legend += $" ({item.Labels.Values.FirstOrDefault()})";
                }

                series.Add(new MetricSeriesSnapshot(
                    legend,
                    item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToArray()));
            }
        }

        return series;
    }

private async Task RefreshClusterOverviewCoreAsync()
{
if (_cluster == null)
{
return;
}

        IsVisible = true;
        ShowStatus = true;
        StatusText = s_loadingStatusText;
        ShowTabs = false;
        ShowCurrentMetrics = false;
        ShowTimeRangeSelector = false;
        ShowClusterScopeSelector = true;

        ClusterOverviewSnapshot snapshot = await LoadClusterOverviewSnapshotAsync(_cluster).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ShowStatus = false;
            ShowTabs = false;
            ShowCurrentMetrics = false;
            ShowTimeRangeSelector = false;
            ShowClusterScopeSelector = true;
            StatusText = null;
            CurrentMetrics.Clear();
        });

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ClusterTrendChart.SetSeries(snapshot.TrendSeries);
            ClusterCpuChart.SetSeries(snapshot.CpuSeries, static value => value <= 0 ? "n/a" : $"{value:F2}c", snapshot.CpuLegendSeries);
            ClusterMemoryChart.SetSeries(snapshot.MemorySeries, static value => value <= 0 ? "n/a" : value.Bytes().Humanize(), snapshot.MemoryLegendSeries);
            ClusterPodChart.SetSeries(snapshot.PodSeries, legendSnapshots: snapshot.PodLegendSeries, innerRadius: 34);
        });
    }

    private async Task<ClusterOverviewSnapshot> LoadClusterOverviewSnapshotAsync(ClusterWorkspaceViewModel cluster)
    {
        await cluster.SeedResource<V1Pod>(true).ConfigureAwait(false);
        await cluster.SeedResource<V1Node>(true).ConfigureAwait(false);

        var nodes = GetClusterScopeNodes(SelectedClusterScope?.Scope ?? ClusterScope.All);
        var nodeNames = nodes.Select(static node => node.Name()).Where(static name => !string.IsNullOrWhiteSpace(name)).ToHashSet(StringComparer.Ordinal);
        var pods = GetClusterScopePods(SelectedClusterScope?.Scope ?? ClusterScope.All, nodeNames);

        decimal cpuUsage = 0;
        decimal cpuRequests = 0;
        decimal cpuLimits = 0;
        decimal cpuCapacity = 0;
        decimal cpuAllocatable = 0;
        long memoryUsageBytes = 0;
        long memoryRequestsBytes = 0;
        long memoryLimitsBytes = 0;
        long memoryCapacityBytes = 0;
        long memoryAllocatableBytes = 0;

        foreach (var metric in cluster.NodeMetrics.Where(metric => nodeNames.Count == 0 || nodeNames.Contains(metric.Name())))
        {
            if (metric.Usage.TryGetValue("cpu", out var cpuUsageQuantity))
            {
                cpuUsage += cpuUsageQuantity.ToDecimal();
            }

            if (metric.Usage.TryGetValue("memory", out var memoryUsageQuantity))
            {
                memoryUsageBytes += memoryUsageQuantity.ToInt64();
            }
        }

        foreach (var node in nodes)
        {
            if (node.Status?.Capacity?.TryGetValue("cpu", out var nodeCpuCapacity) == true)
            {
                cpuCapacity += nodeCpuCapacity.ToDecimal();
            }

            if (node.Status?.Allocatable?.TryGetValue("cpu", out var nodeCpuAllocatable) == true)
            {
                cpuAllocatable += nodeCpuAllocatable.ToDecimal();
            }

            if (node.Status?.Capacity?.TryGetValue("memory", out var nodeMemoryCapacity) == true)
            {
                memoryCapacityBytes += nodeMemoryCapacity.ToInt64();
            }

            if (node.Status?.Allocatable?.TryGetValue("memory", out var nodeMemoryAllocatable) == true)
            {
                memoryAllocatableBytes += nodeMemoryAllocatable.ToInt64();
            }
        }

        foreach (var container in pods.SelectMany(static pod => pod.Spec?.Containers ?? []))
        {
            if (container.Resources?.Requests?.TryGetValue("cpu", out var cpuRequest) == true)
            {
                cpuRequests += cpuRequest.ToDecimal();
            }

            if (container.Resources?.Limits?.TryGetValue("cpu", out var cpuLimit) == true)
            {
                cpuLimits += cpuLimit.ToDecimal();
            }

            if (container.Resources?.Requests?.TryGetValue("memory", out var memoryRequest) == true)
            {
                memoryRequestsBytes += memoryRequest.ToInt64();
            }

            if (container.Resources?.Limits?.TryGetValue("memory", out var memoryLimit) == true)
            {
                memoryLimitsBytes += memoryLimit.ToInt64();
            }
        }

        string trendTitle = SelectedClusterOverviewMode?.Title ?? "CPU";
        IReadOnlyList<ClusterOverviewMetricSeries> trendSeries = [];
        IReadOnlyList<ClusterOverviewMetricSeries> cpuUsageSeries = [];
        IReadOnlyList<ClusterOverviewMetricSeries> memoryUsageSeries = [];
        if (cluster.MetricsServiceType == MetricsServiceType.Prometheus)
        {
            trendSeries = trendTitle switch
            {
                "Memory" => await LoadClusterRingChartAsync(cluster, "Memory Trend", ("workloadMemoryUsage", "Usage")).ConfigureAwait(false),
                "Pods" => await LoadClusterRingChartAsync(cluster, "Pods Trend", ("podUsage", "Usage")).ConfigureAwait(false),
                _ => await LoadClusterRingChartAsync(cluster, "CPU Trend", ("cpuUsage", "Usage")).ConfigureAwait(false),
            };

            cpuUsageSeries = await LoadClusterRingChartAsync(cluster, "CPU Usage", ("cpuUsage", "Usage")).ConfigureAwait(false);
            memoryUsageSeries = await LoadClusterRingChartAsync(cluster, "Memory Usage", ("workloadMemoryUsage", "Usage")).ConfigureAwait(false);
        }

        decimal effectiveCpuUsage = cpuUsage;
        long effectiveMemoryUsageBytes = memoryUsageBytes;

        if (cluster.MetricsServiceType == MetricsServiceType.Prometheus)
        {
            double latestCpuUsage = GetLatestSeriesValue(cpuUsageSeries, "Usage");
            if (latestCpuUsage > 0)
            {
                effectiveCpuUsage = (decimal)latestCpuUsage;
            }

            double latestMemoryUsage = GetLatestSeriesValue(memoryUsageSeries, "Usage");
            if (latestMemoryUsage > 0)
            {
                effectiveMemoryUsageBytes = (long)Math.Round(latestMemoryUsage);
            }
        }

        double cpuUsageDisplay = (double)effectiveCpuUsage;
        double memoryUsageDisplay = effectiveMemoryUsageBytes;
        double cpuFrame = cpuAllocatable > 0 ? (double)cpuAllocatable : (double)cpuCapacity;
        double memoryFrame = memoryAllocatableBytes > 0 ? memoryAllocatableBytes : memoryCapacityBytes;
        decimal podCapacity = SumNodePodCapacity(nodes);
        decimal podAllocatableCapacity = SumNodePodAllocatableCapacity(nodes);
        double podFrame = podAllocatableCapacity > 0 ? (double)podAllocatableCapacity : (double)podCapacity;

        return new ClusterOverviewSnapshot(
            effectiveCpuUsage,
            cpuRequests,
            cpuLimits,
            cpuCapacity,
            cpuAllocatable,
            effectiveMemoryUsageBytes,
            memoryRequestsBytes,
            memoryLimitsBytes,
            memoryCapacityBytes,
            memoryAllocatableBytes,
            pods.Count,
            podCapacity,
            podAllocatableCapacity,
            trendSeries,
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame(cpuUsageDisplay, cpuFrame)),
                ("Requests", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuRequests, cpuFrame)),
                ("Limits", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuLimits, cpuFrame))),
            CreateClusterSeries(
                ("Usage", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryUsageDisplay, memoryFrame)),
                ("Requests", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryRequestsBytes, memoryFrame)),
                ("Limits", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryLimitsBytes, memoryFrame))),
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)pods.Count, podFrame))),
            CreateCpuLegendSeries((decimal)cpuUsageDisplay, cpuRequests, cpuLimits, cpuAllocatable, cpuCapacity),
            CreateMemoryLegendSeries(memoryUsageDisplay, memoryRequestsBytes, memoryLimitsBytes, memoryAllocatableBytes, memoryCapacityBytes),
            CreatePodLegendSeries(pods.Count, podAllocatableCapacity, podCapacity));
    }

    private static double GetLatestSeriesValue(IReadOnlyList<ClusterOverviewMetricSeries> series, string name)
    {
        for (var i = 0; i < series.Count; i++)
        {
            if (!string.Equals(series[i].Name, name, StringComparison.Ordinal) || series[i].Points.Count == 0)
            {
                continue;
            }

            double? value = series[i].Points[^1].Value;
            if (value.HasValue && value.Value > 0)
            {
                return value.Value;
            }
        }

        return 0;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateClusterSeries(params (string Name, decimal Value)[] items)
    {
        List<ClusterOverviewMetricSeries> series = new(items.Length);
        DateTime now = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Value <= 0)
            {
                continue;
            }

            series.Add(new ClusterOverviewMetricSeries(item.Name, [new DateTimePoint(now, (double)item.Value)]));
        }

        return series;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateClusterSeries(params (string Name, long Value)[] items)
    {
        List<ClusterOverviewMetricSeries> series = new(items.Length);
        DateTime now = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Value <= 0)
            {
                continue;
            }

            series.Add(new ClusterOverviewMetricSeries(item.Name, [new DateTimePoint(now, item.Value)]));
        }

        return series;
    }

    private async Task<IReadOnlyList<ClusterOverviewMetricSeries>> LoadClusterRingChartAsync(
        ClusterWorkspaceViewModel cluster,
        string title,
        params (string QueryName, string Label)[] queries)
    {
        try
        {
            var nodeMatcher = string.Join(
                "|",
                GetClusterScopeNodes(SelectedClusterScope?.Scope ?? ClusterScope.All)
                    .Select(static node => node.Name())
                    .Where(static name => !string.IsNullOrWhiteSpace(name))
                    .Select(static name => Regex.Escape(name!)));

            var request = new MetricRequest
            {
                Category = MetricCategory.Cluster,
                RangeSeconds = SelectedTimeRange?.RangeSeconds ?? s_defaultTimeRange.RangeSeconds,
                StepSeconds = 60,
                Queries = queries.Select(query => new MetricQueryDefinition
                {
                    Name = query.QueryName,
                    Options = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["nodes"] = string.IsNullOrWhiteSpace(nodeMatcher) ? ".*" : nodeMatcher,
                    },
                }).ToArray(),
            };

            var result = await cluster.RequestMetricsAsync(request).ConfigureAwait(false);
            if (result.IsEmpty)
            {
                return [];
            }

            var series = new List<ClusterOverviewMetricSeries>(queries.Length);

            foreach (var query in queries)
            {
                if (!result.Metrics.TryGetValue(query.QueryName, out var metricSeries))
                {
                    continue;
                }

                foreach (var item in metricSeries)
                {
                    var points = item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToArray();
                    if (points.Length == 0)
                    {
                        continue;
                    }

                    series.Add(new ClusterOverviewMetricSeries(query.Label, points));
                }
            }

            return series;
        }
        catch
        {
            return [];
        }
    }

    private static decimal SumNodePodCapacity(IReadOnlyList<V1Node> nodes)
    {
        decimal total = 0;
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Status?.Capacity?.TryGetValue("pods", out var quantity) == true)
            {
                total += quantity.ToDecimal();
            }
        }

        return total;
    }

    private static decimal SumNodePodAllocatableCapacity(IReadOnlyList<V1Node> nodes)
    {
        decimal total = 0;
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Status?.Allocatable?.TryGetValue("pods", out var quantity) == true)
            {
                total += quantity.ToDecimal();
            }
        }

        return total;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreatePodLegendSeries(int podCount, decimal podAllocatableCapacity, decimal podCapacity)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, podCount)]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, podAllocatableCapacity))]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, podCapacity))]),
        ];
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateCpuLegendSeries(decimal cpuUsage, decimal cpuRequests, decimal cpuLimits, decimal cpuAllocatable, decimal cpuCapacity)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, (double)Math.Max(0, cpuUsage))]),
            new ClusterOverviewMetricSeries("Requests", [new DateTimePoint(now, (double)Math.Max(0, cpuRequests))]),
            new ClusterOverviewMetricSeries("Limits", [new DateTimePoint(now, (double)Math.Max(0, cpuLimits))]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuAllocatable))]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuCapacity))]),
        ];
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateMemoryLegendSeries(double memoryUsageBytes, long memoryRequestsBytes, long memoryLimitsBytes, long memoryAllocatableBytes, long memoryCapacityBytes)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, memoryUsageBytes)]),
            new ClusterOverviewMetricSeries("Requests", [new DateTimePoint(now, memoryRequestsBytes)]),
            new ClusterOverviewMetricSeries("Limits", [new DateTimePoint(now, memoryLimitsBytes)]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, memoryAllocatableBytes)]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, memoryCapacityBytes)]),
        ];
    }

    private sealed record ClusterOverviewSnapshot(
        decimal CpuUsage,
        decimal CpuRequests,
        decimal CpuLimits,
        decimal CpuCapacity,
        decimal CpuAllocatable,
        long MemoryUsageBytes,
        long MemoryRequestsBytes,
        long MemoryLimitsBytes,
        long MemoryCapacityBytes,
        long MemoryAllocatableBytes,
        int PodCount,
        decimal PodCapacity,
        decimal PodAllocatableCapacity,
        IReadOnlyList<ClusterOverviewMetricSeries> TrendSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> CpuSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> MemorySeries,
        IReadOnlyList<ClusterOverviewMetricSeries> PodSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> CpuLegendSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> MemoryLegendSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> PodLegendSeries);

    private void SyncClusterOverviewTabSelection()
    {
        for (var i = 0; i < ClusterOverviewModes.Count; i++)
        {
            ClusterOverviewModes[i].IsSelected = ReferenceEquals(ClusterOverviewModes[i], SelectedClusterOverviewMode);
        }
    }

    private IReadOnlyList<V1Node> GetClusterScopeNodes(ClusterScope scope)
    {
        var nodes = _cluster?.GetResourceList<V1Node>() ?? [];
        return scope switch
        {
            ClusterScope.MasterNodes => nodes.Where(static node => IsMasterNode(node)).ToArray(),
            ClusterScope.WorkerNodes => nodes.Where(static node => !IsMasterNode(node)).ToArray(),
            _ => nodes,
        };
    }

    private IReadOnlyList<V1Pod> GetClusterScopePods(ClusterScope scope, ISet<string> nodeNames)
    {
        var pods = _cluster?.GetResourceList<V1Pod>() ?? [];
        if (scope == ClusterScope.All || nodeNames.Count == 0)
        {
            return pods;
        }

        return pods.Where(pod => !string.IsNullOrWhiteSpace(pod.Spec?.NodeName) && nodeNames.Contains(pod.Spec!.NodeName!)).ToArray();
    }

    private static bool IsMasterNode(V1Node node)
    {
        var labels = node.Metadata?.Labels;
        if (labels == null)
        {
            return false;
        }

        return labels.ContainsKey("node-role.kubernetes.io/master")
            || labels.ContainsKey("node-role.kubernetes.io/control-plane");
    }

}

public sealed class ClusterScopeOption
{
    public ClusterScopeOption(string label, ClusterScope scope)
    {
        Label = label;
        Scope = scope;
    }

    public string Label { get; }

    public ClusterScope Scope { get; }

    public override string ToString() => Label;
}

public enum ClusterScope
{
    All,
    WorkerNodes,
    MasterNodes,
}

public sealed partial class MetricTabViewModel : ObservableObject, IDisposable
{
    public required string Title { get; init; }

    [ObservableProperty]
    public partial Icon Icon { get; set; }

    [ObservableProperty]
    public partial bool ShowTitle { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ObservableCollection<MetricPanelViewModel> Panels { get; } = [];

    internal void MergePanels(IReadOnlyList<MetricPanelSnapshot> snapshots, bool preserveMissing = false)
    {
        if (!preserveMissing)
        {
            for (var i = Panels.Count - 1; i >= 0; i--)
            {
                if (!snapshots.Any(x => string.Equals(x.Title, Panels[i].Title, StringComparison.Ordinal)))
                {
                    Panels[i].Dispose();
                    Panels.RemoveAt(i);
                }
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Panels.FirstOrDefault(x => string.Equals(x.Title, snapshot.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricPanelViewModel
                {
                    Title = snapshot.Title,
                };
                Panels.Insert(index, existing);
            }

            existing.MergeSeries(snapshot.Series, preserveMissing);

            if (existing != null)
            {
                var currentIndex = Panels.IndexOf(existing);
                if (currentIndex != index)
                {
                    Panels.Move(currentIndex, index);
                }
            }
        }

        ShowTitle = snapshots.Count != 1
            || !string.Equals(Title, snapshots[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        foreach (var panel in Panels)
        {
            panel.Dispose();
        }

        Panels.Clear();
    }
}

public sealed partial class MetricPanelViewModel : ObservableObject, IDisposable
{
    private static readonly TimeSpan s_defaultTimeWindow = TimeSpan.FromHours(1);
    private const float s_lineStrokeThickness = 2f;
    private readonly Axis _yAxis = new()
    {
        LabelsPaint = CreateChartTextPaint(),
        TextSize = 11,
    };
    private readonly Axis _xAxis;

    public required string Title { get; init; }

    public LabelVisual ChartTitle { get; }

    public ObservableCollection<ISeries> Series { get; } = [];

    public ICartesianAxis[] XAxes { get; }

    public ICartesianAxis[] YAxes { get; } =
    [];

    public MetricPanelViewModel()
    {
        ChartTitle = new LabelVisual
        {
            HorizontalAlignment = Align.Start,
            VerticalAlignment = Align.Start,
            TextSize = 11,
            Paint = CreateChartTextPaint(),
        };

        _xAxis = new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm"))
        {
            LabelsPaint = CreateChartTextPaint(),
            TextSize = 11,
        };
        XAxes = [_xAxis];
        YAxes = [_yAxis];
    }

    internal void MergeSeries(IReadOnlyList<MetricSeriesSnapshot> snapshots, bool preserveMissing = false)
    {
        SyncChartTitle();

        if (!preserveMissing)
        {
            for (var i = Series.Count - 1; i >= 0; i--)
            {
                if (Series[i] is not LineSeries<DateTimePoint> line
                    || !snapshots.Any(x => string.Equals(x.Name, line.Name, StringComparison.Ordinal)))
                {
                    (Series[i] as IDisposable)?.Dispose();
                    Series.RemoveAt(i);
                }
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Series
                .OfType<LineSeries<DateTimePoint>>()
                .FirstOrDefault(x => string.Equals(x.Name, snapshot.Name, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new LineSeries<DateTimePoint>
                {
                    Name = snapshot.Name,
                    GeometrySize = 0,
                    Values = new ObservableCollection<DateTimePoint>(snapshot.Points),
                };
                ApplyLineSeriesStyle(existing, index);
                Series.Insert(index, existing);
            }
            else
            {
                existing.Values = new ObservableCollection<DateTimePoint>(snapshot.Points);
                ApplyLineSeriesStyle(existing, index);
                var currentIndex = Series.IndexOf(existing);
                if (currentIndex != index)
                {
                    Series.Move(currentIndex, index);
                }
            }
        }

        UpdateYAxisLimits(Series);
        UpdateXAxisLimits(Series);
    }

    private void SyncChartTitle()
    {
        ChartTitle.Text = Title;
    }

    private void UpdateYAxisLimits(IEnumerable<ISeries> series)
    {
        var values = series
            .OfType<LineSeries<DateTimePoint>>()
            .SelectMany(x => x.Values?.OfType<DateTimePoint>() ?? [])
            .Where(x => x.Value.HasValue)
            .Select(x => x.Value!.Value)
            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            .ToArray();

        if (values.Length == 0)
        {
            _yAxis.MinLimit = null;
            _yAxis.MaxLimit = null;
            return;
        }

        var min = values.Min();
        var max = values.Max();

        if (Math.Abs(max - min) < double.Epsilon)
        {
            var delta = Math.Max(Math.Abs(max) * 0.1, 0.01);
            _yAxis.MinLimit = min - delta;
            _yAxis.MaxLimit = max + delta;
            return;
        }

        var padding = Math.Max((max - min) * 0.15, Math.Abs(max) * 0.02);
        _yAxis.MinLimit = min - padding;
        _yAxis.MaxLimit = max + padding;
    }

    private void UpdateXAxisLimits(IEnumerable<ISeries> series)
    {
        var timestamps = series
            .OfType<LineSeries<DateTimePoint>>()
            .SelectMany(x => x.Values?.OfType<DateTimePoint>() ?? [])
            .Select(x => x.DateTime)
            .Where(x => x != default)
            .Order()
            .ToArray();

        if (timestamps.Length == 0)
        {
            _xAxis.MinLimit = null;
            _xAxis.MaxLimit = null;
            return;
        }

        var min = timestamps[0];
        var max = timestamps[^1];
        var span = max - min;

        if (span < s_defaultTimeWindow)
        {
            min = max - s_defaultTimeWindow;
        }

        _xAxis.MinLimit = min.Ticks;
        _xAxis.MaxLimit = max.Ticks;
    }

    private static SolidColorPaint CreateChartTextPaint()
    {
        var color = Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? SKColors.Black
            : SKColors.White;

        return new SolidColorPaint(color)
        {
            SKTypeface = SKTypeface.FromFamilyName("Inter"),
        };
    }

    private static void ApplyLineSeriesStyle(LineSeries<DateTimePoint> series, int index)
    {
        var palette = Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? ColorPalletes.MaterialDesign500
            : ColorPalletes.MaterialDesign200;
        var color = palette[index % palette.Length].AsSKColor();

        series.Stroke = new SolidColorPaint(color, s_lineStrokeThickness);
        series.Fill = new SolidColorPaint(color.WithAlpha(50));
    }

    public void Dispose()
    {
        DisposePaint(_xAxis.LabelsPaint);
        DisposePaint(_yAxis.LabelsPaint);

        foreach (var series in Series.OfType<IDisposable>().ToArray())
        {
            series.Dispose();
        }

        Series.Clear();
    }

    private static void DisposePaint(object? paint)
    {
        if (paint is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

}

public sealed partial class MetricSummaryItemViewModel : ObservableObject
{
    public required string Key { get; init; }

    [ObservableProperty]
    public partial string Value { get; set; } = string.Empty;
}

internal sealed record MetricTabSnapshot(string Title, Icon Icon, IReadOnlyList<MetricPanelSnapshot> Panels);

public sealed record MetricTimeRangeOption(string Label, int RangeSeconds);

internal sealed record MetricPanelSnapshot(string Title, IReadOnlyList<MetricSeriesSnapshot> Series);

internal sealed record MetricSeriesSnapshot(string Name, IReadOnlyList<DateTimePoint> Points);

internal sealed class MetricsServerHistoryState(string resourceKey)
{
    private static readonly TimeSpan s_historyDuration = TimeSpan.FromHours(1);

    public string ResourceKey { get; } = resourceKey;

    public List<MetricsServerSamplePoint> Samples { get; } = [];

    public void Upsert(DateTimeOffset timestamp, MetricsServerSample sample)
    {
        var cutoff = timestamp - s_historyDuration;
        Samples.RemoveAll(x => x.Timestamp < cutoff);

        var existing = Samples.FindIndex(x => x.Timestamp == timestamp);
        var point = new MetricsServerSamplePoint(timestamp, sample.Cpu, sample.Memory);

        if (existing >= 0)
        {
            Samples[existing] = point;
            return;
        }

        Samples.Add(point);
        Samples.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));
    }
}

internal readonly record struct MetricsServerSample(decimal Cpu, double Memory);

internal readonly record struct MetricsServerSamplePoint(DateTimeOffset Timestamp, decimal Cpu, double Memory);

