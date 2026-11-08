using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Network.v1.Ingress;

public sealed class PropertiesView : ViewBase<V1Ingress>
{
    protected override object Build(V1Ingress vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.IngressPropertiesView_Ingress_Class.CurrentValue()!)
                    .Value(vm.Spec?.IngressClassName ?? ""),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Rules.CurrentValue()!)
                    .Value(vm.Spec?.Rules?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.IngressPropertiesView_TLS_Entries.CurrentValue()!)
                    .Value(vm.Spec?.Tls?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.IngressPropertiesView_Load_Balancer_Entries.CurrentValue()!)
                    .Value(vm.Status?.LoadBalancer?.Ingress?.Count ?? 0));
    }
}
