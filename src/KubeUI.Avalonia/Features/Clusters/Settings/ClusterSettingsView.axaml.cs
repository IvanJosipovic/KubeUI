using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Views;

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



