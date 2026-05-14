using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
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
using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Themes;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

public sealed partial class MetricsControl : UserControl, IInitializeCluster
{
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
    private bool _suppressRefreshQueue;
    private string? StatusText;
    private bool ShowStatus;
    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricTabViewModel> Tabs { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool ShowTabs { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricSummaryItemViewModel> CurrentMetrics { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool ShowCurrentMetrics { get; set; }

    [GeneratedDirectProperty]
    public partial MetricTabViewModel? SelectedTab { get; set; }

    [GeneratedDirectProperty]
    public partial MetricPanelViewModel? SelectedPanel { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowTimeRangeSelector { get; set; }

    public static readonly StyledProperty<V1Pod?> PodProperty =
        AvaloniaProperty.Register<MetricsControl, V1Pod?>(nameof(Pod));

    public static readonly StyledProperty<V1Container?> ContainerProperty =
        AvaloniaProperty.Register<MetricsControl, V1Container?>(nameof(Container));

    public static readonly StyledProperty<MetricTimeRangeOption?> SelectedTimeRangeProperty =
        AvaloniaProperty.Register<MetricsControl, MetricTimeRangeOption?>(nameof(SelectedTimeRange));

    static MetricsControl()
    {
        PodProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        ContainerProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        SelectedTimeRangeProperty.Changed.AddClassHandler<MetricsControl>((control, _) => control.QueueRefresh());
        BoundsProperty.Changed.AddClassHandler<MetricsControl>((control, args) =>
        {
            if (args.NewValue is Rect bounds && bounds.Width > 0 && bounds.Height > 0)
            {
                control.QueueChartRelayout();
            }
        });
        SelectedTabProperty.Changed.AddClassHandler<MetricsControl>((control, _) =>
        {
            control.SyncTabSelection();
            control.SelectedPanel = control.SelectedTab?.Panels.FirstOrDefault();
            if (!control._suppressSelectedTabRefresh)
            {
                control.QueueRefresh();
            }
        });
    }

    public IReadOnlyList<MetricTimeRangeOption> TimeRangeOptions => s_timeRangeOptions;

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

    public MetricsControl()
    {
        InitializeComponent();
        _logger = Application.Current is IServiceProviderHost host
            ? host.Services.GetRequiredService<ILogger<MetricsControl>>()
            : Microsoft.Extensions.Logging.Abstractions.NullLogger<MetricsControl>.Instance;
        SelectedTimeRange = s_defaultTimeRange;

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(30);
            s_timer.Start();
        }
    }

    private void MetricTab_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton || toggleButton.IsChecked != true || toggleButton.Tag is not MetricTabViewModel tab)
        {
            return;
        }

        SelectedTab = tab;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        _suppressRefreshQueue = true;

        UnsubscribeCluster();
        _cluster = cluster;
        SubscribeCluster();

        _suppressRefreshQueue = false;
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
        if (_cluster != null || (Pod != null && Container != null))
        {
            QueueRefresh();
        }
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        if (_cluster != null || (Pod != null && Container != null))
        {
            QueueRefresh();
        }
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
        if (_cluster != null)
        {
            QueueRefresh();
        }
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind)
            or nameof(ClusterWorkspaceViewModel.IsMetricsAvailable))
        {
            if (_cluster != null)
            {
                QueueRefresh();
            }
        }
    }

    private void QueueRefresh()
    {
        if (_suppressRefreshQueue || (_cluster == null && (Pod == null || Container == null)))
        {
            return;
        }

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

        if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
        {
            IsVisible = false;
            DisposeTabs();
            CurrentMetrics.Clear();
            SelectedTab = null;
            ShowTimeRangeSelector = false;
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
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowCurrentMetrics = false;
        ShowTimeRangeSelector = false;
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

    private Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPrometheusTabSnapshotAsync(MetricTabDefinition tabDefinition)
    {
        return MetricsControlPrometheusBackend.LoadPrometheusTabSnapshotAsync(_cluster!, tabDefinition, SelectedTimeRange, _logger);
    }

    private Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPrometheusPanelSnapshotAsync(string tabTitle, MetricPanelDefinition panel)
    {
        return MetricsControlPrometheusBackend.LoadPrometheusPanelSnapshotAsync(_cluster!, tabTitle, panel, SelectedTimeRange, _logger);
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
            var capture = MetricsControlMetricsServerBackend.CapturePodContainerMetricsServerCharts(_cluster, Pod, Container.Name, _metricsServerHistory);
            _metricsServerHistory = capture.History;
            var tabs = capture.Tabs;
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
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowCurrentMetrics = false;
        ShowTimeRangeSelector = false;
        StatusText = s_loadingStatusText;

        var request = MetricsControlPrometheusBackend.CreatePodContainerPrometheusRequest(Pod, Container.Name);
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

    private Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPodContainerPrometheusTabSnapshotAsync(MetricPanelDefinition tabDefinition)
    {
        return MetricsControlPrometheusBackend.LoadPodContainerPrometheusTabSnapshotAsync(_cluster!, tabDefinition, Pod, Container, SelectedTimeRange, _logger);
    }

    private Task<(MetricPanelSnapshot? Snapshot, bool HadFailure)> LoadPodContainerPrometheusPanelSnapshotAsync(string tabTitle, MetricPanelDefinition panel)
    {
        return MetricsControlPrometheusBackend.LoadPodContainerPrometheusPanelSnapshotAsync(_cluster!, tabTitle, panel, Pod, Container, SelectedTimeRange, _logger);
    }

    private IReadOnlyList<MetricTabSnapshot> CapturePodContainerMetricsServerCharts(V1Pod pod, string containerName)
    {
        var capture = MetricsControlMetricsServerBackend.CapturePodContainerMetricsServerCharts(_cluster!, pod, containerName, _metricsServerHistory);
        _metricsServerHistory = capture.History;
        return capture.Tabs;
    }

    private static string GetPodContainerResourceKey(V1Pod pod, string containerName)
    {
        return $"pod:{pod.Namespace()}:{pod.Name()}:{containerName}";
    }

    private static Icon GetPodContainerTabIcon(string title)
    {
        return MetricsControlPrometheusBackend.GetPodContainerTabIcon(title);
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
        return MetricsControlPrometheusBackend.CreatePodContainerPrometheusRequest(pod, containerName);
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
        var capture = MetricsControlMetricsServerBackend.TryCaptureMetricsServerCharts(_cluster!, resource, _metricsServerHistory);
        _metricsServerHistory = capture.History;
        return capture.Tabs;
    }

    private static string? GetMetricsServerResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return MetricsControlMetricsServerBackend.GetMetricsServerResourceKey(resource);
    }

    private MetricsServerSample? TryCreatePodMetricsServerSample(V1Pod pod)
    {
        return MetricsControlMetricsServerBackend.TryCreatePodMetricsServerSample(_cluster!, pod);
    }

    private MetricsServerSample? TryCreateNodeMetricsServerSample(V1Node node)
    {
        return MetricsControlMetricsServerBackend.TryCreateNodeMetricsServerSample(_cluster!, node);
    }

    private static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        return MetricsControlMetricsServerBackend.NormalizeMetricsServerTimestamp(timestamp);
    }

    private static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(MetricsServerHistoryState history)
    {
        return MetricsControlMetricsServerBackend.CreateMetricsServerTabs(history);
    }

    private static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<MetricsServerSamplePoint> samples,
        Func<MetricsServerSamplePoint, double> selector)
    {
        return MetricsControlMetricsServerBackend.CreateMetricsServerChartPoints(samples, selector);
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
        return MetricsControlPrometheusBackend.ApplySelectedTimeRange(request, SelectedTimeRange);
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
        return MetricsControlPrometheusBackend.CreateSeries(panel, result);
    }

    private void SyncTabSelection()
    {
        for (var i = 0; i < Tabs.Count; i++)
        {
            Tabs[i].IsSelected = ReferenceEquals(Tabs[i], SelectedTab);
        }
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

    public XamlDrawnLabelVisual ChartTitle { get; }

    public ObservableCollection<ISeries> Series { get; } = [];

    public ICartesianAxis[] XAxes { get; }

    public ICartesianAxis[] YAxes { get; } =
    [];

    public MetricPanelViewModel()
    {
        ChartTitle = new XamlDrawnLabelVisual
        {
            HorizontalAlign = LiveChartsCore.Drawing.Align.Start,
            VerticalAlign = LiveChartsCore.Drawing.Align.Start,
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
