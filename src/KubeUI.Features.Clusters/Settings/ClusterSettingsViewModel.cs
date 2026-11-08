using KubeUI.Controls.Presentation;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Settings;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private ClusterSettings? _subscribedClusterSettings;
    private ClusterMetricsSettings? _subscribedMetricsSettings;
    private INotifyPropertyChanged? _subscribedRuntime;
    private readonly IAzureMonitorWorkspaceService _azureMonitorWorkspaceService;
    private int _workspaceLoadVersion;
    private bool _disposed;

    public IClusterPreferenceStore SettingsService { get; }

    public IReadOnlyList<MetricsServiceOption> MetricsServiceOptions { get; } =
    [
        new(MetricsServiceType.Auto, L10n.Instance.ClusterSettingsView_MetricsServiceAuto.CurrentValue()!),
        new(MetricsServiceType.None, L10n.Instance.ClusterSettingsView_MetricsServiceNone.CurrentValue()!),
        new(MetricsServiceType.KubernetesMetricsServer, L10n.Instance.ClusterSettingsView_MetricsServiceMetricsServer.CurrentValue()!),
        new(MetricsServiceType.Prometheus, L10n.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue()!),
    ];

    public IReadOnlyList<PrometheusProviderOption> PrometheusProviderOptions { get; } =
    [
        new(null, L10n.Instance.ClusterSettingsView_PrometheusProviderAuto.CurrentValue()!),
        new(PrometheusProviderKind.Operator, L10n.Instance.ClusterSettingsView_PrometheusProviderOperator.CurrentValue()!),
        new(PrometheusProviderKind.OpenShift, L10n.Instance.ClusterSettingsView_PrometheusProviderOpenShift.CurrentValue()!),
        new(PrometheusProviderKind.Manual, L10n.Instance.ClusterSettingsView_PrometheusProviderManual.CurrentValue()!),
        new(PrometheusProviderKind.External, L10n.Instance.ClusterSettingsView_PrometheusProviderExternal.CurrentValue()!),
        new(PrometheusProviderKind.AzureMonitor, L10n.Instance.ClusterSettingsView_PrometheusProviderAzureMonitor.CurrentValue()!),
    ];

    public ClusterWorkspace? Cluster { get; set; }

    public ClusterSettingsViewModel(IClusterPreferenceStore settingsService, IAzureMonitorWorkspaceService azureMonitorWorkspaceService)
    {
        Title = L10n.Instance.ClusterSettingsView_Title.CurrentValue()!;
        SettingsService = settingsService;
        _azureMonitorWorkspaceService = azureMonitorWorkspaceService;
        AzureMonitorStatusText = L10n.Instance.ClusterSettingsView_AzureMonitorNotChecked.CurrentValue()!;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged -= OnRuntimePropertyChanged;
        }

        Cluster = cluster;
        _subscribedRuntime = Cluster.Runtime as INotifyPropertyChanged;
        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged += OnRuntimePropertyChanged;
        }

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
        && SelectedPrometheusProvider?.Value is not (PrometheusProviderKind.External or PrometheusProviderKind.AzureMonitor);

    public bool ShowPrometheusDirectUrlSettings => ShowPrometheusSettings && SelectedPrometheusProvider?.Value != PrometheusProviderKind.AzureMonitor;

    public bool ShowPrometheusCustomTransportSettings => ShowPrometheusSettings && SelectedPrometheusProvider?.Value != PrometheusProviderKind.AzureMonitor;

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
                    AzureMonitorStatusText = L10n.Instance.ClusterSettingsView_AzureMonitorNotSignedIn.CurrentValue()!;
                    AzureMonitorSubscriptions = [];
                    AzureMonitorWorkspaces = [];
                    OnPropertyChanged(nameof(SelectedAzureMonitorSubscription));
                    OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                });
                return;
            }

            var accountText = string.Format(
                L10n.Instance.ClusterSettingsView_AzureMonitorSignedInFormat.CurrentValue()!,
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
                    AzureMonitorStatusText = L10n.Instance.ClusterSettingsView_AzureMonitorNoSubscriptions.CurrentValue()!;
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
                    AzureMonitorStatusText = L10n.Instance.ClusterSettingsView_AzureMonitorNoWorkspaces.CurrentValue()!;
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
                return L10n.Instance.ClusterSettingsView_ActiveMetricsServiceNotConnected.CurrentValue()!;
            }

            if (!Cluster.Runtime.IsMetricsAvailable || Cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.None)
            {
                return ClusterSettings?.MetricsSettings.MetricsServiceType == MetricsServiceType.None
                    ? L10n.Instance.ClusterSettingsView_ActiveMetricsServiceDisabled.CurrentValue()!
                    : L10n.Instance.ClusterSettingsView_ActiveMetricsServiceUnavailable.CurrentValue()!;
            }

            return Cluster.Runtime.ActiveMetricsBackend.Type switch
            {
                MetricsServiceType.KubernetesMetricsServer => L10n.Instance.ClusterSettingsView_ActiveMetricsServiceMetricsServer.CurrentValue()!,
                MetricsServiceType.Prometheus when Cluster.Runtime.ActiveMetricsBackend.PrometheusProviderKind is { } provider
                    => string.Format(
                        L10n.Instance.ClusterSettingsView_ActiveMetricsServicePrometheusFormat.CurrentValue()!,
                        L10n.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue(),
                        GetPrometheusProviderLabel(provider)),
                MetricsServiceType.Prometheus => L10n.Instance.ClusterSettingsView_MetricsServicePrometheus.CurrentValue()!,
                _ => Cluster.Runtime.ActiveMetricsBackend.Type.ToString(),
            };
        }
    }

    [ObservableProperty]
    public partial bool IsRefreshingPermissions { get; set; }

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
        SettingsService.SaveSettings();

        if (e.PropertyName == nameof(ClusterSettings.MetricsSettings))
        {
            SubscribeMetricsSettings(ClusterSettings.MetricsSettings);
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
    }

    private void OnMetricsSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();
        if (e.PropertyName is nameof(ClusterMetricsSettings.MetricsServiceType) or nameof(ClusterMetricsSettings.PrometheusProviderKind))
        {
            RaiseMetricsPropertiesChanged();
        }
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IClusterRuntime.Connected)
            or nameof(IClusterRuntime.IsMetricsAvailable)
            or nameof(IClusterRuntime.ActiveMetricsBackend))
        {
            OnPropertyChanged(nameof(ActiveMetricsServiceText));
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
        OnPropertyChanged(nameof(ActiveMetricsServiceText));
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

        if (_subscribedRuntime is not null)
        {
            _subscribedRuntime.PropertyChanged -= OnRuntimePropertyChanged;
            _subscribedRuntime = null;
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
    }
}

public sealed record MetricsServiceOption(MetricsServiceType Value, string Label);

public sealed record PrometheusProviderOption(PrometheusProviderKind? Value, string Label);
