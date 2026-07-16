using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    public ISettingsService SettingsService { get; }

    public ClusterWorkspaceViewModel? Cluster { get; set; }

    public ClusterSettingsViewModel(ISettingsService settingsService)
    {
        Title = Assets.Resources.ClusterSettingsView_Title;
        SettingsService = settingsService;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster);
        DebugContainerImage = ClusterSettings.DebugContainerImage;
    }

    [ObservableProperty]
    public partial ClusterSettings ClusterSettings { get; set; }

    [ObservableProperty]
    public partial string Namespace { get; set; }

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = string.Empty;

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
}



