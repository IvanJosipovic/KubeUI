using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.AccessControl.v1.ClusterRole;

public sealed class PropertiesView : ViewBase<V1ClusterRole>
{
    protected override object Build(V1ClusterRole vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Rules.CurrentValue()!)
                    .Value(vm.Rules.Count),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.ClusterRolePropertiesView_Aggregation_Selectors.CurrentValue()!)
                    .Value(vm.AggregationRule.ClusterRoleSelectors.Count));
    }
}
