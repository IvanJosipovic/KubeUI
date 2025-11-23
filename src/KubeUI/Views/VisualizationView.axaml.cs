using Dock.Model.Core;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Views;

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
                var cluster = Application.Current.GetRequiredService<ClusterManager>().GetDefault();
                await cluster.Connect();

                cluster.SelectedNamespaces.Add(cluster.Namespaces.FirstOrDefault(x => x.Name() == "cert-manager"));

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
