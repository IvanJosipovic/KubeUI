using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;

namespace KubeUI.Features.Resources.Configuration.v1.PodDisruptionBudget;

public sealed class PropertiesView : ViewBase<V1PodDisruptionBudget>
{
    protected override object Build(V1PodDisruptionBudget vm)
    {
        ArgumentNullException.ThrowIfNull(vm);

        return new StackPanel()
            .Children(
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Min_Available.CurrentValue()!)
                    .Value(vm.Spec.MinAvailable),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Max_Unavailable.CurrentValue()!)
                    .Value(vm.Spec.MaxUnavailable),
                new PropertyItem()
                    .Key(FoundationLanguageManager.Instance.Shared_Selector_Labels.CurrentValue()!)
                    .Value(vm.Spec.Selector?.MatchLabels.Count ?? 0),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Disruptions_Allowed.CurrentValue()!)
                    .Value((object?)vm.Status.DisruptionsAllowed ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Current_Healthy.CurrentValue()!)
                    .Value((object?)vm.Status.CurrentHealthy ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Desired_Healthy.CurrentValue()!)
                    .Value((object?)vm.Status.DesiredHealthy ?? string.Empty),
                new PropertyItem()
                    .Key(ResourcesLanguageManager.Instance.PodDisruptionBudgetPropertiesView_Expected_Pods.CurrentValue()!)
                    .Value((object?)vm.Status.ExpectedPods ?? string.Empty));
    }
}
