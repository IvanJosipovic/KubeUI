using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Core.v1.Namespace;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Namespace>>
{
    protected override object Build(ResourcePropertiesViewModel<V1Namespace> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Namespace properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Phase!)
                    .BindValue(vm, static resource => resource?.Status?.Phase ?? ""),
                new MetricsControl { DataContext = resource },
                new PropertyItem()
                    .Key(Assets.Resources.NamespacePropertiesView_Finalizers!)
                    .BindValue(vm, static resource => resource?.Spec?.Finalizers?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Conditions!)
                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0));
    }
}
