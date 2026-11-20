using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.Lease;

public sealed class PropertiesView : ViewBase<V1Lease>
{
    protected override object Build(V1Lease vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Holder_Identity.CurrentValue()!)
                    .Value(vm.Spec.HolderIdentity),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Lease_Duration_Seconds.CurrentValue()!)
                    .Value((object?)vm.Spec.LeaseDurationSeconds ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Acquire_Time.CurrentValue()!)
                    .Value((object?)vm.Spec.AcquireTime ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Renew_Time.CurrentValue()!)
                    .Value((object?)vm.Spec.RenewTime ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Lease_Transitions.CurrentValue()!)
                    .Value((object?)vm.Spec.LeaseTransitions ?? string.Empty));
    }
}
