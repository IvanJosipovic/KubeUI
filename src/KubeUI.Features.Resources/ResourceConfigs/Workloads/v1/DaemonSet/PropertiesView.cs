using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.DaemonSet;

public sealed class PropertiesView : ViewBase<V1DaemonSet>
{
    protected override object Build(V1DaemonSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.DaemonSetPropertiesView_Desired_Scheduled.CurrentValue()!)
                    .Value(vm.Status?.DesiredNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.DaemonSetPropertiesView_Current_Scheduled.CurrentValue()!)
                    .Value(vm.Status?.CurrentNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.DaemonSetPropertiesView_Ready.CurrentValue()!)
                    .Value(vm.Status?.NumberReady ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.DaemonSetPropertiesView_Updated.CurrentValue()!)
                    .Value(vm.Status?.UpdatedNumberScheduled ?? 0),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.DaemonSetPropertiesView_Available.CurrentValue()!)
                                    .Value(vm.Status?.NumberAvailable ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.DaemonSetPropertiesView_Unavailable.CurrentValue()!)
                                    .Value(vm.Status?.NumberUnavailable ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.DaemonSetPropertiesView_Misscheduled.CurrentValue()!)
                                    .Value(vm.Status?.NumberMisscheduled ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Conditions.CurrentValue()!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Configuration.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Min_Ready_Seconds.CurrentValue()!)
                                    .Value(vm.Spec?.MinReadySeconds ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Revision_History_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Selector_Labels.CurrentValue()!)
                                    .Value(vm.Spec?.Selector?.MatchLabels?.Count ?? 0))));
    }
}
