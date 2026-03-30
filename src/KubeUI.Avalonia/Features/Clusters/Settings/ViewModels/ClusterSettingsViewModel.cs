using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using System.ComponentModel;

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
    }

    [ObservableProperty]
    public partial ClusterSettings ClusterSettings { get; set; }

    public bool ShowPrometheusServiceSettings => ClusterSettings?.MetricsServiceType == MetricsServiceType.Prometheus;

    public bool ShowPrometheusExternalSettings => ClusterSettings?.MetricsServiceType == MetricsServiceType.PrometheusExternal;

    public bool ShowAzureManagedPrometheusSettings => ClusterSettings?.MetricsServiceType == MetricsServiceType.AzureManagedPrometheus;

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
        if (e.PropertyName == nameof(ClusterSettings.MetricsServiceType))
        {
            RaiseMetricsVisibilityPropertiesChanged();
            SettingsService.SaveSettings();
        }
    }

    private void RaiseMetricsVisibilityPropertiesChanged()
    {
        OnPropertyChanged(nameof(ShowPrometheusServiceSettings));
        OnPropertyChanged(nameof(ShowPrometheusExternalSettings));
        OnPropertyChanged(nameof(ShowAzureManagedPrometheusSettings));
    }
}



