using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Features.Resources.Properties;

namespace KubeUI.Features.Resources.Workloads.v1.ReplicaSet;

public sealed class PropertiesView : ViewBase<V1ReplicaSet>
{
    protected override object Build(V1ReplicaSet vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Desired_Replicas.CurrentValue()!)
                    .Value(vm.Spec?.Replicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Current_Replicas.CurrentValue()!)
                    .Value(vm.Status?.Replicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Ready_Replicas.CurrentValue()!)
                    .Value(vm.Status?.ReadyReplicas ?? 0),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Available_Replicas.CurrentValue()!)
                    .Value(vm.Status?.AvailableReplicas ?? 0),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Status.ToBinding(),
                    new PropertyItem()
                        .Key(ResourcesLanguageManager.Instance.ReplicaSetPropertiesView_Fully_Labeled_Replicas.CurrentValue()!)
                        .Value(vm.Status?.FullyLabeledReplicas ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Observed_Generation.CurrentValue()!)
                        .Value(vm.Status?.ObservedGeneration ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Conditions.CurrentValue()!)
                        .Value(vm.Status?.Conditions?.Count ?? 0)),
                ResourcePropertiesSection.Create(
                    FoundationLanguageManager.Instance.Shared_Configuration.ToBinding(),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Min_Ready_Seconds.CurrentValue()!)
                        .Value(vm.Spec?.MinReadySeconds ?? 0),
                    new PropertyItem()
                        .Key(FoundationLanguageManager.Instance.Shared_Selector_Labels.CurrentValue()!)
                        .Value(vm.Spec?.Selector?.MatchLabels?.Count ?? 0)));
    }
}
