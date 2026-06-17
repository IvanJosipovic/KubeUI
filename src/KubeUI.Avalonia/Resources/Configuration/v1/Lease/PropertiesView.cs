using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.Lease;

public sealed class PropertiesView : ViewBase<V1Lease>
{
    protected override object Build(V1Lease vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.LeasePropertiesView_Holder_Identity)
                    .Value(vm.Spec.HolderIdentity),
                new PropertyItem()
                    .Key(Assets.Resources.LeasePropertiesView_Lease_Duration_Seconds)
                    .Value(vm.Spec.LeaseDurationSeconds),
                new PropertyItem()
                    .Key(Assets.Resources.LeasePropertiesView_Acquire_Time)
                    .Value(vm.Spec.AcquireTime),
                new PropertyItem()
                    .Key(Assets.Resources.LeasePropertiesView_Renew_Time)
                    .Value(vm.Spec.RenewTime),
                new PropertyItem()
                    .Key(Assets.Resources.LeasePropertiesView_Lease_Transitions)
                    .Value(vm.Spec.LeaseTransitions));
    }
}
