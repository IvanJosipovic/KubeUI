using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Resources.Visualization.Views;

public sealed partial class VisualizationView : UserControl
{
    public VisualizationView()
    {
        InitializeComponent();

        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        DataContext = await DesignTimePreview.CreateClusterBoundViewModelAsync<VisualizationViewModel, V1Pod>();
    }
}



