using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Storage.v1.PersistentVolume;

public sealed class PropertiesView : ViewBase<V1PersistentVolume>
{
    protected override object Build(V1PersistentVolume vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Phase.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Reason.CurrentValue()!)
                    .Value(vm.Status?.Reason ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Storage_Class.CurrentValue()!)
                    .Value(vm.Spec?.StorageClassName ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Reclaim_Policy.CurrentValue()!)
                    .Value(vm.Spec?.PersistentVolumeReclaimPolicy ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Volume_Mode.CurrentValue()!)
                    .Value(vm.Spec?.VolumeMode ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Access_Modes.CurrentValue()!)
                    .Value(vm.Spec?.AccessModes?.Count ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Capacity_Entries.CurrentValue()!)
                    .Value(vm.Spec?.Capacity?.Count ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Mount_Options.CurrentValue()!)
                    .Value(vm.Spec?.MountOptions?.Count ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PersistentVolumePropertiesView_Claim_Name.CurrentValue()!)
                    .Value(vm.Spec?.ClaimRef?.Name ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PersistentVolumePropertiesView_Last_Phase_Transition.CurrentValue()!)
                    .Value(vm.Status?.LastPhaseTransitionTime ?? DateTime.MinValue));
    }
}
