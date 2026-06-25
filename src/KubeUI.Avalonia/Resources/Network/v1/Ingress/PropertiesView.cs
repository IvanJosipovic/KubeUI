using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Network.v1.Ingress;

public sealed class PropertiesView : ViewBase<V1Ingress>
{
    protected override object Build(V1Ingress vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_Ingress_Class!)
                    .Value(vm.Spec?.IngressClassName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Rules!)
                    .Value(vm.Spec?.Rules?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_TLS_Entries!)
                    .Value(vm.Spec?.Tls?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_Load_Balancer_Entries!)
                    .Value(vm.Status?.LoadBalancer?.Ingress?.Count ?? 0));
    }
}
