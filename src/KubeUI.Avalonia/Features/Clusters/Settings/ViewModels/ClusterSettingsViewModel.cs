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

    public ISettingsService SettingsService { get; }

    public ClusterWorkspaceViewModel? Cluster { get; set; }

    public ClusterSettingsViewModel()
    {
        Title = Assets.Resources.ClusterSettingsViewModel_Title;

        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster);
        _ = LoadPrometheusProvidersAsync();
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
}

public sealed record MetricProviderOption(PrometheusProviderKind? Id, string Name);



