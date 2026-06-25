using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim;

public sealed class PropertiesView : ViewBase<V1PersistentVolumeClaim>
{
    protected override object Build(V1PersistentVolumeClaim vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Phase!)
                    .Value(vm.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Storage_Class!)
                    .Value(vm.Spec?.StorageClassName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PersistentVolumeClaimPropertiesView_Volume_Name!)
                    .Value(vm.Spec?.VolumeName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Access_Modes!)
                    .Value(vm.Spec?.AccessModes?.Count ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Volume_Mode!)
                                    .Value(vm.Spec?.VolumeMode ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.PersistentVolumeClaimPropertiesView_Requested_Storage!)
                                    .Value(vm.Spec?.Resources?.Requests?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Capacity_Entries!)
                                    .Value(vm.Status?.Capacity?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))));
    }
}
