using k8s.Models;

namespace KubeUI.Resources.Configuration;

public sealed partial class V1PodDisruptionBudgetConfig : ResourceConfigBase<V1PodDisruptionBudget>
{
    public override bool IsNamespaced => true;
    public override string Category => "Configuration";
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1PodDisruptionBudget, IntOrString>()
            {
                Name = "Min Available",
                Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
                Field = x => x.Spec.MinAvailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, IntOrString>()
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
