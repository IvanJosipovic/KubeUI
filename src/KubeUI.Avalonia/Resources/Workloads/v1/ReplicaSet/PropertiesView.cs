using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Features.Resources.Properties;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet;

public sealed class PropertiesView : ViewBase<ResourcePropertiesViewModel<V1ReplicaSet>>
{
    protected override object Build(ResourcePropertiesViewModel<V1ReplicaSet> vm)
    {
        ArgumentNullException.ThrowIfNull(vm);
        var resource = vm.Object ?? throw new InvalidOperationException("ReplicaSet properties require a resource.");

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Desired_Replicas!)
                    .BindValue(vm, static resource => resource?.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Current_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .BindValue(vm, static resource => resource?.Status?.AvailableReplicas ?? 0),
                new MetricsControl { DataContext = resource },
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.ReplicaSetPropertiesView_Fully_Labeled_Replicas!)
                                    .BindValue(vm, static resource => resource?.Status?.FullyLabeledReplicas ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Observed_Generation!)
                                    .BindValue(vm, static resource => resource?.Status?.ObservedGeneration ?? 0),
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
                                    .Key(Assets.Resources.Shared_Selector_Labels!)
                                    .BindValue(vm, static resource => resource?.Spec?.Selector?.MatchLabels?.Count ?? 0))));
    }
}
