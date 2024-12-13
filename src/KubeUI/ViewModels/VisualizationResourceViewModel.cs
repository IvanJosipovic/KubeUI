using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class VisualizationResourceViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ICluster Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta> Object { get; set; }

    [ObservableProperty]
    public partial string IconPath { get; set; }
    public IFactory Factory { get; set; } = Application.Current.GetRequiredService<IFactory>();

    public void Initialize(ICluster cluster, IKubernetesObject<V1ObjectMeta> @object)
    {
        Cluster = cluster;
        Object = @object;
        IconPath = Utilities.GetKubeAssetPath(@object.GetType());
    }

    [RelayCommand]
    private void ViewYaml(IKubernetesObject<V1ObjectMeta> resource)
    {
        var vm = Application.Current.GetRequiredService<ResourceYamlViewModel>();

        vm.Initialize(Cluster, resource);

        Factory.AddToBottom(vm);
    }

    [RelayCommand]
    private void ViewProperties(IKubernetesObject<V1ObjectMeta> resource)
    {
        var propType = typeof(ResourcePropertiesViewModel<>).MakeGenericType(resource.GetType());

        var instance = Application.Current.GetRequiredService(propType) as IDockable;
        instance.CanFloat = false;

        propType.GetMethod(nameof(ResourcePropertiesViewModel<V1Pod>.Initialize)).Invoke(instance, [Cluster, resource]);

        Factory?.AddToRight(instance);
    }
}
