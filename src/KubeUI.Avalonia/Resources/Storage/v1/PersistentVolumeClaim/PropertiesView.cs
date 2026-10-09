using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Storage.v1.PersistentVolumeClaim;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1PersistentVolumeClaim>>
{
    protected override object Build(ResourcePropertiesViewModel<V1PersistentVolumeClaim> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Persistent volume claim properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Phase!)
                    .BindValue(vm, static resource => resource?.Status?.Phase ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Storage_Class!)
                    .BindValue(vm, static resource => resource?.Spec?.StorageClassName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.PersistentVolumeClaimPropertiesView_Volume_Name!)
                    .BindValue(vm, static resource => resource?.Spec?.VolumeName ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Access_Modes!)
                    .BindValue(vm, static resource => resource?.Spec?.AccessModes?.Count ?? 0),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Volume_Mode!)
                                    .BindValue(vm, static resource => resource?.Spec?.VolumeMode ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.PersistentVolumeClaimPropertiesView_Requested_Storage!)
                                    .BindValue(vm, static resource => resource?.Spec?.Resources?.Requests?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Capacity_Entries!)
                                    .BindValue(vm, static resource => resource?.Status?.Capacity?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0))));
    }
}
