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
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Min_Available.CurrentValue()!)
                    .Value(vm.Spec.MinAvailable),
                new PropertyItem()
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Max_Unavailable.CurrentValue()!)
                    .Value(vm.Spec.MaxUnavailable),
                new PropertyItem()
                    .Key(SharedL10n.Instance.Shared_Selector_Labels.CurrentValue()!)
                    .Value(vm.Spec.Selector?.MatchLabels.Count ?? 0),
                new PropertyItem()
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Disruptions_Allowed.CurrentValue()!)
                    .Value(vm.Status.DisruptionsAllowed),
                new PropertyItem()
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Current_Healthy.CurrentValue()!)
                    .Value(vm.Status.CurrentHealthy),
                new PropertyItem()
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Desired_Healthy.CurrentValue()!)
                    .Value(vm.Status.DesiredHealthy),
                new PropertyItem()
                    .Key(L10n.Instance.PodDisruptionBudgetPropertiesView_Expected_Pods.CurrentValue()!)
                    .Value(vm.Status.ExpectedPods));
    }
}
