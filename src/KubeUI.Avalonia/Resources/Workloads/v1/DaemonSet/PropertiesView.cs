using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.DaemonSet;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1DaemonSet>>
{
    protected override object Build(ResourcePropertiesViewModel<V1DaemonSet> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("DaemonSet properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Desired_Scheduled!)
                    .BindValue(vm, static resource => resource?.Status?.DesiredNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Current_Scheduled!)
                    .BindValue(vm, static resource => resource?.Status?.CurrentNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Ready!)
                    .BindValue(vm, static resource => resource?.Status?.NumberReady ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.DaemonSetPropertiesView_Updated!)
                    .BindValue(vm, static resource => resource?.Status?.UpdatedNumberScheduled ?? 0),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Available!)
                                    .BindValue(vm, static resource => resource?.Status?.NumberAvailable ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Unavailable!)
                                    .BindValue(vm, static resource => resource?.Status?.NumberUnavailable ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DaemonSetPropertiesView_Misscheduled!)
                                    .BindValue(vm, static resource => resource?.Status?.NumberMisscheduled ?? 0),
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
                                    .Key(Assets.Resources.Shared_Min_Ready_Seconds!)
                                    .BindValue(vm, static resource => resource?.Spec?.MinReadySeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .BindValue(vm, static resource => resource?.Spec?.RevisionHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Selector_Labels!)
                                    .BindValue(vm, static resource => resource?.Spec?.Selector?.MatchLabels?.Count ?? 0))));
    }
}
