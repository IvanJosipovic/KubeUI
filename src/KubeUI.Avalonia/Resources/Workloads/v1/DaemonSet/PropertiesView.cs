using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet;

public sealed class PropertiesView : ViewBase<V1DaemonSet>
{
    protected override object Build(V1DaemonSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Desired_Scheduled!)
                    .Value(vm.Status?.DesiredNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Current_Scheduled!)
                    .Value(vm.Status?.CurrentNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Ready!)
                    .Value(vm.Status?.NumberReady ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Updated!)
                    .Value(vm.Status?.UpdatedNumberScheduled ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Available!)
                                    .Value(vm.Status?.NumberAvailable ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Unavailable!)
                                    .Value(vm.Status?.NumberUnavailable ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Misscheduled!)
                                    .Value(vm.Status?.NumberMisscheduled ?? 0),
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
                                    .Key(Assets.Resources.Shared_Min_Ready_Seconds!)
                                    .Value(vm.Spec?.MinReadySeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .Value(vm.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Selector_Labels!)
                                    .Value(vm.Spec?.Selector?.MatchLabels?.Count ?? 0))));
    }
}
