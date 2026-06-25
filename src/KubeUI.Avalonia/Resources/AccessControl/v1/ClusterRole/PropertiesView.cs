using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.AccessControl.v1.ClusterRole;

public sealed class PropertiesView : ViewBase<V1ClusterRole>
{
    protected override object Build(V1ClusterRole vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Rules)
                    .Value(vm.Rules.Count),
                new PropertyItem()
                    .Key(Assets.Resources.ClusterRolePropertiesView_Aggregation_Selectors)
                    .Value(vm.AggregationRule.ClusterRoleSelectors.Count));
    }
}
