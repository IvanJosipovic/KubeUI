using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Storage.v1.StorageClass;

public sealed class PropertiesView : ViewBase<V1StorageClass>
{
    protected override object Build(V1StorageClass vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.StorageClassPropertiesView_Provisioner.CurrentValue()!)
                    .Value(vm.Provisioner ?? ""),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Reclaim_Policy.CurrentValue()!)
                    .Value(vm.ReclaimPolicy ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.StorageClassPropertiesView_Volume_Binding_Mode.CurrentValue()!)
                    .Value(vm.VolumeBindingMode ?? ""),
                new PropertyItem()
                    .Key(L10n.Instance.StorageClassPropertiesView_Allow_Volume_Expansion.CurrentValue()!)
                    .Value(vm.AllowVolumeExpansion ?? false),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Mount_Options.CurrentValue()!)
                    .Value(vm.MountOptions?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.StorageClassPropertiesView_Parameters.CurrentValue()!)
                    .Value(vm.Parameters?.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.StorageClassPropertiesView_Allowed_Topologies.CurrentValue()!)
                    .Value(vm.AllowedTopologies?.Count ?? 0));
    }
}
