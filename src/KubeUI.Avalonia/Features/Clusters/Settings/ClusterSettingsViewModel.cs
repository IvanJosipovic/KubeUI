using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Settings;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    private ClusterSettings? _subscribedClusterSettings;
    private INotifyPropertyChanged? _subscribedRuntime;
    private readonly IAzureMonitorWorkspaceService _azureMonitorWorkspaceService;
    private int _workspaceLoadVersion;

    public ISettingsService SettingsService { get; }

    public IReadOnlyList<MetricsServiceOption> MetricsServiceOptions { get; } =
    [
        new(MetricsServiceType.Auto, Assets.Resources.ClusterSettingsView_MetricsServiceAuto!),
        new(MetricsServiceType.None, Assets.Resources.ClusterSettingsView_MetricsServiceNone!),
        new(MetricsServiceType.KubernetesMetricsServer, Assets.Resources.ClusterSettingsView_MetricsServiceMetricsServer!),
        new(MetricsServiceType.Prometheus, Assets.Resources.ClusterSettingsView_MetricsServicePrometheus!),
    ];

    public IReadOnlyList<PrometheusProviderOption> PrometheusProviderOptions { get; } =
    [
        new(null, Assets.Resources.ClusterSettingsView_PrometheusProviderAuto!),
        new(PrometheusProviderKind.Operator, Assets.Resources.ClusterSettingsView_PrometheusProviderOperator!),
        new(PrometheusProviderKind.OpenShift, Assets.Resources.ClusterSettingsView_PrometheusProviderOpenShift!),
        new(PrometheusProviderKind.Manual, Assets.Resources.ClusterSettingsView_PrometheusProviderManual!),
        new(PrometheusProviderKind.External, Assets.Resources.ClusterSettingsView_PrometheusProviderExternal!),
        new(PrometheusProviderKind.AzureMonitor, Assets.Resources.ClusterSettingsView_PrometheusProviderAzureMonitor!),
    ];

    public ClusterWorkspace? Cluster { get; set; }

    public ClusterSettingsViewModel(ISettingsService settingsService, IAzureMonitorWorkspaceService azureMonitorWorkspaceService)
    {
        Title = Assets.Resources.ClusterSettingsView_Title!;
        SettingsService = settingsService;
        _azureMonitorWorkspaceService = azureMonitorWorkspaceService;
        AzureMonitorStatusText = Assets.Resources.ClusterSettingsView_AzureMonitorNotChecked!;
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
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster.Runtime);
        DebugContainerImage = ClusterSettings.DebugContainerImage;
        AzureMonitorSubscriptions = [];
        AzureMonitorWorkspaces = [];
        SelectedAzureMonitorSubscription = AzureMonitorSubscriptions.FirstOrDefault(
            subscription => subscription.SubscriptionId == ClusterSettings.AzureMonitorSubscriptionId);
        SelectedAzureMonitorWorkspace = AzureMonitorWorkspaces.FirstOrDefault(
            workspace => workspace.ResourceId == ClusterSettings.AzureMonitorWorkspaceId);
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
            var current = ClusterSettings?.MetricsServiceType ?? MetricsServiceType.Auto;
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
            if (ClusterSettings is null || value is null || ClusterSettings.MetricsServiceType == value.Value)
            {
                return;
            }

            ClusterSettings.MetricsServiceType = value.Value;
        }
    }

    public PrometheusProviderOption? SelectedPrometheusProvider
    {
        get
        {
            var current = ClusterSettings?.PrometheusProviderKind;
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
            if (ClusterSettings is null || value is null || ClusterSettings.PrometheusProviderKind == value.Value)
            {
                return;
            }

            ClusterSettings.PrometheusProviderKind = value.Value;
        }
    }

    public bool ShowPrometheusSettings => ClusterSettings?.MetricsServiceType == MetricsServiceType.Prometheus;

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
        get => AzureMonitorSubscriptions.FirstOrDefault(subscription => subscription.SubscriptionId == ClusterSettings?.AzureMonitorSubscriptionId);
        set
        {
            if (value is null || ClusterSettings is null || ClusterSettings.AzureMonitorSubscriptionId == value.SubscriptionId)
            {
                return;
            }

            ClusterSettings.AzureMonitorSubscriptionId = value.SubscriptionId;
            OnPropertyChanged();
            _ = LoadAzureMonitorWorkspacesAsync(value);
        }
    }

    public AzureMonitorWorkspaceInfo? SelectedAzureMonitorWorkspace
    {
        get => AzureMonitorWorkspaces.FirstOrDefault(workspace => workspace.ResourceId == ClusterSettings?.AzureMonitorWorkspaceId);
        set
        {
            if (value is null || ClusterSettings is null || ClusterSettings.AzureMonitorWorkspaceId == value.ResourceId)
            {
                return;
            }

            ClusterSettings.AzureMonitorWorkspaceId = value.ResourceId;
            ClusterSettings.AzureMonitorQueryEndpoint = value.QueryEndpoint;
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
                    AzureMonitorStatusText = Assets.Resources.ClusterSettingsView_AzureMonitorNotSignedIn!;
                    AzureMonitorSubscriptions = [];
                    AzureMonitorWorkspaces = [];
                    OnPropertyChanged(nameof(SelectedAzureMonitorSubscription));
                    OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                });
                return;
            }

            var accountText = string.Format(
                Assets.Resources.ClusterSettingsView_AzureMonitorSignedInFormat!,
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
                    AzureMonitorStatusText = Assets.Resources.ClusterSettingsView_AzureMonitorNoSubscriptions!;
                    AzureMonitorWorkspaces = [];
                    OnPropertyChanged(nameof(SelectedAzureMonitorWorkspace));
                    return;
                }

                var selected = AzureMonitorSubscriptions.FirstOrDefault(
                    subscription => subscription.SubscriptionId == ClusterSettings.AzureMonitorSubscriptionId)
                    ?? AzureMonitorSubscriptions.FirstOrDefault();
                if (selected is null)
                {
                    AzureMonitorWorkspaces = [];
                    return;
                }

                if (ClusterSettings.AzureMonitorSubscriptionId == selected.SubscriptionId)
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
                    AzureMonitorStatusText = Assets.Resources.ClusterSettingsView_AzureMonitorNoWorkspaces!;
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
                return Assets.Resources.ClusterSettingsView_ActiveMetricsServiceNotConnected!;
            }

            if (!Cluster.Runtime.IsMetricsAvailable || Cluster.Runtime.ActiveMetricsBackend.Type == MetricsServiceType.None)
            {
                return ClusterSettings?.MetricsServiceType == MetricsServiceType.None
                    ? Assets.Resources.ClusterSettingsView_ActiveMetricsServiceDisabled!
                    : Assets.Resources.ClusterSettingsView_ActiveMetricsServiceUnavailable!;
            }

            return Cluster.Runtime.ActiveMetricsBackend.Type switch
            {
                MetricsServiceType.KubernetesMetricsServer => Assets.Resources.ClusterSettingsView_ActiveMetricsServiceMetricsServer!,
                MetricsServiceType.Prometheus when Cluster.Runtime.ActiveMetricsBackend.PrometheusProviderKind is { } provider
                    => string.Format(
                        Assets.Resources.ClusterSettingsView_ActiveMetricsServicePrometheusFormat!,
                        Assets.Resources.ClusterSettingsView_MetricsServicePrometheus,
                        GetPrometheusProviderLabel(provider)),
                MetricsServiceType.Prometheus => Assets.Resources.ClusterSettingsView_MetricsServicePrometheus!,
                _ => Cluster.Runtime.ActiveMetricsBackend.Type.ToString(),
            };
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
        RaiseMetricsPropertiesChanged();
    }

    private void OnClusterSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();

        if (e.PropertyName is nameof(ClusterSettings.MetricsServiceType) or nameof(ClusterSettings.PrometheusProviderKind))
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
}

public sealed record MetricsServiceOption(MetricsServiceType Value, string Label);

public sealed record PrometheusProviderOption(PrometheusProviderKind? Value, string Label);
