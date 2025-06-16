using k8s.Models;

namespace KubeUI.Resources.Workloads.Configuration;

public sealed partial class V1PodDisruptionBudgetConfig : ResourceConfigBase<V1PodDisruptionBudget>
{
    public override string Category => "Configuration";
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1PodDisruptionBudget, IntstrIntOrString>()
            {
                Name = "Min Available",
                Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
                Field = x => x.Spec.MinAvailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, IntstrIntOrString>()
            {
                Name = "Max Unavailable",
                Display = x => x.Spec.MaxUnavailable != null ? x.Spec.MaxUnavailable.Value : "",
                Field = x => x.Spec.MaxUnavailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, int>()
            {
                Name = "Current Healthy",
                Display = x => x.Status.CurrentHealthy.ToString(),
                Field = x => x.Status.CurrentHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, int>()
            {
                Name = "Desired Healthy",
                Display = x => x.Status.DesiredHealthy.ToString(),
                Field = x => x.Status.DesiredHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }
}
