using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Humanizer;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics;
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

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Metric tabs are owned and disposed by MetricsControl.")]
public sealed partial class MetricsControl : UserControl, IInitializeCluster, INotifyPropertyChanged, IDisposable
{
    private static string LoadingStatusText => Assets.Resources.MetricsControl_Loading!;
    private static string NoMetricsStatusText => Assets.Resources.MetricsControl_NoMetrics!;
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Default);
    private static readonly IReadOnlyList<MetricTimeRangeOption> s_timeRangeOptions =
    [
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_1Hour!, 3600),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_2Hours!, 7200),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_4Hours!, 14400),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_24Hours!, 86400),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_48Hours!, 172800),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_1Week!, 604800),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_1Month!, 2592000),
        new MetricTimeRangeOption(Assets.Resources.Metrics_TimeRange_2Months!, 5184000),
    ];
    private static readonly MetricTimeRangeOption s_defaultTimeRange = s_timeRangeOptions[0];
    private ClusterWorkspace? _cluster;
    private MetricsServerHistoryState? _metricsServerHistory;
    private string? _prometheusCacheKey;
    private int? _prometheusCacheRangeSeconds;
    private bool _suppressSelectedTabRefresh;
    private readonly ILogger<MetricsControl> _logger;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private CancellationTokenSource? _lifecycleCancellation;
    private int _refreshPending;
    private bool _suppressRefreshQueue;
    private V1Pod? _pod;
    private V1Container? _container;
    private MetricTimeRangeOption? _selectedTimeRange;
    private bool _showTabs;
    private bool _showStatus;
    private bool _showTimeRangeSelector;
    private string? _statusText;
    private MetricTabViewModel? _selectedTab;
    private MetricPanelViewModel? _selectedPanel;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MetricTabViewModel> Tabs { get; } = [];

    public bool ShowTabs { get => _showTabs; private set => SetProperty(ref _showTabs, value, nameof(ShowTabs)); }

    public bool ShowStatus { get => _showStatus; private set => SetProperty(ref _showStatus, value, nameof(ShowStatus)); }

    public string? StatusText { get => _statusText; private set => SetProperty(ref _statusText, value, nameof(StatusText)); }

    public MetricTabViewModel? SelectedTab
    {
        get => _selectedTab;
        private set
        {
            if (ReferenceEquals(_selectedTab, value))
            {
                return;
            }

            _selectedTab = value;
            OnPropertyChanged(nameof(SelectedTab));
            SyncTabSelection();
            SelectedPanel = _selectedTab?.Panels.FirstOrDefault();
            if (!_suppressSelectedTabRefresh)
            {
                QueueRefresh();
            }
        }
    }

    public MetricPanelViewModel? SelectedPanel
    {
        get => _selectedPanel;
        private set => SetProperty(ref _selectedPanel, value, nameof(SelectedPanel));
    }

    public bool ShowTimeRangeSelector { get => _showTimeRangeSelector; private set => SetProperty(ref _showTimeRangeSelector, value, nameof(ShowTimeRangeSelector)); }

    public ClusterWorkspace? Cluster { get; private set; }

    public IReadOnlyList<MetricTimeRangeOption> TimeRangeOptions => s_timeRangeOptions;

    public MetricTimeRangeOption? SelectedTimeRange
    {
        get => _selectedTimeRange;
        set
        {
            if (ReferenceEquals(_selectedTimeRange, value))
            {
                return;
            }

            _selectedTimeRange = value;
            OnPropertyChanged(nameof(SelectedTimeRange));
            QueueRefresh();
        }
    }

    public V1Pod? Pod
    {
        get => _pod;
        set
        {
            if (ReferenceEquals(_pod, value))
            {
                return;
            }

            _pod = value;
            OnPropertyChanged(nameof(Pod));
            QueueRefresh();
        }
    }

    public V1Container? Container
    {
        get => _container;
        set
        {
            if (ReferenceEquals(_container, value))
            {
                return;
            }

            _container = value;
            OnPropertyChanged(nameof(Container));
            QueueRefresh();
        }
    }

    public MetricsControl()
    {
        Content = Build();
        _logger = Application.Current is IServiceProviderHost host
            ? host.Services.GetRequiredService<ILogger<MetricsControl>>()
            : Microsoft.Extensions.Logging.Abstractions.NullLogger<MetricsControl>.Instance;
        SelectedTimeRange = s_defaultTimeRange;
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += Timer_Tick;
    }

    private Control Build()
    {
        var timeRangeSelector = new ComboBox()
            .Width(132)
            .IsVisible(this, x => x.ShowTimeRangeSelector)
            .ItemsSource(this, x => x.TimeRangeOptions)
            .SelectedItem(this, x => x.SelectedTimeRange, BindingMode.TwoWay)
            .ItemTemplate(new FuncDataTemplate<MetricTimeRangeOption>((option, _) => new TextBlock().Text(option.Label)));

        var tabs = new ItemsControl()
            .ItemsSource(this, x => x.Tabs)
            .ItemsPanel(new ItemsPanelTemplate
            {
                Content = (IServiceProvider? _) => new TemplateResult<Control>(
                    new StackPanel().Orientation(Orientation.Horizontal).Spacing(6),
                    new NameScope())
            })
            .ItemTemplate(new FuncDataTemplate<MetricTabViewModel>((tab, _) =>
                new ToggleButton()
                    .Padding(8, 6)
                    .Command(new RelayCommand(() => SelectedTab = tab))
                    .IsChecked(tab, x => x.IsSelected)
                    .ToolTip_Tip(tab, x => x.Title)
                    .Content(
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .Spacing(6)
                            .Children(
                                new FluentIcon { FontSize = 14, Icon = tab.Icon },
                                new TextBlock().Text(tab, x => x.Title)))));

        var chart = new ResponsiveCartesianChart()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Title(CompiledBinding.Create<MetricsControl, XamlDrawnLabelVisual?>(x => x.SelectedPanel!.ChartTitle))
            .Series(CompiledBinding.Create<MetricsControl, IEnumerable<ISeries>>(x => x.SelectedPanel!.Series))
            .XAxes(CompiledBinding.Create<MetricsControl, ICartesianAxis[]>(x => x.SelectedPanel!.XAxes))
            .YAxes(CompiledBinding.Create<MetricsControl, ICartesianAxis[]>(x => x.SelectedPanel!.YAxes))
            .Behaviors(new Features.Resources.Properties.Behaviors.ChartWheelScrollBehavior());

        return new Grid()
            .Rows("Auto,*")
            .RowSpacing(8)
            .Children(
                new Grid()
                    .Cols("Auto,*")
                    .ColumnSpacing(10)
                    .Children(timeRangeSelector, tabs),
                new Border()
                    .Row(1)
                    .Padding(12)
                    .Classes("overview-card")
                    .IsVisible(this, x => x.ShowTabs)
                    .Child(chart),
                new TextBlock()
                    .Row(1)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(this, x => x.StatusText)
                    .IsVisible(this, x => x.ShowStatus));
    }

    private void SetProperty<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        _suppressRefreshQueue = true;

        UnsubscribeCluster();
        _cluster = cluster;
        Cluster = cluster;
        OnPropertyChanged(nameof(Cluster));
        SubscribeCluster();

        _suppressRefreshQueue = false;
        QueueRefresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lifecycleCancellation?.Cancel();
        _lifecycleCancellation?.Dispose();
        _lifecycleCancellation = new CancellationTokenSource();
        UnsubscribeCluster();
        SubscribeCluster();
        _timer.Start();
        QueueRefresh();
        QueueChartRelayout();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
        _lifecycleCancellation?.Cancel();
        _lifecycleCancellation?.Dispose();
        _lifecycleCancellation = null;
        Interlocked.Exchange(ref _refreshPending, 0);
        UnsubscribeCluster();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
        _lifecycleCancellation?.Cancel();
        _lifecycleCancellation?.Dispose();
        _lifecycleCancellation = null;
        UnsubscribeCluster();
        DisposeTabs();
        _refreshGate.Dispose();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_cluster != null || (Pod != null && Container != null))
        {
            QueueRefresh();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_cluster != null || (Pod != null && Container != null))
        {
            QueueRefresh();
        }
    }

    private void SubscribeCluster()
    {
        if (_cluster?.Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        }

        if (_cluster?.Runtime.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }

        if (_cluster?.Runtime.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }
    }

    private void UnsubscribeCluster()
    {
        if (_cluster?.Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        }

        if (_cluster?.Runtime.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }

        if (_cluster?.Runtime.NodeMetrics is INotifyCollectionChanged nodeMetrics)
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
        if (e.PropertyName is nameof(IClusterRuntime.ActiveMetricsBackend)
            or nameof(IClusterRuntime.IsMetricsAvailable))
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
            var cancellationToken = _lifecycleCancellation?.Token ?? CancellationToken.None;
            while (Interlocked.Exchange(ref _refreshPending, 0) == 1)
            {
                try
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        await RefreshCoreAsync(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () => await RefreshCoreAsync(cancellationToken).ConfigureAwait(false));
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Metrics refresh canceled for cluster {ClusterName}.", _cluster?.Runtime.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error refreshing metrics for cluster {ClusterName}.", _cluster?.Runtime.Name);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (Tabs.Count == 0)
                        {
                            DisposeTabs();
                            SelectedTab = null;
                            ShowTabs = false;
                            ShowTimeRangeSelector = false;
                            ShowStatus = true;
                            StatusText = Assets.Resources.MetricsControl_LoadFailed;
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

    private async Task RefreshCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Dispatcher.UIThread.CheckAccess())
        {
            throw new InvalidOperationException("MetricsControl refresh must run on the UI thread.");
        }

        if (Pod != null && Container != null)
        {
            await RefreshPodContainerCoreAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
        {
            IsVisible = false;
            DisposeTabs();
            SelectedTab = null;
            ShowTimeRangeSelector = false;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;

        if (Pod != null && Container != null)
        {
            await RefreshPodContainerCoreAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (_cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.KubernetesMetricsServer)
        {
            ShowTimeRangeSelector = false;
            var historyTabs = TryCaptureMetricsServerCharts(resource);
            MergeTabs(historyTabs);
            ShowTabs = Tabs.Count > 0;
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs
                ? null
                : Assets.Resources.MetricsControl_MetricsServerUnavailable;

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

        if (_cluster.Runtime.ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            DisposeTabs();
            SelectedTab = null;
            ShowTabs = false;
            ShowTimeRangeSelector = false;
            ShowStatus = true;
            StatusText = Assets.Resources.MetricsControl_Unavailable;
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowTimeRangeSelector = false;
        StatusText = LoadingStatusText;

        var descriptor = await ResourceMetricsCatalog.CreateAsync(_cluster, resource).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (descriptor == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsVisible = false;
                DisposeTabs();
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
            ShowTabs = Tabs.Count > 0;
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
                StatusText = LoadingStatusText;
            }
            else if (ShowTabs)
            {
                ShowStatus = false;
                StatusText = null;
            }
            else
            {
                ShowStatus = true;
                StatusText = descriptor.EmptyState ?? NoMetricsStatusText;
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
                ShowTimeRangeSelector = false;
                ShowStatus = true;
            StatusText = descriptor.EmptyState ?? NoMetricsStatusText;
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
                StatusText = LoadingStatusText;
            });
        }

        var snapshot = await LoadPrometheusTabSnapshotAsync(tabDefinition, cancellationToken).ConfigureAwait(false);
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
                StatusText = Assets.Resources.MetricsControl_LoadFailed;
                return;
            }

            ShowStatus = true;
            StatusText = NoMetricsStatusText;
            QueueChartRelayout();
        });
    }

    private Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPrometheusTabSnapshotAsync(MetricTabDefinition tabDefinition, CancellationToken cancellationToken)
    {
        return MetricsControlPrometheusBackend.LoadPrometheusTabSnapshotAsync(_cluster!, tabDefinition, SelectedTimeRange, _logger, cancellationToken);
    }

    private async Task RefreshPodContainerCoreAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_cluster == null || Pod == null || Container == null || string.IsNullOrWhiteSpace(Container.Name))
        {
            IsVisible = false;
            DisposeTabs();
            SelectedTab = null;
            ShowTabs = false;
            ShowTimeRangeSelector = false;
            ShowStatus = false;
            StatusText = null;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;
        if (_cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.KubernetesMetricsServer)
        {
            ShowTimeRangeSelector = false;
            var capture = MetricsControlMetricsServerBackend.CapturePodContainerMetricsServerCharts(_cluster, Pod, Container.Name, _metricsServerHistory);
            _metricsServerHistory = capture.History;
            var tabs = capture.Tabs;
            MergeTabs(tabs);
            ShowTabs = Tabs.Count > 0;
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs ? null : Assets.Resources.MetricsControl_ContainerMetricsServerUnavailable;

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

        if (_cluster.Runtime.ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            DisposeTabs();
            SelectedTab = null;
            ShowTabs = false;
            ShowTimeRangeSelector = false;
            ShowStatus = true;
            StatusText = Assets.Resources.MetricsControl_Unavailable;
            return;
        }

        ShowStatus = true;
        ShowTabs = false;
        ShowTimeRangeSelector = false;
        StatusText = LoadingStatusText;

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
                    ? LoadingStatusText
                    : null
                : NoMetricsStatusText;

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
                StatusText = NoMetricsStatusText;
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
                StatusText = LoadingStatusText;
            });
        }

        var snapshot = await LoadPodContainerPrometheusTabSnapshotAsync(tabDefinition, cancellationToken).ConfigureAwait(false);
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
                    ? Assets.Resources.MetricsControl_ContainerLoadFailed
                    : NoMetricsStatusText;
                QueueChartRelayout();
                return;
            }

            QueueChartRelayout();
        });
    }

    private Task<(MetricTabSnapshot? Snapshot, bool HadRequestFailures)> LoadPodContainerPrometheusTabSnapshotAsync(MetricPanelDefinition tabDefinition, CancellationToken cancellationToken)
    {
        return MetricsControlPrometheusBackend.LoadPodContainerPrometheusTabSnapshotAsync(_cluster!, tabDefinition, Pod, Container, SelectedTimeRange, _logger, cancellationToken);
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

    private IReadOnlyList<MetricTabSnapshot> TryCaptureMetricsServerCharts(IKubernetesObject<V1ObjectMeta> resource)
    {
        var capture = MetricsControlMetricsServerBackend.TryCaptureMetricsServerCharts(_cluster!, resource, _metricsServerHistory);
        _metricsServerHistory = capture.History;
        return capture.Tabs;
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

    private void SyncTabSelection()
    {
        for (var i = 0; i < Tabs.Count; i++)
        {
            Tabs[i].IsSelected = ReferenceEquals(Tabs[i], SelectedTab);
        }
    }

}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Metric panels are owned and disposed by their metric tab.")]
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
