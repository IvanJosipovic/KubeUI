using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Job;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Job>>
{
    protected override object Build(ResourcePropertiesViewModel<V1Job> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Job properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Completions!)
                    .BindValue(vm, static resource => resource?.Spec?.Completions ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Parallelism!)
                    .BindValue(vm, static resource => resource?.Spec?.Parallelism ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Succeeded!)
                    .BindValue(vm, static resource => resource?.Status?.Succeeded ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Failed!)
                    .BindValue(vm, static resource => resource?.Status?.Failed ?? 0),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Backoff_Limit!)
                                    .BindValue(vm, static resource => resource?.Spec?.BackoffLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Active_Deadline_Seconds!)
                                    .BindValue(vm, static resource => resource?.Spec?.ActiveDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Completion_Mode!)
                                    .BindValue(vm, static resource => resource?.Spec?.CompletionMode ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Suspend!)
                                    .BindValue(vm, static resource => resource?.Spec?.Suspend ?? false))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Active_Pods!)
                                    .BindValue(vm, static resource => resource?.Status?.Active ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Start_Time!)
                                    .BindValue(vm, static resource => resource?.Status?.StartTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Completion_Time!)
                                    .BindValue(vm, static resource => resource?.Status?.CompletionTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0))));
    }
}
