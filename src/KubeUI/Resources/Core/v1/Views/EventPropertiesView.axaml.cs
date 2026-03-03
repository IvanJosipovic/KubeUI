using Avalonia.Controls;
using k8s.Models;

namespace KubeUI.Views;

public partial class EventPropertiesView : UserControl
{
    public EventPropertiesView()
    {
        InitializeComponent();
#if DEBUG
        if (Design.IsDesignMode)
        {
            DataContext = new Corev1Event()
            {
                Metadata = new()
                {
                    Name = "event1",
                    NamespaceProperty = "default",
                    CreationTimestamp = DateTime.Now,
                    Labels = new Dictionary<string, string>()
                    {
                        {"test", "testVal" }
                    },
                    Annotations = new Dictionary<string, string>()
                    {
                        { "test1","testval1" }
                    }
                },
                Message = "This is a message\nNewLine\n",
                Source = new()
                {
                    Component = "kubelet",
                    Host = "r720"
                },
                Reason = "My reason",
                FirstTimestamp = DateTime.Now.AddDays(7),
                LastTimestamp = DateTime.Now,
                Count = 1337,
                Type = "Warning",
                InvolvedObject = new()
                {
                    ApiVersion = "v1",
                    Kind = "Pod",
                    NamespaceProperty = "trivy-operator",
                    Name = "trivy-operator-86f4f99bf8-hkrgw",
                    FieldPath = "spec.containers{trivy-operator}"
                }
            };
        }
#endif
    }
}
