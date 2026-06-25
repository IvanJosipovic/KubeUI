using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ConfigMap;

public sealed class PropertiesView : ViewBase<V1ConfigMap>
{
    protected override object Build(V1ConfigMap vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.ConfigMapPropertiesView_Binary_Data)
                    .Value(vm.BinaryData?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Data)
                    .Value(vm.Data?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.ConfigMapPropertiesView_Immutable)
                    .Value(vm.Immutable));
    }
}
