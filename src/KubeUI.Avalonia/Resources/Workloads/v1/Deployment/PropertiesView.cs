using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Deployment;

public sealed class PropertiesView : ViewBase<V1Deployment>
{
    protected override object Build(V1Deployment vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Replicas!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Updated_Replicas!)
                    .Value(vm.Status?.UpdatedReplicas ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.DeploymentPropertiesView_Rollout!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Unavailable_Replicas!)
                                    .Value(vm.Status?.UnavailableReplicas ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Strategy_Type!)
                                    .Value(vm.Spec?.Strategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Progress_Deadline_Seconds!)
                                    .Value(vm.Spec?.ProgressDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0))));
    }
}
