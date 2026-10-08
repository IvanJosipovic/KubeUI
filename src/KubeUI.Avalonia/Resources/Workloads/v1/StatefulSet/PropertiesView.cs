using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1StatefulSet>>
{
    protected override object Build(ResourcePropertiesViewModel<V1StatefulSet> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("StatefulSet properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Replicas!)
                    .BindValue(vm, static resource => resource?.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Current_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.CurrentReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Updated_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.UpdatedReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.StatefulSetPropertiesView_Service_Name!)
                    .BindValue(vm, static resource => resource?.Spec?.ServiceName ?? ""),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Current_Revision!)
                                    .BindValue(vm, static resource => resource?.Status?.CurrentRevision ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Update_Revision!)
                                    .BindValue(vm, static resource => resource?.Status?.UpdateRevision ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Pod_Management_Policy!)
                                    .BindValue(vm, static resource => resource?.Spec?.PodManagementPolicy ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Update_Strategy!)
                                    .BindValue(vm, static resource => resource?.Spec?.UpdateStrategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .BindValue(vm, static resource => resource?.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Min_Ready_Seconds!)
                                    .BindValue(vm, static resource => resource?.Spec?.MinReadySeconds ?? 0))));
    }
}
