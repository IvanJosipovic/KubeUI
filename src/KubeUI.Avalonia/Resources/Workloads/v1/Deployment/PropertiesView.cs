using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Deployment;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1Deployment>>
{
    protected override object Build(ResourcePropertiesViewModel<V1Deployment> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("Deployment properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Replicas!)
                    .BindValue(vm, static resource => resource?.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.AvailableReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Updated_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.UpdatedReplicas ?? 0),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.DeploymentPropertiesView_Rollout!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Unavailable_Replicas!)
                                    .BindValue(vm, static resource => resource?.Status?.UnavailableReplicas ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Conditions!)
                                    .BindValue(vm, static resource => resource?.Status?.Conditions?.Count ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Strategy_Type!)
                                    .BindValue(vm, static resource => resource?.Spec?.Strategy?.Type ?? ""),
                                new PropertyItem()
                                    .Key(Assets.Resources.DeploymentPropertiesView_Progress_Deadline_Seconds!)
                                    .BindValue(vm, static resource => resource?.Spec?.ProgressDeadlineSeconds ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Revision_History_Limit!)
                                    .BindValue(vm, static resource => resource?.Spec?.RevisionHistoryLimit ?? 0))));
    }
}
