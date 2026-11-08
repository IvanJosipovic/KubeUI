using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.Workloads.v1.DaemonSet;

public sealed class PropertiesView : ViewBase<V1DaemonSet>
{
    protected override object Build(V1DaemonSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(new MetricsControl { DataContext = vm },
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Desired_Scheduled.CurrentValue()!)
                    .Value(vm.Status?.DesiredNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Current_Scheduled.CurrentValue()!)
                    .Value(vm.Status?.CurrentNumberScheduled ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Ready.CurrentValue()!)
                    .Value(vm.Status?.NumberReady ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Updated.CurrentValue()!)
                    .Value(vm.Status?.UpdatedNumberScheduled ?? 0),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Status,
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Available.CurrentValue()!)
                        .Value(vm.Status?.NumberAvailable ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Unavailable.CurrentValue()!)
                        .Value(vm.Status?.NumberUnavailable ?? 0),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.DaemonSetPropertiesView_Misscheduled.CurrentValue()!)
                        .Value(vm.Status?.NumberMisscheduled ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                        .Value(vm.Status?.Conditions?.Count ?? 0)),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Configuration,
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Min_Ready_Seconds.CurrentValue()!)
                        .Value(vm.Spec?.MinReadySeconds ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Revision_History_Limit.CurrentValue()!)
                        .Value(vm.Spec?.RevisionHistoryLimit ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Selector_Labels.CurrentValue()!)
                        .Value(vm.Spec?.Selector?.MatchLabels?.Count ?? 0)));
    }
}
