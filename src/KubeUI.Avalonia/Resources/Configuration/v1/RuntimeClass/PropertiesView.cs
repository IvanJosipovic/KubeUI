using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.RuntimeClass;

public sealed class PropertiesView : ViewBase<V1RuntimeClass>
{
    protected override object Build(V1RuntimeClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.RuntimeClassPropertiesView_Handler)
                    .Value(vm.Handler),
                new PropertyItem()
                    .Key(Assets.Resources.RuntimeClassPropertiesView_Overhead_Entries)
                    .Value(vm.Overhead?.PodFixed.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.RuntimeClassPropertiesView_Node_Selector_Entries)
                    .Value(vm.Scheduling?.NodeSelector.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Tolerations)
                    .Value(vm.Scheduling?.Tolerations.Count ?? 0));
    }
}
