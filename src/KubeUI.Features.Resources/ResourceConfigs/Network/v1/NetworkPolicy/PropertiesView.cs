using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Network.v1.NetworkPolicy;

public sealed class PropertiesView : ViewBase<V1NetworkPolicy>
{
    protected override object Build(V1NetworkPolicy vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.NetworkPolicyPropertiesView_Pod_Selector_Labels.CurrentValue()!)
                    .Value(vm.Spec?.PodSelector?.MatchLabels?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.NetworkPolicyPropertiesView_Policy_Types.CurrentValue()!)
                    .Value(vm.Spec?.PolicyTypes?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.NetworkPolicyPropertiesView_Ingress_Rules.CurrentValue()!)
                    .Value(vm.Spec?.Ingress?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.NetworkPolicyPropertiesView_Egress_Rules.CurrentValue()!)
                    .Value(vm.Spec?.Egress?.Count ?? 0));
    }
}
