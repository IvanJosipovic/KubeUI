using Avalonia.Markup.Declarative;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;

namespace KubeUI.Avalonia.Resources.Configuration.v1.PodDisruptionBudget;

public sealed class PropertiesView : ViewBase<V1PodDisruptionBudget>
{
    protected override object Build(V1PodDisruptionBudget vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Min_Available)
                    .Value(vm.Spec.MinAvailable),
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Max_Unavailable)
                    .Value(vm.Spec.MaxUnavailable),
                new PropertyItem()
                    .Key(Assets.Resources.Shared_Selector_Labels)
                    .Value(vm.Spec.Selector?.MatchLabels.Count ?? 0),
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Disruptions_Allowed)
                    .Value(vm.Status.DisruptionsAllowed),
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Current_Healthy)
                    .Value(vm.Status.CurrentHealthy),
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Desired_Healthy)
                    .Value(vm.Status.DesiredHealthy),
                new PropertyItem()
                    .Key(Assets.Resources.PodDisruptionBudgetPropertiesView_Expected_Pods)
                    .Value(vm.Status.ExpectedPods));
    }
}
