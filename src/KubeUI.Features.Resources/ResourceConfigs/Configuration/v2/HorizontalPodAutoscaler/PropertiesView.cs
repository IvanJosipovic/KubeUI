using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v2.HorizontalPodAutoscaler;

public sealed class PropertiesView : ViewBase<V2HorizontalPodAutoscaler>
{
    protected override object Build(V2HorizontalPodAutoscaler vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(L10n.Instance.HorizontalPodAutoscalerPropertiesView_Min_Replicas.CurrentValue()!)
                    .Value(vm.Spec.MinReplicas),
                new PropertyItem()
                    .Key(L10n.Instance.HorizontalPodAutoscalerPropertiesView_Max_Replicas.CurrentValue()!)
                    .Value(vm.Spec.MaxReplicas),
                new PropertyItem()
                    .Key(L10n.Instance.HorizontalPodAutoscalerPropertiesView_Metrics.CurrentValue()!)
                    .Value(vm.Spec.Metrics.Count),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Current_Replicas.CurrentValue()!)
                    .Value(vm.Status.CurrentReplicas),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Desired_Replicas.CurrentValue()!)
                    .Value(vm.Status.DesiredReplicas),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Observed_Generation.CurrentValue()!)
                    .Value(vm.Status.ObservedGeneration));
    }
}
