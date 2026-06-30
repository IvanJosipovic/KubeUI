using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.PriorityClass;

public sealed class PropertiesView : ViewBase<V1PriorityClass>
{
    protected override object Build(V1PriorityClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.PriorityClassPropertiesView_Value)
                    .Value(vm.Value),
                new PropertyItem()
                    .Key(Assets.Resources.PriorityClassPropertiesView_Global_Default)
                    .Value(vm.GlobalDefault),
                new PropertyItem()
                    .Key(Assets.Resources.PriorityClassPropertiesView_Preemption_Policy)
                    .Value(vm.PreemptionPolicy),
                new PropertyItem()
                    .Key(Assets.Resources.PriorityClassPropertiesView_Description)
                    .Value(vm.Description));
    }
}
