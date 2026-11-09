using System.Collections.Specialized;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Settings;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private ClusterSettings? _subscribedClusterSettings;
    private ClusterMetricsSettings? _subscribedMetricsSettings;
    private ObservableCollection<PrometheusHeader>? _subscribedPrometheusHeaders;
    private INotifyPropertyChanged? _subscribedRuntime;
    private IClusterMetrics? _subscribedClusterMetrics;
    private readonly IAzureMonitorWorkspaceService _azureMonitorWorkspaceService;
    private readonly DispatcherTimer _settingsSaveTimer = new(DispatcherPriority.Background);
    private int _workspaceLoadVersion;
    private bool _disposed;
    private bool _settingsSavePending;
    private bool _metricsReinitializePending;

    public IClusterPreferenceStore SettingsService { get; }

    public IReadOnlyList<MetricsServiceOption> MetricsServiceOptions { get; } =
    [
        new(MetricsServiceType.Auto, ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServiceAuto.CurrentValue()!),
        new(MetricsServiceType.None, ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServiceNone.CurrentValue()!),
        new(MetricsServiceType.KubernetesMetricsServer, ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServiceMetricsServer.CurrentValue()!),
        new(MetricsServiceType.Prometheus, ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue()!),
    ];

    public IReadOnlyList<PrometheusProviderOption> PrometheusProviderOptions { get; } =
    [
        new(null, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderAuto.CurrentValue()!),
        new(PrometheusProviderKind.Operator, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderOperator.CurrentValue()!),
        new(PrometheusProviderKind.OpenShift, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderOpenShift.CurrentValue()!),
        new(PrometheusProviderKind.Manual, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderManual.CurrentValue()!),
        new(PrometheusProviderKind.External, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderExternal.CurrentValue()!),
        new(PrometheusProviderKind.AzureMonitor, ClustersLanguageManager.Instance.ClusterSettingsView_PrometheusProviderAzureMonitor.CurrentValue()!),
    ];

    public ClusterWorkspace? Cluster { get; set; }

    public ClusterSettingsViewModel(IClusterPreferenceStore settingsService, IAzureMonitorWorkspaceService azureMonitorWorkspaceService)
    {
        Title = ClustersLanguageManager.Instance.ClusterSettingsView_Title.CurrentValue()!;
        SettingsService = settingsService;
        _azureMonitorWorkspaceService = azureMonitorWorkspaceService;
        AzureMonitorStatusText = ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorNotChecked.CurrentValue()!;
        _settingsSaveTimer.Interval = TimeSpan.FromMilliseconds(300);
        _settingsSaveTimer.Tick += OnSettingsSaveTimerTick;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged -= OnRuntimePropertyChanged;
        }

        if (_subscribedClusterMetrics is not null)
        {
            _subscribedClusterMetrics.PropertyChanged -= OnClusterMetricsPropertyChanged;
        }

        Cluster = cluster;
        _subscribedRuntime = Cluster.Runtime as INotifyPropertyChanged;
        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged += OnRuntimePropertyChanged;
        }

        _subscribedClusterMetrics = Cluster.Runtime.Metrics;
        _subscribedClusterMetrics.PropertyChanged += OnClusterMetricsPropertyChanged;

        Id = nameof(ClusterSettingsViewModel) + Cluster.Runtime.Name;
        ClusterSettings = SettingsService.GetClusterSettings(cluster.Runtime);
        DebugContainerImage = ClusterSettings.DebugContainerImage;
        AzureMonitorSubscriptions = [];
        AzureMonitorWorkspaces = [];
        SelectedAzureMonitorSubscription = AzureMonitorSubscriptions.FirstOrDefault(
            subscription => subscription.SubscriptionId == ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId);
        SelectedAzureMonitorWorkspace = AzureMonitorWorkspaces.FirstOrDefault(
            workspace => workspace.ResourceId == ClusterSettings.MetricsSettings.AzureMonitorWorkspaceId);

        if (ClusterSettings.MetricsSettings.PrometheusProviderKind == PrometheusProviderKind.AzureMonitor
            && !string.IsNullOrWhiteSpace(ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId))
        {
            _ = RefreshAzureMonitorAsync(CancellationToken.None);
        }
    }

    [ObservableProperty]
    public partial ClusterSettings ClusterSettings { get; set; }

    [ObservableProperty]
    public partial string Namespace { get; set; }

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = string.Empty;

    public MetricsServiceOption? SelectedMetricsService
    {
        get
        {
            var current = ClusterSettings?.MetricsSettings.MetricsServiceType ?? MetricsServiceType.Auto;
            foreach (var option in MetricsServiceOptions)
            {
                if (option.Value == current)
                {
                    return option;
                }
            }

            return null;
        }
        set
        {
            if (ClusterSettings is null || value is null || ClusterSettings.MetricsSettings.MetricsServiceType == value.Value)
            {
                return;
            }

            ClusterSettings.MetricsSettings.MetricsServiceType = value.Value;
        }
    }

    public PrometheusProviderOption? SelectedPrometheusProvider
    {
        get
        {
            var current = ClusterSettings?.MetricsSettings.PrometheusProviderKind;
            foreach (var option in PrometheusProviderOptions)
            {
                if (option.Value == current)
                {
                    return option;
                }
            }

            return null;
        }
        set
        {
            if (ClusterSettings is null || value is null || ClusterSettings.MetricsSettings.PrometheusProviderKind == value.Value)
            {
                return;
            }

            ClusterSettings.MetricsSettings.PrometheusProviderKind = value.Value;
        }
    }

    public bool ShowPrometheusSettings => ClusterSettings?.MetricsSettings.MetricsServiceType == MetricsServiceType.Prometheus;

    public bool ShowPrometheusServiceSettings => ShowPrometheusSettings
        && SelectedPrometheusProvider?.Value is not (PrometheusProviderKind.External or PrometheusProviderKind.AzureMonitor)
        && string.IsNullOrWhiteSpace(ClusterSettings?.MetricsSettings.PrometheusDirectUrl);

    public bool ShowPrometheusDirectUrlSettings => ShowPrometheusSettings && SelectedPrometheusProvider?.Value != PrometheusProviderKind.AzureMonitor;

    public bool ShowPrometheusCustomTransportSettings => ShowPrometheusSettings && SelectedPrometheusProvider?.Value != PrometheusProviderKind.AzureMonitor;

    public bool ShowPrometheusAuthenticationConflictWarning =>
        (!string.IsNullOrEmpty(ClusterSettings?.MetricsSettings.PrometheusBasicAuthUsername)
            || !string.IsNullOrEmpty(ClusterSettings?.MetricsSettings.PrometheusBasicAuthPassword))
        && ClusterSettings.MetricsSettings.PrometheusCustomHeaders.Any(static header =>
            string.Equals(header.Name, "Authorization", StringComparison.OrdinalIgnoreCase));

    public bool ShowInsecureAuthenticationWarning
    {
        get
        {
            if (!ShowPrometheusCustomTransportSettings || !HasPrometheusAuthentication)
            {
                return false;
            }

            var directUrl = ClusterSettings.MetricsSettings.PrometheusDirectUrl;
            if (Uri.TryCreate(directUrl, UriKind.Absolute, out var uri))
            {
                return uri.Scheme != Uri.UriSchemeHttps;
            }

            return !ClusterSettings.MetricsSettings.PrometheusUseHttps;
        }
    }

    private bool HasPrometheusAuthentication =>
        ClusterSettings.MetricsSettings.PrometheusCustomHeaders.Any(static header =>
            !string.IsNullOrWhiteSpace(header.Name) || !string.IsNullOrEmpty(header.Value))
        || !string.IsNullOrEmpty(ClusterSettings.MetricsSettings.PrometheusBasicAuthUsername)
        || !string.IsNullOrEmpty(ClusterSettings.MetricsSettings.PrometheusBasicAuthPassword);

    public bool ShowAzureMonitorSettings => ShowPrometheusSettings && SelectedPrometheusProvider?.Value == PrometheusProviderKind.AzureMonitor;

    [ObservableProperty]
    public partial ObservableCollection<AzureMonitorSubscriptionInfo> AzureMonitorSubscriptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<AzureMonitorWorkspaceInfo> AzureMonitorWorkspaces { get; set; } = [];

    [ObservableProperty]
    public partial string AzureMonitorStatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsAzureMonitorBusy { get; set; }

    public AzureMonitorSubscriptionInfo? SelectedAzureMonitorSubscription
    {
        get => AzureMonitorSubscriptions.FirstOrDefault(subscription => subscription.SubscriptionId == ClusterSettings?.MetricsSettings.AzureMonitorSubscriptionId);
        set
        {
            if (value is null || ClusterSettings is null || ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId == value.SubscriptionId)
            {
                return;
            }

            ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId = value.SubscriptionId;
            ClusterSettings.MetricsSettings.AzureMonitorWorkspaceId = null;
            ClusterSettings.MetricsSettings.AzureMonitorQueryEndpoint = null;
            OnPropertyChanged();
            _ = LoadAzureMonitorWorkspacesAsync(value);
        }
    }

    public AzureMonitorWorkspaceInfo? SelectedAzureMonitorWorkspace
    {
        get => AzureMonitorWorkspaces.FirstOrDefault(workspace => workspace.ResourceId == ClusterSettings?.MetricsSettings.AzureMonitorWorkspaceId);
        set
        {
            if (value is null || ClusterSettings is null || ClusterSettings.MetricsSettings.AzureMonitorWorkspaceId == value.ResourceId)
            {
                return;
            }

            ClusterSettings.MetricsSettings.AzureMonitorWorkspaceId = value.ResourceId;
            ClusterSettings.MetricsSettings.AzureMonitorQueryEndpoint = value.QueryEndpoint;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private async Task RefreshAzureMonitorAsync(CancellationToken cancellationToken)
    {
        IsAzureMonitorBusy = true;
        try
        {
            var auth = await _azureMonitorWorkspaceService
                .GetAuthenticationStatusAsync(cancellationToken).ConfigureAwait(false);
            if (!auth.AzureCliSignedIn)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    AzureMonitorStatusText = ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorNotSignedIn.CurrentValue()!;
                    AzureMonitorSubscriptions = [];
                    AzureMonitorWorkspaces = [];
                    OnPropertyChanged(nameof(SelectedAzureMonitorSubscription));
                    OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                });
                return;
            }

            var accountText = string.Format(
                ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorSignedInFormat.CurrentValue()!,
                auth.Username ?? "(unknown)",
                auth.TenantId ?? "(unknown)");
            var subscriptions = await _azureMonitorWorkspaceService
                .GetSubscriptionsAsync(cancellationToken).ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AzureMonitorStatusText = accountText;
                AzureMonitorSubscriptions = new ObservableCollection<AzureMonitorSubscriptionInfo>(subscriptions);
                OnPropertyChanged(nameof(SelectedAzureMonitorSubscription));
                if (subscriptions.Count == 0)
                {
                    AzureMonitorStatusText = ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorNoSubscriptions.CurrentValue()!;
                    AzureMonitorWorkspaces = [];
                    OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                    return;
                }

                var selected = AzureMonitorSubscriptions.FirstOrDefault(
                    subscription => subscription.SubscriptionId == ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId)
                    ?? AzureMonitorSubscriptions.FirstOrDefault();
                if (selected is null)
                {
                    AzureMonitorWorkspaces = [];
                    return;
                }

                if (ClusterSettings.MetricsSettings.AzureMonitorSubscriptionId == selected.SubscriptionId)
                {
                    _ = LoadAzureMonitorWorkspacesAsync(selected);
                }
                else
                {
                    SelectedAzureMonitorSubscription = selected;
                }
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => AzureMonitorStatusText = ex.Message);
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => IsAzureMonitorBusy = false);
        }
    }

    private async Task LoadAzureMonitorWorkspacesAsync(AzureMonitorSubscriptionInfo subscription)
    {
        var loadVersion = Interlocked.Increment(ref _workspaceLoadVersion);
        try
        {
            var workspaces = await _azureMonitorWorkspaceService
                .GetWorkspacesAsync(subscription.SubscriptionId).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (loadVersion != _workspaceLoadVersion)
                {
                    return;
                }

                AzureMonitorWorkspaces = new ObservableCollection<AzureMonitorWorkspaceInfo>(
                    workspaces.OrderBy(workspace => workspace.Name, StringComparer.Ordinal));
                OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                if (SelectedAzureMonitorWorkspace is null && AzureMonitorWorkspaces.Count > 0)
                {
                    SelectedAzureMonitorWorkspace = AzureMonitorWorkspaces[0];
                }

                if (AzureMonitorWorkspaces.Count == 0)
                {
                    AzureMonitorStatusText = ClustersLanguageManager.Instance.ClusterSettingsView_AzureMonitorNoWorkspaces.CurrentValue()!;
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (loadVersion == _workspaceLoadVersion)
                {
                    AzureMonitorStatusText = ex.Message;
                    AzureMonitorWorkspaces = [];
                }
            });
        }
    }

    public string ActiveMetricsServiceText
    {
        get
        {
            if (Cluster is null || !Cluster.Runtime.Connected)
            {
                return ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceNotConnected.CurrentValue()!;
            }

            if (!Cluster.Runtime.Metrics.IsMetricsAvailable || Cluster.Runtime.Metrics.ActiveMetricsBackend.Type == MetricsServiceType.None)
            {
                return ClusterSettings?.MetricsSettings.MetricsServiceType == MetricsServiceType.None
                    ? ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceDisabled.CurrentValue()!
                    : ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceUnavailable.CurrentValue()!;
            }

            return Cluster.Runtime.Metrics.ActiveMetricsBackend.Type switch
            {
                MetricsServiceType.KubernetesMetricsServer => ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServiceMetricsServer.CurrentValue()!,
                MetricsServiceType.Prometheus when Cluster.Runtime.Metrics.ActiveMetricsBackend.PrometheusProviderKind is { } provider
                    => string.Format(
                        ClustersLanguageManager.Instance.ClusterSettingsView_ActiveMetricsServicePrometheusFormat.CurrentValue()!,
                        ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue(),
                        GetPrometheusProviderLabel(provider)),
                MetricsServiceType.Prometheus => ClustersLanguageManager.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue()!,
                _ => Cluster.Runtime.Metrics.ActiveMetricsBackend.Type.ToString(),
            };
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefreshPermissions))]
    public partial bool IsRefreshingPermissions { get; set; }

    public bool CanRefreshPermissions => !IsRefreshingPermissions;

    public string ClusterTitle => string.Format(
        ClustersLanguageManager.Instance.ClusterSettingsView_TitleFormat.CurrentValue()!,
        Cluster?.Runtime.Name);

    [RelayCommand]
    private async Task RefreshPermissionsAsync()
    {
        if (Cluster is null || IsRefreshingPermissions)
        {
            return;
        }

        IsRefreshingPermissions = true;
        try
        {
            await Cluster.RefreshPermissionsAsync();
        }
        finally
        {
            IsRefreshingPermissions = false;
        }
    }

    [RelayCommand]
    private void AddNamespace()
    {
        if (!string.IsNullOrEmpty(Namespace) && !ClusterSettings.Namespaces.Contains(Namespace))
        {
            ClusterSettings.Namespaces.Add(Namespace);

            SettingsService.SaveSettings();

            Namespace = "";
        }
    }

    [RelayCommand]
    private void RemoveNamespace(string ns)
    {
        if (!string.IsNullOrEmpty(ns) && ClusterSettings.Namespaces.Contains(ns))
        {
            ClusterSettings.Namespaces.Remove(ns);

            SettingsService.SaveSettings();
        }
    }

    partial void OnDebugContainerImageChanged(string value)
    {
        if (ClusterSettings == null)
        {
            return;
        }

        ClusterSettings.DebugContainerImage = value;
        SettingsService.SaveSettings();
    }

    partial void OnClusterSettingsChanged(ClusterSettings value)
    {
        if (_subscribedClusterSettings is not null)
        {
            _subscribedClusterSettings.PropertyChanged -= OnClusterSettingsPropertyChanged;
        }

        _subscribedClusterSettings = value;
        _subscribedClusterSettings.PropertyChanged += OnClusterSettingsPropertyChanged;
        SubscribeMetricsSettings(value.MetricsSettings);
        RaiseMetricsPropertiesChanged();
    }

    private void OnClusterSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClusterSettings.MetricsSettings))
        {
            SubscribeMetricsSettings(ClusterSettings.MetricsSettings);
            QueueSettingsSave(reinitializeMetrics: true);
            RaiseMetricsPropertiesChanged();
        }
    }

    private void SubscribeMetricsSettings(ClusterMetricsSettings metricsSettings)
    {
        if (_subscribedMetricsSettings is not null)
        {
            _subscribedMetricsSettings.PropertyChanged -= OnMetricsSettingsPropertyChanged;
        }

        _subscribedMetricsSettings = metricsSettings;
        _subscribedMetricsSettings.PropertyChanged += OnMetricsSettingsPropertyChanged;
        SubscribePrometheusHeaders(metricsSettings.PrometheusCustomHeaders);
    }

    private void SubscribePrometheusHeaders(ObservableCollection<PrometheusHeader> headers)
    {
        if (_subscribedPrometheusHeaders is not null)
        {
            _subscribedPrometheusHeaders.CollectionChanged -= OnPrometheusHeadersCollectionChanged;
            UnsubscribePrometheusHeaderItems(_subscribedPrometheusHeaders);
        }

        _subscribedPrometheusHeaders = headers;
        _subscribedPrometheusHeaders.CollectionChanged += OnPrometheusHeadersCollectionChanged;
        SubscribePrometheusHeaderItems(_subscribedPrometheusHeaders);
    }

    private void SubscribePrometheusHeaderItems(IEnumerable<PrometheusHeader> headers)
    {
        foreach (var header in headers)
        {
            header.PropertyChanged -= OnPrometheusHeaderPropertyChanged;
            header.PropertyChanged += OnPrometheusHeaderPropertyChanged;
        }
    }

    private void UnsubscribePrometheusHeaderItems(IEnumerable<PrometheusHeader> headers)
    {
        foreach (var header in headers)
        {
            header.PropertyChanged -= OnPrometheusHeaderPropertyChanged;
        }
    }

    private void OnPrometheusHeadersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (PrometheusHeader header in e.OldItems)
            {
                header.PropertyChanged -= OnPrometheusHeaderPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (PrometheusHeader header in e.NewItems)
            {
                header.PropertyChanged -= OnPrometheusHeaderPropertyChanged;
                header.PropertyChanged += OnPrometheusHeaderPropertyChanged;
            }
        }

        QueueSettingsSave(reinitializeMetrics: true);
        RaiseMetricsPropertiesChanged();
    }

    private void OnPrometheusHeaderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        QueueSettingsSave(reinitializeMetrics: true);
        RaiseMetricsPropertiesChanged();
    }

    private void OnMetricsSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClusterMetricsSettings.PrometheusCustomHeaders))
        {
            SubscribePrometheusHeaders(ClusterSettings.MetricsSettings.PrometheusCustomHeaders);
        }

        QueueSettingsSave(reinitializeMetrics: true);
        RaiseMetricsPropertiesChanged();
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IClusterRuntime.Connected))
        {
            OnPropertyChanged(nameof(ActiveMetricsServiceText));
        }
    }

    private void OnClusterMetricsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IClusterMetrics.IsMetricsAvailable)
            or nameof(IClusterMetrics.ActiveMetricsBackend))
        {
            OnPropertyChanged(nameof(ActiveMetricsServiceText));
        }
    }

    private void QueueSettingsSave(bool reinitializeMetrics = false)
    {
        _settingsSavePending = true;
        _metricsReinitializePending |= reinitializeMetrics;
        _settingsSaveTimer.Stop();
        _settingsSaveTimer.Start();
    }

    private void OnSettingsSaveTimerTick(object? sender, EventArgs e)
    {
        _settingsSaveTimer.Stop();
        FlushSettingsSave(reinitializeMetrics: true);
    }

    private void FlushSettingsSave(bool reinitializeMetrics)
    {
        if (_settingsSavePending)
        {
            _settingsSavePending = false;
            SettingsService.SaveSettings();
        }

        if (_metricsReinitializePending)
        {
            _metricsReinitializePending = false;
            if (reinitializeMetrics && Cluster?.Runtime.Connected == true)
            {
                _ = Cluster.Runtime.ReinitializeMetricsAsync();
            }
        }
    }

    private string GetPrometheusProviderLabel(PrometheusProviderKind provider)
    {
        foreach (var option in PrometheusProviderOptions)
        {
            if (option.Value == provider)
            {
                return option.Label;
            }
        }

        return provider.ToString();
    }

    private void RaiseMetricsPropertiesChanged()
    {
        OnPropertyChanged(nameof(SelectedMetricsService));
        OnPropertyChanged(nameof(SelectedPrometheusProvider));
        OnPropertyChanged(nameof(ShowPrometheusSettings));
        OnPropertyChanged(nameof(ShowPrometheusServiceSettings));
        OnPropertyChanged(nameof(ShowPrometheusDirectUrlSettings));
        OnPropertyChanged(nameof(ShowPrometheusCustomTransportSettings));
        OnPropertyChanged(nameof(ShowAzureMonitorSettings));
        OnPropertyChanged(nameof(ShowPrometheusAuthenticationConflictWarning));
        OnPropertyChanged(nameof(ShowInsecureAuthenticationWarning));
        OnPropertyChanged(nameof(ActiveMetricsServiceText));
    }

    [RelayCommand]
    private void AddPrometheusHeader()
    {
        ClusterSettings.MetricsSettings.PrometheusCustomHeaders.Add(new PrometheusHeader());
    }

    [RelayCommand]
    private void RemovePrometheusHeader(PrometheusHeader? header)
    {
        if (header is not null)
        {
            ClusterSettings.MetricsSettings.PrometheusCustomHeaders.Remove(header);
        }
    }

    /// <summary>Releases the event subscriptions held by this view model.</summary>
    /// <remarks>
    /// The cluster runtime, the cluster settings and the metrics settings all hold strong references
    /// to this view model while subscribed. Without an explicit teardown the view model would outlive
    /// the settings document and continue reacting to changes on the cluster it was displaying.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _settingsSaveTimer.Stop();
        _settingsSaveTimer.Tick -= OnSettingsSaveTimerTick;
        FlushSettingsSave(reinitializeMetrics: true);

        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged -= OnRuntimePropertyChanged;
            _subscribedRuntime = null;
        }

        if (_subscribedClusterMetrics is not null)
        {
            _subscribedClusterMetrics.PropertyChanged -= OnClusterMetricsPropertyChanged;
            _subscribedClusterMetrics = null;
        }

        if (_subscribedClusterSettings is not null)
        {
            _subscribedClusterSettings.PropertyChanged -= OnClusterSettingsPropertyChanged;
            _subscribedClusterSettings = null;
        }

        if (_subscribedMetricsSettings is not null)
        {
            _subscribedMetricsSettings.PropertyChanged -= OnMetricsSettingsPropertyChanged;
            _subscribedMetricsSettings = null;
        }

        if (_subscribedPrometheusHeaders is not null)
        {
            _subscribedPrometheusHeaders.CollectionChanged -= OnPrometheusHeadersCollectionChanged;
            UnsubscribePrometheusHeaderItems(_subscribedPrometheusHeaders);
            _subscribedPrometheusHeaders = null;
        }
    }
}

public sealed record MetricsServiceOption(MetricsServiceType Value, string Label);

public sealed record PrometheusProviderOption(PrometheusProviderKind? Value, string Label);
