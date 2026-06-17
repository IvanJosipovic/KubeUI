using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.ResourceQuota;

public sealed class PropertiesView : ViewBase<V1ResourceQuota>
{
    protected override object Build(V1ResourceQuota vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.ResourceQuotaPropertiesView_Spec_Hard)
                    .Value(vm.Spec.Hard.Count),
                new PropertyItem()
                    .Key(Assets.Resources.ResourceQuotaPropertiesView_Spec_Scopes)
                    .Value(vm.Spec.Scopes.Count),
                new PropertyItem()
                    .Key(Assets.Resources.ResourceQuotaPropertiesView_Status_Hard)
                    .Value(vm.Status.Hard.Count),
                new PropertyItem()
                    .Key(Assets.Resources.ResourceQuotaPropertiesView_Status_Used)
                    .Value(vm.Status.Used.Count));
    }
}
