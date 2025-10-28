using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using k8s.Models;

namespace KubeUI;

public partial class V1PodProperties : UserControl
{
    public V1PodProperties()
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
                    Containers = [
                        new(){
                            Image = "test"
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
