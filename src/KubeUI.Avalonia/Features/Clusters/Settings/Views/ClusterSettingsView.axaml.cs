using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Clusters.Settings.Views;

public sealed partial class ClusterSettingsView : UserControl
{
    public ClusterSettingsView()
    {
        InitializeComponent();

        if (DataContext == null)
        {
            DesignTimePreview.Run(InitializeDesignTimeDataAsync);
        }
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<ClusterSettingsViewModel, V1Pod>();
    }
}



