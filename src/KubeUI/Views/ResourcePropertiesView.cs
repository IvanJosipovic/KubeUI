using k8s.Models;
using k8s;
using KubeUI.ViewModels;

namespace KubeUI.Views;

public sealed class ResourcePropertiesView<T> : MyViewBase<ResourcePropertiesViewModel<T>> where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    protected override StyleGroup? BuildStyles() => [];

    protected override object Build(ResourcePropertiesViewModel<T>? vm)
    {
        if (vm == null) return new StackPanel();

        var sp = new StackPanel()
            .Children([
                new Grid().Cols("*,3*").VerticalAlignment(VerticalAlignment.Top)
                    .Children([
                        new Label().Row(0).Col(0).Content("Name:"),
                        new Label().Row(0).Col(1).Content(@vm.Object.Metadata.Name),
                        ]),
                new Grid().Cols("*,3*").VerticalAlignment(VerticalAlignment.Top)
                    .Children([
                        new Label().Row(1).Col(0).Content("Namespace:"),
                        new Label().Row(1).Col(1).Content(@vm.Object.Metadata.NamespaceProperty),
                        ]),
                ]);

        return sp;
    }
}
