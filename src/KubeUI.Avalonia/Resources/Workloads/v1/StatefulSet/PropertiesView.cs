using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.StatefulSet;

public sealed class PropertiesView : ViewBase<V1StatefulSet>
{
    protected override object Build(V1StatefulSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Replicas!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Current_Replicas!)
                    .Value(vm.Status?.CurrentReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Updated_Replicas!)
                    .Value(vm.Status?.UpdatedReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.StatefulSetPropertiesView_Service_Name!)
                    .Value(vm.Spec?.ServiceName ?? ""),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Current_Revision!)
                                    .Value(vm.Status?.CurrentRevision ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Update_Revision!)
                                    .Value(vm.Status?.UpdateRevision ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Pod_Management_Policy!)
                                    .Value(vm.Spec?.PodManagementPolicy ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.StatefulSetPropertiesView_Update_Strategy!)
                                    .Value(vm.Spec?.UpdateStrategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Min_Ready_Seconds!)
                                    .Value(vm.Spec?.MinReadySeconds ?? 0))));
    }
}
