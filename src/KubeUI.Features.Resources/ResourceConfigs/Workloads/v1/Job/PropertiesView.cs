using k8s.Models;
using KubeUI.Features.Resources.Metrics.Controls;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.Job;

public sealed class PropertiesView : ViewBase<V1Job>
{
    protected override object Build(V1Job vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(new MetricsControl { DataContext = vm },
                new PropertyItem()
                    .Key(L10n.Instance.JobPropertiesView_Completions.CurrentValue()!)
                    .Value(vm.Spec?.Completions ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.JobPropertiesView_Parallelism.CurrentValue()!)
                    .Value(vm.Spec?.Parallelism ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.JobPropertiesView_Succeeded.CurrentValue()!)
                    .Value(vm.Status?.Succeeded ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.JobPropertiesView_Failed.CurrentValue()!)
                    .Value(vm.Status?.Failed ?? 0),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Configuration.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Backoff_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.BackoffLimit ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Active_Deadline_Seconds.CurrentValue()!)
                                    .Value(vm.Spec?.ActiveDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Completion_Mode.CurrentValue()!)
                                    .Value(vm.Spec?.CompletionMode ?? ""),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Suspend.CurrentValue()!)
                                    .Value(vm.Spec?.Suspend ?? false))),
                new ExpandableSection()
                    .Header(SharedL10n.Instance.Shared_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Active_Pods.CurrentValue()!)
                                    .Value(vm.Status?.Active ?? 0),
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Start_Time.CurrentValue()!)
                                    .Value(vm.Status?.StartTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(L10n.Instance.JobPropertiesView_Completion_Time.CurrentValue()!)
                                    .Value(vm.Status?.CompletionTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(SharedL10n.Instance.Shared_Conditions.CurrentValue()!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))));
    }
}
