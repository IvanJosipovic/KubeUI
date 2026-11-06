using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.RuntimeClass;

public sealed class PropertiesView : ViewBase<V1RuntimeClass>
{
    protected override object Build(V1RuntimeClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.RuntimeClassPropertiesView_Handler.CurrentValue()!)
                    .Value(vm.Handler),
                new PropertyItem()
                    .Key(L10n.Instance.RuntimeClassPropertiesView_Overhead_Entries.CurrentValue()!)
                    .Value(vm.Overhead?.PodFixed.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.RuntimeClassPropertiesView_Node_Selector_Entries.CurrentValue()!)
                    .Value(vm.Scheduling?.NodeSelector.Count ?? 0),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Tolerations.CurrentValue()!)
                    .Value(vm.Scheduling?.Tolerations.Count ?? 0));
    }
}
