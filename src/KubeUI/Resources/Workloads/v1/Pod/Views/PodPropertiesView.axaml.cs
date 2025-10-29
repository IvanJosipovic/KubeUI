using k8s.Models;

namespace KubeUI.Resources.Workloads.v1.Pod.Views;

public partial class PodPropertiesView : UserControl
{
    public PodPropertiesView()
    {
        InitializeComponent();
#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new V1Pod()
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new()
                {
                    Name = "testSecret",
                    NamespaceProperty = "default",
                    OwnerReferences = [
                        new(){
                            Controller = true,
                            Name = "the-controller"
                        }
                    ]
                },
                Spec = new()
                {
                    InitContainers = [
                        new(){
                            Name = "init1",
                            Image = "testinit:latest",
                            ImagePullPolicy = "Always"
                        }
                    ],
                    Containers = [
                        new(){
                            Name = "cont1",
                            Image = "testcont:v1",
                            ImagePullPolicy = "Never"
                        }
                    ],
                    NodeName = "r720"
                },
                Status = new()
                {
                    Phase = "Pending",
                    PodIP = "12.21.33.11"
                }
            };
        }
#endif
    }
}
