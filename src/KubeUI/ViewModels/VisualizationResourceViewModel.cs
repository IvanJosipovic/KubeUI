using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class VisualizationResourceViewModel : ObservableObject
{
    [ObservableProperty]
    private ICluster _cluster;

    [ObservableProperty]
    private IKubernetesObject<V1ObjectMeta> _object;

    public new IFactory Factory { get; set; } = Application.Current.GetRequiredService<IFactory>();

    public void Initialize(ICluster cluster, IKubernetesObject<V1ObjectMeta> @object)
    {
        Cluster = cluster;
        Object = @object;
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
