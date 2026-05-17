using Avalonia.Controls;
using k8s.Models;
using KubeUI.Avalonia.Resources.Configuration.v1.PodDisruptionBudget.Views;

namespace KubeUI.Avalonia.Resources.Configuration.v1.PodDisruptionBudget;

public sealed partial class V1PodDisruptionBudgetConfig : ResourceConfigBase<V1PodDisruptionBudget>
{
    public V1PodDisruptionBudgetConfig(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
    public override bool IsNamespaced => true;
    public override string Category => CategoryString("ResourceConfig_Category_Configuration", "Configuration");
    public override int Order => 5;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V1PodDisruptionBudget, IntOrString>()
            {
                Key = "min-available",
                Name = Assets.Resources.V1PodDisruptionBudgetConfig_Min_Available!,
                Display = x => x.Spec.MinAvailable != null ? x.Spec.MinAvailable.Value : "",
                Field = x => x.Spec.MinAvailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, IntOrString>()
            {
                Key = "max-unavailable",
                Name = Assets.Resources.V1PodDisruptionBudgetConfig_Max_Unavailable!,
                Display = x => x.Spec.MaxUnavailable != null ? x.Spec.MaxUnavailable.Value : "",
                Field = x => x.Spec.MaxUnavailable,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, int>()
            {
                Key = "current-healthy",
                Name = Assets.Resources.V1PodDisruptionBudgetConfig_Current_Healthy!,
                Field = x => x.Status.CurrentHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V1PodDisruptionBudget, int>()
            {
                Key = "desired-healthy",
                Name = Assets.Resources.V1PodDisruptionBudgetConfig_Desired_Healthy!,
                Field = x => x.Status.DesiredHealthy,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
        ];
    }

    public override Control[] Properties(V1PodDisruptionBudget resource) => [new PropertiesView()];
}

