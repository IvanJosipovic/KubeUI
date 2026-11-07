using k8s.Models;
using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Resources.Workloads.v1.StatefulSet;

public sealed class PropertiesView : ViewBase<V1StatefulSet>
{
    protected override object Build(V1StatefulSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Replicas.CurrentValue()!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Ready_Replicas.CurrentValue()!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Current_Replicas.CurrentValue()!)
                    .Value(vm.Status?.CurrentReplicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Updated_Replicas.CurrentValue()!)
                    .Value(vm.Status?.UpdatedReplicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Available_Replicas.CurrentValue()!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.StatefulSetPropertiesView_Service_Name.CurrentValue()!)
                    .Value(vm.Spec?.ServiceName ?? ""),
                new ExpandableSection()
                    .Header(FoundationLanguageManager.Instance.Shared_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.StatefulSetPropertiesView_Current_Revision.CurrentValue()!)
                                    .Value(vm.Status?.CurrentRevision ?? ""),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.StatefulSetPropertiesView_Update_Revision.CurrentValue()!)
                                    .Value(vm.Status?.UpdateRevision ?? ""),
                                new PropertyItem()
                                    .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(FoundationLanguageManager.Instance.Shared_Configuration.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.StatefulSetPropertiesView_Pod_Management_Policy.CurrentValue()!)
                                    .Value(vm.Spec?.PodManagementPolicy ?? ""),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.StatefulSetPropertiesView_Update_Strategy.CurrentValue()!)
                                    .Value(vm.Spec?.UpdateStrategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(FoundationLanguageManager.Instance.Shared_Revision_History_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(FoundationLanguageManager.Instance.Shared_Min_Ready_Seconds.CurrentValue()!)
                                    .Value(vm.Spec?.MinReadySeconds ?? 0))));
    }
}
