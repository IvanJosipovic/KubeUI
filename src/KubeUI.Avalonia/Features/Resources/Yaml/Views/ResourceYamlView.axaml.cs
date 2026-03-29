using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Views;

public sealed partial class ResourceYamlView : UserControl
{
    public ResourceYamlView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                var cluster = Application.Current.GetRequiredService<ClusterWorkspaceCatalog>().GetDefault();
                await cluster.Connect();
                var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

                var obj = new V1Pod()
                {
                    ApiVersion = V1Pod.KubeApiVersion,
                    Kind = V1Pod.KubeKind,
                    Metadata = new()
                    {
                        Name = "test",
                        NamespaceProperty = "default",
                    },
                    Spec = new()
                    {
                        Containers = [
                            new(){
                                Image = "nginx",
                                ImagePullPolicy = "Always"
                            }
                        ]
                    }
                };

                vm.Initialize(cluster, obj);

                DataContext = vm;
            });
        }
#endif
    }
}
