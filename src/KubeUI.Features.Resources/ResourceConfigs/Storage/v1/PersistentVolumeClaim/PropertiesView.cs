using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.Storage.v1.PersistentVolumeClaim;

public sealed class PropertiesView : ViewBase<V1PersistentVolumeClaim>
{
    protected override object Build(V1PersistentVolumeClaim vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(new MetricsControl { DataContext = vm },
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Phase.CurrentValue()!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Storage_Class.CurrentValue()!)
                    .Value(vm.Spec?.StorageClassName ?? ""),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PersistentVolumeClaimPropertiesView_Volume_Name.CurrentValue()!)
                    .Value(vm.Spec?.VolumeName ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Access_Modes.CurrentValue()!)
                    .Value(vm.Spec?.AccessModes?.Count ?? 0),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Configuration.ToBinding(),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Volume_Mode.CurrentValue()!)
                        .Value(vm.Spec?.VolumeMode ?? ""),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.PersistentVolumeClaimPropertiesView_Requested_Storage.CurrentValue()!)
                        .Value(vm.Spec?.Resources?.Requests?.Count ?? 0)),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Status.ToBinding(),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Capacity_Entries.CurrentValue()!)
                        .Value(vm.Status?.Capacity?.Count ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                        .Value(vm.Status?.Conditions?.Count ?? 0)));
    }
}
