using k8s.Models;
using Scrutor;

namespace KubeUI.Resources.Workloads.Configuration;

[ServiceDescriptor<ResourceConfigBase<V1PodDisruptionBudget>>(ServiceLifetime.Transient)]
public sealed partial class V1PodDisruptionBudgetConfig : ResourceConfigBase<V1PodDisruptionBudget>
{
    public override string Category => "Configuration";
    public override int Order => 5;

    public override IList<IResourceListViewDefinitionColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
            {
                Name = "Min Avalilable",
                Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
                Field = x => x.Spec.MinAvailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, IntstrIntOrString>()
            {
                Name = "Max Unavailable",
                Display = x => x.Spec.MaxUnavailable != null ? x.Spec.MaxUnavailable.Value : "",
                Field = x => x.Spec.MaxUnavailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
            {
                Name = "Current Healthy",
                Display = x => x.Status.CurrentHealthy.ToString(),
                Field = x => x.Status.CurrentHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListViewDefinitionColumn<V1PodDisruptionBudget, int>()
            {
                Name = "Desired Healthy",
                Display = x => x.Status.DesiredHealthy.ToString(),
                Field = x => x.Status.DesiredHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override IList<ResourceListViewMenuItem> MenuItems()
    {
        return [

        ];
    }

    public override Control[] Properties(V1PodDisruptionBudget resource)
    {
        return null;
    }
}
