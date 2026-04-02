using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Yaml.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;

namespace KubeUI.Avalonia.Features.Resources.Yaml.Views;

public sealed partial class ResourceYamlView : UserControl
{
    public ResourceYamlView()
    {
        InitializeComponent();

        DesignTimePreview.Run(InitializeDesignTimeDataAsync);
    }

    private async Task InitializeDesignTimeDataAsync()
    {
        var cluster = await DesignTimePreview.CreateClusterAsync<V1Pod>();
        var vm = DesignTimePreview.Get<ResourceYamlViewModel>();

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
    }
}
