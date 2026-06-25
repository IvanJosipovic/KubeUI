using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Storage.v1.PersistentVolume;

public sealed class PropertiesView : ViewBase<V1PersistentVolume>
{
    protected override object Build(V1PersistentVolume vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Phase!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Reason!)
                    .Value(vm.Status?.Reason ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Storage_Class!)
                    .Value(vm.Spec?.StorageClassName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Reclaim_Policy!)
                    .Value(vm.Spec?.PersistentVolumeReclaimPolicy ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Volume_Mode!)
                    .Value(vm.Spec?.VolumeMode ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Access_Modes!)
                    .Value(vm.Spec?.AccessModes?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Capacity_Entries!)
                    .Value(vm.Spec?.Capacity?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Mount_Options!)
                    .Value(vm.Spec?.MountOptions?.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.PersistentVolumePropertiesView_Claim_Name!)
                    .Value(vm.Spec?.ClaimRef?.Name ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PersistentVolumePropertiesView_Last_Phase_Transition!)
                    .Value(vm.Status?.LastPhaseTransitionTime ?? DateTime.MinValue));
    }
}
