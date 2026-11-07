using k8s.Models;
using KubeUI.Controls.Presentation;

namespace KubeUI.Features.Resources.Workloads.v1.CronJob;

public sealed class PropertiesView : ViewBase<V1CronJob>
{
    protected override object Build(V1CronJob vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Schedule.CurrentValue()!)
                    .Value(vm.Spec?.Schedule ?? ""),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Suspend.CurrentValue()!)
                    .Value(vm.Spec?.Suspend ?? false),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Concurrency_Policy.CurrentValue()!)
                    .Value(vm.Spec?.ConcurrencyPolicy ?? ""),
                new ExpandableSection()
                    .Header(FoundationLanguageManager.Instance.Shared_Configuration.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Successful_History_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.SuccessfulJobsHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Failed_History_Limit.CurrentValue()!)
                                    .Value(vm.Spec?.FailedJobsHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Starting_Deadline_Seconds.CurrentValue()!)
                                    .Value(vm.Spec?.StartingDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Time_Zone.CurrentValue()!)
                                    .Value(vm.Spec?.TimeZone ?? ""))),
                new ExpandableSection()
                    .Header(FoundationLanguageManager.Instance.Shared_Status.ToBinding())
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Active_Jobs.CurrentValue()!)
                                    .Value(vm.Status?.Active?.Count ?? 0),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Last_Schedule_Time.CurrentValue()!)
                                    .Value(vm.Status?.LastScheduleTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(ResourcesLanguageManager.Instance.CronJobPropertiesView_Last_Successful_Time.CurrentValue()!)
                                    .Value(vm.Status?.LastSuccessfulTime ?? DateTime.MinValue))));
    }
}
