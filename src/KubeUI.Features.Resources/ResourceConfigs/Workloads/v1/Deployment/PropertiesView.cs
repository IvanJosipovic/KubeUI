using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.Deployment;

public sealed class PropertiesView : ViewBase<V1Deployment>
{
    protected override object Build(V1Deployment vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Replicas.CurrentValue()!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Available_Replicas.CurrentValue()!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Ready_Replicas.CurrentValue()!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Updated_Replicas.CurrentValue()!)
                    .Value(vm.Status?.UpdatedReplicas ?? 0),
                new ExpandableSection()
                    .Header(L10n.Instance.DeploymentPropertiesView_Rollout.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.DeploymentPropertiesView_Unavailable_Replicas.CurrentValue()!)
                                    .Value(vm.Status?.UnavailableReplicas ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Conditions.CurrentValue()!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.DeploymentPropertiesView_Strategy_Type.CurrentValue()!)
                                    .Value(vm.Spec?.Strategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(L10n.Instance.DeploymentPropertiesView_Progress_Deadline_Seconds.CurrentValue()!)
                                    .Value(vm.Spec?.ProgressDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Revision_History_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0))));
    }
}
