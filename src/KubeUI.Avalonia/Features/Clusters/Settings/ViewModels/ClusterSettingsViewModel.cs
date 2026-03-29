using KubeUI.Kubernetes;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;

namespace KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
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
}



