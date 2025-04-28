using k8s.Models;

namespace KubeUI.Resources.Network;

public sealed partial class V2HorizontalPodAutoscalerConfig : ResourceConfigBase<V2HorizontalPodAutoscaler>
{
    public override string Category => "Network";
    public override int Order => 4;

    public override IList<IResourceListColumn> Columns()
    {
        return [
            NameColumn(SortDirection.Ascending),
            NamespaceColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Min Pods",
                Display = x => (x.Spec.MinReplicas ?? 0).ToString(),
                Field = x => x.Spec.MinReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Max Pods",
                Display = x => x.Spec.MaxReplicas.ToString(),
                Field = x => x.Spec.MaxReplicas,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            new ResourceListColumn<V2HorizontalPodAutoscaler, int>()
            {
                Name = "Replica",
                Display = x => (x.Status.CurrentReplicas ?? 0).ToString(),
                Field = x => x.Status.CurrentReplicas ?? 0,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
            AgeColumn(),
            new ResourceListColumn<V2HorizontalPodAutoscaler, string>()
            {
                Name = "Conditions",
                Display = x => x.Status.Conditions.FirstOrDefault(y => y.Status == "True").Type,
                Field = x => x.Status.Conditions.First(y => y.Status == "True").Type,
                Width = nameof(DataGridLengthUnitType.SizeToHeader)
            },
        ];
    }
}
