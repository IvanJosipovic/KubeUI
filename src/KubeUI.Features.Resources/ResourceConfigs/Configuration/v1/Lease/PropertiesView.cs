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
                    .Value(vm.Spec.LeaseDurationSeconds),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Acquire_Time.CurrentValue()!)
                    .Value(vm.Spec.AcquireTime),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Renew_Time.CurrentValue()!)
                    .Value(vm.Spec.RenewTime),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.LeasePropertiesView_Lease_Transitions.CurrentValue()!)
                    .Value(vm.Spec.LeaseTransitions));
    }
}
