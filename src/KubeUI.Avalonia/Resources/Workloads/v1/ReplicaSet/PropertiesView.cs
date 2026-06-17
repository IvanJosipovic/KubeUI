using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Workloads.v1.ReplicaSet;

public sealed class PropertiesView : ViewBase<V1ReplicaSet>
{
    protected override object Build(V1ReplicaSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Desired_Replicas!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Current_Replicas!)
                    .Value(vm.Status?.Replicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Ready_Replicas!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Available_Replicas!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                new ExpandableSection()
                    .Header(Assets.Resources.Shared_Status!)
                    .IsExpanded(true)
                    .Content(
                        new StackPanel()
                            .Children(
                                new PropertyItem()
                                    .Key(Assets.Resources.ReplicaSetPropertiesView_Fully_Labeled_Replicas!)
                                    .Value(vm.Status?.FullyLabeledReplicas ?? 0),
                                new PropertyItem()
                                    .Key(Assets.Resources.Shared_Observed_Generation!)
                                    .Value(vm.Status?.ObservedGeneration ?? 0),
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
                                    .Key(Assets.Resources.Shared_Selector_Labels!)
                                    .Value(vm.Spec?.Selector?.MatchLabels?.Count ?? 0))));
    }
}
