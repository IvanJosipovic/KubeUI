using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Network.v1.NetworkPolicy;

public sealed class PropertiesView : ViewBase<V1NetworkPolicy>
{
    protected override object Build(V1NetworkPolicy vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.NetworkPolicyPropertiesView_Pod_Selector_Labels!)
                    .Value(vm.Spec?.PodSelector?.MatchLabels?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.NetworkPolicyPropertiesView_Policy_Types!)
                    .Value(vm.Spec?.PolicyTypes?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.NetworkPolicyPropertiesView_Ingress_Rules!)
                    .Value(vm.Spec?.Ingress?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.NetworkPolicyPropertiesView_Egress_Rules!)
                    .Value(vm.Spec?.Egress?.Count ?? 0));
    }
}
