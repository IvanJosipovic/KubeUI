using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Visualization.Views;

public sealed partial class VisualizationView : UserControl
{
    public VisualizationView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();

                cluster.SelectedNamespaces.Add(cluster.GetResource<V1Namespace>(null, "cert-manager"));

                var vm = Application.Current.GetRequiredService<VisualizationViewModel>();

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



