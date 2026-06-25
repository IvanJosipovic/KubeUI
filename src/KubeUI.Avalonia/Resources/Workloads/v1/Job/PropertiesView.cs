using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Job;

public sealed class PropertiesView : ViewBase<V1Job>
{
    protected override object Build(V1Job vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Completions!)
                    .Value(vm.Spec?.Completions ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Parallelism!)
                    .Value(vm.Spec?.Parallelism ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Succeeded!)
                    .Value(vm.Status?.Succeeded ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.JobPropertiesView_Failed!)
                    .Value(vm.Status?.Failed ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Configuration!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Backoff_Limit!)
                                    .Value(vm.Spec?.BackoffLimit ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Active_Deadline_Seconds!)
                                    .Value(vm.Spec?.ActiveDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Completion_Mode!)
                                    .Value(vm.Spec?.CompletionMode ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Suspend!)
                                    .Value(vm.Spec?.Suspend ?? false))),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Active_Pods!)
                                    .Value(vm.Status?.Active ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Start_Time!)
                                    .Value(vm.Status?.StartTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(Assets.Resources.JobPropertiesView_Completion_Time!)
                                    .Value(vm.Status?.CompletionTime ?? DateTime.MinValue),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .Value(vm.Status?.Conditions?.Count ?? 0))));
    }
}
