using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.ResourceQuota;

public sealed class PropertiesView : ViewBase<V1ResourceQuota>
{
    protected override object Build(V1ResourceQuota vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.ResourceQuotaPropertiesView_Spec_Hard.CurrentValue()!)
                    .Value(vm.Spec.Hard.Count),
                new PropertyItem()
                    .Key(L10n.Instance.ResourceQuotaPropertiesView_Spec_Scopes.CurrentValue()!)
                    .Value(vm.Spec.Scopes.Count),
                new PropertyItem()
                    .Key(L10n.Instance.ResourceQuotaPropertiesView_Status_Hard.CurrentValue()!)
                    .Value(vm.Status.Hard.Count),
                new PropertyItem()
                    .Key(L10n.Instance.ResourceQuotaPropertiesView_Status_Used.CurrentValue()!)
                    .Value(vm.Status.Used.Count));
    }
}
