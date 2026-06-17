using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.CronJob;

public sealed class PropertiesView : ViewBase<V1CronJob>
{
    protected override object Build(V1CronJob vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.CronJobPropertiesView_Schedule!)
                    .Value(vm.Spec?.Schedule ?? ""),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Suspend!)
                    .Value(vm.Spec?.Suspend ?? false),
                new PropertyItem()
                    .Key(Assets.Resources.CronJobPropertiesView_Concurrency_Policy!)
                    .Value(vm.Spec?.ConcurrencyPolicy ?? ""),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Successful_History_Limit!)
                                    .Value(vm.Spec?.SuccessfulJobsHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Failed_History_Limit!)
                                    .Value(vm.Spec?.FailedJobsHistoryLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Starting_Deadline_Seconds!)
                                    .Value(vm.Spec?.StartingDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Time_Zone!)
                                    .Value(vm.Spec?.TimeZone ?? ""))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Active_Jobs!)
                                    .Value(vm.Status?.Active?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Last_Schedule_Time!)
                                    .Value(vm.Status?.LastScheduleTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(Assets.Resources.CronJobPropertiesView_Last_Successful_Time!)
                                    .Value(vm.Status?.LastSuccessfulTime ?? DateTime.MinValue))));
    }
}
