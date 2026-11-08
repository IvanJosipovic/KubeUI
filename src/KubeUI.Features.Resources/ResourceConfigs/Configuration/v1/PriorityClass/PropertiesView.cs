using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.PriorityClass;

public sealed class PropertiesView : ViewBase<V1PriorityClass>
{
    protected override object Build(V1PriorityClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PriorityClassPropertiesView_Value.CurrentValue()!)
                    .Value(vm.Value),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PriorityClassPropertiesView_Global_Default.CurrentValue()!)
                    .Value(vm.GlobalDefault),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PriorityClassPropertiesView_Preemption_Policy.CurrentValue()!)
                    .Value(vm.PreemptionPolicy),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PriorityClassPropertiesView_Description.CurrentValue()!)
                    .Value(vm.Description));
    }
}
