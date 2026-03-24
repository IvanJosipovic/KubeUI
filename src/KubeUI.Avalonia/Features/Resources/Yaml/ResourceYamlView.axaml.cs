using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Views;

public sealed partial class ResourceYamlView : UserControl
{
    public ResourceYamlView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
            cluster.Connect().GetAwaiter().GetResult();
            var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

            var obj = new V1Namespace()
            {
                Metadata = new()
                {
                    Name = "test"
                }
            };

            vm.Initialize(cluster, obj);

            DataContext = vm;
        }
#endif
    }
}
