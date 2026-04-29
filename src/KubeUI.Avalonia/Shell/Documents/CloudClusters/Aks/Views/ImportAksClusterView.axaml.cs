using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.ViewModels;

namespace KubeUI.Avalonia.Shell.Documents.CloudClusters.Aks.Views;

public partial class ImportAksClusterView : UserControl
{
    public ImportAksClusterView()
    {
        InitializeComponent();

        if (DataContext == null)
        {
            DesignTimePreview.Run(InitializeDesignTimeDataAsync);
        }
    }

    private Task InitializeDesignTimeDataAsync()
    {
        DataContext = DesignTimePreview.Get<ImportAksClusterViewModel>();
        return Task.CompletedTask;
    }
}
