using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Network.v1.Ingress;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Ingress>>
{
    protected override object Build(ResourcePropertiesViewModel<V1Ingress> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Ingress properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_Ingress_Class!)
                    .BindValue(vm, static resource => resource?.Spec?.IngressClassName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Rules!)
                    .BindValue(vm, static resource => resource?.Spec?.Rules?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_TLS_Entries!)
                    .BindValue(vm, static resource => resource?.Spec?.Tls?.Count ?? 0),
                new MetricsControl { DataContext = resource },
                new PropertyItem()
                    .Key(Assets.Resources.IngressPropertiesView_Load_Balancer_Entries!)
                    .BindValue(vm, static resource => resource?.Status?.LoadBalancer?.Ingress?.Count ?? 0));
    }
}
