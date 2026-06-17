using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v2.HorizontalPodAutoscaler;

public sealed class PropertiesView : ViewBase<V2HorizontalPodAutoscaler>
{
    protected override object Build(V2HorizontalPodAutoscaler vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.HorizontalPodAutoscalerPropertiesView_Min_Replicas)
                    .Value(vm.Spec.MinReplicas),
                new PropertyItem()
                    .Key(Assets.Resources.HorizontalPodAutoscalerPropertiesView_Max_Replicas)
                    .Value(vm.Spec.MaxReplicas),
                new PropertyItem()
                    .Key(Assets.Resources.HorizontalPodAutoscalerPropertiesView_Metrics)
                    .Value(vm.Spec.Metrics.Count),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Current_Replicas)
                    .Value(vm.Status.CurrentReplicas),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Desired_Replicas)
                    .Value(vm.Status.DesiredReplicas),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Observed_Generation)
                    .Value(vm.Status.ObservedGeneration));
    }
}
