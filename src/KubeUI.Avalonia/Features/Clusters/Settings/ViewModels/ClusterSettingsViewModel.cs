using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using System.ComponentModel;
using Avalonia.Threading;

namespace KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    private ClusterSettings? _subscribedClusterSettings;
    private ClusterWorkspaceViewModel? _subscribedCluster;

    public ISettingsService SettingsService { get; }

    public ClusterWorkspaceViewModel? Cluster { get; set; }

    public ClusterSettingsViewModel()
    {
        Title = Assets.Resources.ClusterSettingsViewModel_Title;

        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        if (_subscribedCluster != null)
        {
            _subscribedCluster.PropertyChanged -= OnClusterPropertyChanged;
        }

        Cluster = cluster;
        _subscribedCluster = cluster;
        _subscribedCluster.PropertyChanged += OnClusterPropertyChanged;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster);
        _ = LoadPrometheusProvidersAsync();
        RaiseMetricsVisibilityPropertiesChanged();
    }

    [ObservableProperty]
    public partial ClusterSettings ClusterSettings { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MetricProviderOption> PrometheusProviders { get; set; } = [];

    public bool ShowPrometheusSettings => ClusterSettings?.MetricsServiceType == MetricsServiceType.Prometheus;

    public bool ShowPrometheusProviderSettings => ShowPrometheusSettings;

    public bool ShowPrometheusServiceSettings => ShowPrometheusSettings && ClusterSettings?.PrometheusProviderKind != PrometheusProviderKind.External;

    public bool ShowPrometheusDirectUrlSettings => ShowPrometheusSettings;

    public bool ShowPrometheusBearerTokenSettings => ShowPrometheusSettings;

    public bool ShowDetectedMetricsBackend => ClusterSettings?.MetricsServiceType == MetricsServiceType.Auto && Cluster != null;

    public string? DetectedMetricsBackendText => !ShowDetectedMetricsBackend || Cluster == null
        ? null
        : Cluster.ActiveMetricsBackend.Type switch
        {
            MetricsServiceType.Prometheus when Cluster.ActivePrometheusProviderKind != null
                => string.Format(
                    Assets.Resources.ClusterSettingsView_AutoMetricsBackend_PrometheusWithProvider,
                    GetProviderDisplayName(Cluster.ActivePrometheusProviderKind.Value)),
            MetricsServiceType.Prometheus
                => Assets.Resources.ClusterSettingsView_AutoMetricsBackend_Prometheus,
            MetricsServiceType.KubernetesMetricsServer
                => Assets.Resources.ClusterSettingsView_AutoMetricsBackend_KubernetesMetricsServer,
            MetricsServiceType.None
                => Assets.Resources.ClusterSettingsView_AutoMetricsBackend_None,
            _ => null,
        };

    [ObservableProperty]
    public partial string Namespace { get; set; }

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

    partial void OnClusterSettingsChanged(ClusterSettings value)
    {
        if (_subscribedClusterSettings != null)
        {
            _subscribedClusterSettings.PropertyChanged -= OnClusterSettingsPropertyChanged;
        }

        _subscribedClusterSettings = value;

        if (_subscribedClusterSettings != null)
        {
            _subscribedClusterSettings.PropertyChanged += OnClusterSettingsPropertyChanged;
        }

        RaiseMetricsVisibilityPropertiesChanged();
    }

    private void OnClusterSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterSettings.MetricsServiceType) or nameof(ClusterSettings.PrometheusProviderKind))
        {
            RaiseMetricsVisibilityPropertiesChanged();
        }

        SettingsService.SaveSettings();
    }

    private void RaiseMetricsVisibilityPropertiesChanged()
    {
        OnPropertyChanged(nameof(ShowPrometheusSettings));
        OnPropertyChanged(nameof(ShowPrometheusProviderSettings));
        OnPropertyChanged(nameof(ShowPrometheusServiceSettings));
        OnPropertyChanged(nameof(ShowPrometheusDirectUrlSettings));
        OnPropertyChanged(nameof(ShowPrometheusBearerTokenSettings));
        OnPropertyChanged(nameof(ShowDetectedMetricsBackend));
        OnPropertyChanged(nameof(DetectedMetricsBackendText));
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind))
        {
            RaiseMetricsVisibilityPropertiesChanged();
        }
    }

    private async Task LoadPrometheusProvidersAsync()
    {
        if (Cluster == null)
        {
            return;
        }

        var providers = await Cluster.GetAvailablePrometheusProvidersAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            PrometheusProviders =
            [
                new MetricProviderOption(null, "Auto Detect"),
                .. providers.Select(static provider => new MetricProviderOption(provider.Kind, provider.Name)),
            ];
        });
    }

    private static string GetProviderDisplayName(PrometheusProviderKind providerKind) => providerKind switch
    {
        PrometheusProviderKind.Operator => Assets.Resources.ClusterSettingsView_PrometheusProvider_Operator,
        PrometheusProviderKind.OpenShift => Assets.Resources.ClusterSettingsView_PrometheusProvider_OpenShift,
        PrometheusProviderKind.Manual => Assets.Resources.ClusterSettingsView_PrometheusProvider_Manual,
        PrometheusProviderKind.External => Assets.Resources.ClusterSettingsView_PrometheusProvider_External,
        _ => providerKind.ToString(),
    };
}

public sealed record MetricProviderOption(PrometheusProviderKind? Id, string Name);



