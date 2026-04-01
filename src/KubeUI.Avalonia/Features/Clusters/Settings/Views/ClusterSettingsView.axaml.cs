using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Settings.Views;

public sealed partial class ClusterSettingsView : UserControl
{
    public ClusterSettingsView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode && DataContext == null)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();

                var vm = Application.Current.GetRequiredService<ClusterSettingsViewModel>();

                if (vm is IInitializeCluster init)
                {
                    init.Initialize(cluster);
                }

                DataContext = vm;
            });
        }
#endif
    }
}



